using Microsoft.AspNetCore.Mvc;
using StayShare.Models;
using StayShare.Repositories;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Collections.Generic;
namespace StayShare.Controllers
{
    public class BookingController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public BookingController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
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

            await _unitOfWork.Bookings.AddAsync(model);
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
            var list = await _unitOfWork.Bookings.GetByResidentAsync(user.UserId);
            return View(list);
        }

        // GET: /Booking/MyBookings
        public async Task<IActionResult> MyBookings(string filter = "all")
        {
            if (!User.Identity.IsAuthenticated) return RedirectToAction("Login", "Auth");
            var role = User.FindFirst(ClaimTypes.Role)?.Value ?? "";
            if (!string.Equals(role, "Resident", StringComparison.OrdinalIgnoreCase))
            {
                return RedirectToAction("Index1", "Home");
            }

            var email = User.FindFirst(ClaimTypes.Email)?.Value ?? "";
            var user = await _unitOfWork.Users.GetUserByEmailAsync(email);
            if (user == null) return RedirectToAction("Login", "Auth");

            // Get booking requests
            var bookingRequests = await _unitOfWork.Bookings.GetByResidentAsync(user.UserId);
            
            // Filter booking requests
            IEnumerable<BookingRequest> filteredRequests = bookingRequests;
            switch (filter.ToLower())
            {
                case "pending":
                    filteredRequests = bookingRequests?.Where(r => r.Status == BookingStatus.Pending);
                    break;
                case "accepted":
                    filteredRequests = bookingRequests?.Where(r => r.Status == BookingStatus.Accepted);
                    break;
                case "declined":
                    filteredRequests = bookingRequests?.Where(r => r.Status == BookingStatus.Declined);
                    break;
                default:
                    filteredRequests = bookingRequests;
                    break;
            }

            // Get occupancies (current and past stays)
            var occupancies = await _unitOfWork.Occupancies.GetOccupanciesByUserIdAsync(user.UserId);
            var currentStay = occupancies?.FirstOrDefault(o => o.IsActive && o.JoinedAt.HasValue && o.JoinedAt.Value <= DateTime.UtcNow);
            var upcomingStay = occupancies?.FirstOrDefault(o => o.Status == OccupancyStatus.Accepted && o.JoinedAt.HasValue && o.JoinedAt.Value > DateTime.UtcNow);
            var pastStays = occupancies?.Where(o => !o.IsActive && (o.Status == OccupancyStatus.Left || o.ExitDate.HasValue))
                .OrderByDescending(o => o.ExitDate ?? o.JoinedAt)
                .ToList();

            // Get roommates for current stay
            var roommates = new List<User>();
            if (currentStay != null)
            {
                var roomOccupancies = await _unitOfWork.Occupancies.GetOccupanciesByRoomIdAsync(currentStay.RoomId);
                roommates = roomOccupancies?.Where(o => o.IsActive && o.UserId != user.UserId && o.JoinedAt.HasValue && o.JoinedAt.Value <= DateTime.UtcNow)
                    .Select(o => o.User)
                    .Where(u => u != null)
                    .ToList() ?? new List<User>();
            }

            // Pass data to view
            ViewBag.BookingRequests = filteredRequests?.ToList() ?? new List<BookingRequest>();
            ViewBag.CurrentStay = currentStay;
            ViewBag.UpcomingStay = upcomingStay;
            ViewBag.PastStays = pastStays ?? new List<RoomOccupancy>();
            ViewBag.Roommates = roommates;
            ViewBag.Filter = filter;
            ViewBag.PendingCount = bookingRequests?.Count(r => r.Status == BookingStatus.Pending) ?? 0;
            ViewBag.AcceptedCount = bookingRequests?.Count(r => r.Status == BookingStatus.Accepted) ?? 0;
            ViewBag.DeclinedCount = bookingRequests?.Count(r => r.Status == BookingStatus.Declined) ?? 0;
            ViewBag.TotalCount = bookingRequests?.Count() ?? 0;

            return View();
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
            var list = await _unitOfWork.Bookings.GetByHostAsync(email);
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

            var req = await _unitOfWork.Bookings.GetByIdAsync(id);
            if (req == null) return NotFound();
            if (!string.Equals(req.Room.Property.OwnerContact?.Trim(), email?.Trim(), StringComparison.OrdinalIgnoreCase)) return Forbid();

            // Date range overlap and capacity check
            var room = await _unitOfWork.Rooms.GetRoomByIdAsync(req.RoomId);
            var existingOccupancies = await _unitOfWork.Occupancies.GetOccupanciesByRoomIdAsync(req.RoomId);
            
            // Calculate the end date for this new request
            var newStartDate = req.MoveInDate;
            var newEndDate = req.MoveInDate.AddMonths(req.Months);
            
            // Check for date overlaps with existing accepted/active occupancies
            var overlappingOccupancies = existingOccupancies?.Where(o => 
                (o.IsActive || o.Status == OccupancyStatus.Accepted) && 
                o.JoinedAt.HasValue &&
                // Check if date ranges overlap: (StartA <= EndB) && (EndA >= StartB)
                (o.JoinedAt.Value < newEndDate) && 
                (GetOccupancyEndDate(o) > newStartDate)
            ).ToList() ?? new List<RoomOccupancy>();
            
            // Count unique overlapping occupants
            var overlappingCount = overlappingOccupancies.Count();
            
            // Check if adding this person would exceed capacity during the overlap period
            if (overlappingCount >= room.Capacity)
            {
                var conflictDates = string.Join(", ", overlappingOccupancies.Take(3).Select(o => o.JoinedAt?.ToString("yyyy-MM-dd")));
                TempData["ErrorMessage"] = $"Room capacity conflict. Existing bookings overlap with requested dates ({conflictDates}...).";
                return RedirectToAction("Incoming");
            }

            // Accept → create occupancy
            await _unitOfWork.Occupancies.AddOccupancyAsync(new RoomOccupancy
            {
                RoomId = req.RoomId,
                UserId = req.UserId,
                RequestedAt = req.CreatedAt,
                JoinedAt = req.MoveInDate,
                ExitDate = req.MoveInDate.AddMonths(req.Months), // Store planned exit date
                IsActive = req.MoveInDate <= DateTime.UtcNow, // Only active if move-in date is today or past
                Status = OccupancyStatus.Accepted
            });

            req.Status = BookingStatus.Accepted;
            req.Note = $"Your request for room #{room.RoomNumber} at {room.Property?.Name} was accepted.";
            await _unitOfWork.Bookings.UpdateAsync(req);
            await _unitOfWork.CommitAsync();

            // Update room availability based on new occupancy
            await UpdateRoomAvailabilityAsync(req.RoomId);

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
            var list = await _unitOfWork.Bookings.GetByHostAsync(email);
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

            var req = await _unitOfWork.Bookings.GetByIdAsync(id);
            if (req == null) return NotFound();
            if (!string.Equals(req.Room.Property.OwnerContact?.Trim(), email?.Trim(), StringComparison.OrdinalIgnoreCase)) return Forbid();

            req.Status = BookingStatus.Declined;
            req.Note = $"Your request for room #{req.Room?.RoomNumber} at {req.Room?.Property?.Name} was declined.";
            await _unitOfWork.Bookings.UpdateAsync(req);
            await _unitOfWork.CommitAsync();

            TempData["SuccessMessage"] = "Booking declined.";
            return RedirectToAction("Incoming");
        }

        // Helper method to update room availability based on current + future occupancy
        private async Task UpdateRoomAvailabilityAsync(int roomId)
        {
            var room = await _unitOfWork.Rooms.GetRoomByIdAsync(roomId);
            if (room != null)
            {
                var occupancies = await _unitOfWork.Occupancies.GetOccupanciesByRoomIdAsync(roomId);
                
                // Count current occupants (already moved in)
                var currentOccupants = occupancies?.Count(o => o.IsActive && o.JoinedAt.HasValue && o.JoinedAt.Value <= DateTime.UtcNow) ?? 0;
                
                // Count future accepted bookings (will move in later)
                var futureOccupants = occupancies?.Count(o => o.Status == OccupancyStatus.Accepted && o.JoinedAt.HasValue && o.JoinedAt.Value > DateTime.UtcNow) ?? 0;
                
                // Total committed occupants = current + future
                var totalCommittedOccupants = currentOccupants + futureOccupants;
                
                // Room is available if total committed occupants < capacity
                room.IsAvailable = totalCommittedOccupants < room.Capacity;
                
                await _unitOfWork.Rooms.UpdateRoomAsync(room);
                await _unitOfWork.CommitAsync();
            }
        }

        // Helper method to get occupancy end date
        private DateTime GetOccupancyEndDate(RoomOccupancy occupancy)
        {
            // If ExitDate is set, use it
            if (occupancy.ExitDate.HasValue)
            {
                return occupancy.ExitDate.Value;
            }
            
            // For legacy data without ExitDate, estimate based on JoinedAt + reasonable duration
            // This is a fallback - ideally all new occupancies should have ExitDate set
            if (occupancy.JoinedAt.HasValue)
            {
                return occupancy.JoinedAt.Value.AddMonths(12); // Default to 12 months for legacy data
            }
            
            // Last resort fallback
            return DateTime.UtcNow.AddMonths(12);
        }
    }
}



