using Microsoft.AspNetCore.Mvc;
using StayShare.Models;
using StayShare.Repositories;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace StayShare.Controllers
{
    public class BookingController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IBookingRepository _bookings;

        public BookingController(IUnitOfWork unitOfWork, IBookingRepository bookings)
        {
            _unitOfWork = unitOfWork;
            _bookings = bookings;
        }

        // GET: /Booking/Create?roomId=123
        public async Task<IActionResult> Create(int roomId)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Auth");
            }

            var role = User.FindFirst(ClaimTypes.Role)?.Value ?? "";
            if (!string.Equals(role, "Resident", StringComparison.OrdinalIgnoreCase))
            {
                return RedirectToAction("Index1", "Home");
            }

            var room = await _unitOfWork.Rooms.GetRoomByIdAsync(roomId);
            if (room == null || room.Property == null)
            {
                return NotFound();
            }
            ViewBag.Room = room;
            return View(new BookingRequest { RoomId = roomId, MoveInDate = DateTime.UtcNow.Date.AddDays(3), Months = 6 });
        }

        // POST: /Booking/Create
        [HttpPost]
        public async Task<IActionResult> Create(BookingRequest model)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Auth");
            }

            var role = User.FindFirst(ClaimTypes.Role)?.Value ?? "";
            if (!string.Equals(role, "Resident", StringComparison.OrdinalIgnoreCase))
            {
                return RedirectToAction("Index1", "Home");
            }

            if (!ModelState.IsValid)
            {
                var room = await _unitOfWork.Rooms.GetRoomByIdAsync(model.RoomId);
                ViewBag.Room = room;
                return View(model);
            }

            var userEmail = User.FindFirst(ClaimTypes.Email)?.Value ?? "";
            var user = await _unitOfWork.Users.GetUserByEmailAsync(userEmail);
            if (user == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            model.UserId = user.UserId;
            model.Status = BookingStatus.Pending;
            model.CreatedAt = DateTime.UtcNow;
            model.ExpiresAt = DateTime.UtcNow.AddHours(24);

            await _bookings.AddAsync(model);
            await _unitOfWork.CommitAsync();

            TempData["SuccessMessage"] = "Request sent to host.";
            return RedirectToAction("MyRequests");
        }

        // GET: /Booking/MyRequests
        public async Task<IActionResult> MyRequests()
        {
            if (!User.Identity.IsAuthenticated) return RedirectToAction("Login", "Auth");
            var email = User.FindFirst(ClaimTypes.Email)?.Value ?? "";
            var user = await _unitOfWork.Users.GetUserByEmailAsync(email);
            if (user == null) return RedirectToAction("Login", "Auth");
            var list = await _bookings.GetByResidentAsync(user.UserId);
            return View(list);
        }

        // GET: /Booking/Incoming
        public async Task<IActionResult> Incoming(int? propertyId)
        {
            if (!User.Identity.IsAuthenticated) return RedirectToAction("Login", "Auth");
            var role = User.FindFirst(ClaimTypes.Role)?.Value ?? "";
            if (!(string.Equals(role, "Host", StringComparison.OrdinalIgnoreCase) || string.Equals(role, "Owner", StringComparison.OrdinalIgnoreCase)))
            {
                return RedirectToAction("Index1", "Home");
            }
            var email = User.FindFirst(ClaimTypes.Email)?.Value ?? "";
            var list = await _bookings.GetByHostAsync(email);
            if (propertyId.HasValue)
            {
                list = list?.Where(r => r.Room != null && r.Room.Property != null && r.Room.Property.PropertyId == propertyId.Value);
                ViewBag.PropertyId = propertyId.Value;
            }
            return View(list);
        }

        // POST: /Booking/Approve/5
        [HttpPost]
        public async Task<IActionResult> Approve(int id)
        {
            if (!User.Identity.IsAuthenticated) return RedirectToAction("Login", "Auth");
            var email = User.FindFirst(ClaimTypes.Email)?.Value ?? "";
            var role = User.FindFirst(ClaimTypes.Role)?.Value ?? "";
            if (!(string.Equals(role, "Host", StringComparison.OrdinalIgnoreCase) || string.Equals(role, "Owner", StringComparison.OrdinalIgnoreCase)))
            {
                return RedirectToAction("Index1", "Home");
            }

            var req = await _bookings.GetByIdAsync(id);
            if (req == null) return NotFound();
            if (!string.Equals(req.Room.Property.OwnerContact?.Trim(), email?.Trim(), StringComparison.OrdinalIgnoreCase)) return Forbid();

            // Capacity check
            var room = await _unitOfWork.Rooms.GetRoomByIdAsync(req.RoomId);
            var occupants = (await _unitOfWork.Occupancies.GetOccupanciesByRoomIdAsync(req.RoomId))?.Count(o => o.IsActive) ?? 0;
            if (occupants >= room.Capacity)
            {
                TempData["ErrorMessage"] = "Room is full.";
                return RedirectToAction("Incoming");
            }

            // Accept → create occupancy
            await _unitOfWork.Occupancies.AddOccupancyAsync(new RoomOccupancy
            {
                RoomId = req.RoomId,
                UserId = req.UserId,
                RequestedAt = req.CreatedAt,
                JoinedAt = req.MoveInDate,
                IsActive = true,
                Status = OccupancyStatus.Accepted
            });

            req.Status = BookingStatus.Accepted;
            req.Note = $"Your request for room #{room.RoomNumber} at {room.Property?.Name} was accepted.";
            await _bookings.UpdateAsync(req);
            await _unitOfWork.CommitAsync();

            TempData["SuccessMessage"] = "Booking approved.";
            return RedirectToAction("Incoming");
        }

        // GET: /Booking/PendingCount
        [HttpGet]
        public async Task<IActionResult> PendingCount()
        {
            if (!User.Identity.IsAuthenticated) return Json(new { count = 0 });
            var role = User.FindFirst(ClaimTypes.Role)?.Value ?? "";
            if (!(string.Equals(role, "Host", StringComparison.OrdinalIgnoreCase) || string.Equals(role, "Owner", StringComparison.OrdinalIgnoreCase)))
            {
                return Json(new { count = 0 });
            }

            var email = User.FindFirst(ClaimTypes.Email)?.Value ?? "";
            var list = await _bookings.GetByHostAsync(email);
            var pendingCount = list?.Count(r => r.Status == BookingStatus.Pending) ?? 0;
            return Json(new { count = pendingCount });
        }

        // POST: /Booking/Decline/5
        [HttpPost]
        public async Task<IActionResult> Decline(int id)
        {
            if (!User.Identity.IsAuthenticated) return RedirectToAction("Login", "Auth");
            var email = User.FindFirst(ClaimTypes.Email)?.Value ?? "";
            var role = User.FindFirst(ClaimTypes.Role)?.Value ?? "";
            if (!(string.Equals(role, "Host", StringComparison.OrdinalIgnoreCase) || string.Equals(role, "Owner", StringComparison.OrdinalIgnoreCase)))
            {
                return RedirectToAction("Index1", "Home");
            }

            var req = await _bookings.GetByIdAsync(id);
            if (req == null) return NotFound();
            if (!string.Equals(req.Room.Property.OwnerContact?.Trim(), email?.Trim(), StringComparison.OrdinalIgnoreCase)) return Forbid();

            req.Status = BookingStatus.Declined;
            req.Note = $"Your request for room #{req.Room?.RoomNumber} at {req.Room?.Property?.Name} was declined.";
            await _bookings.UpdateAsync(req);
            await _unitOfWork.CommitAsync();

            TempData["SuccessMessage"] = "Booking declined.";
            return RedirectToAction("Incoming");
        }
    }
}



