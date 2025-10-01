using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using StayShare.Models;
using StayShare.Repositories;
using System.Threading.Tasks;

namespace StayShare.Controllers
{
    public class RoomController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public RoomController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // GET: /Room/Residents/5
        [HttpGet("Room/Residents/{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> Residents(int id)
        {
            var room = await _unitOfWork.Rooms.GetRoomByIdAsync(id);
            if (room == null)
            {
                return NotFound();
            }

            var occupancies = await _unitOfWork.Occupancies.GetOccupanciesByRoomIdAsync(id);
            var current = occupancies?.Where(o => o.IsActive).ToList() ?? new System.Collections.Generic.List<StayShare.Models.RoomOccupancy>();
            var upcoming = occupancies?.Where(o => !o.IsActive && o.Status == StayShare.Models.OccupancyStatus.Accepted && o.JoinedAt.HasValue && o.JoinedAt.Value > System.DateTime.UtcNow)
                .OrderBy(o => o.JoinedAt)
                .ToList() ?? new System.Collections.Generic.List<StayShare.Models.RoomOccupancy>();
            var past = occupancies?.Where(o => !o.IsActive && (o.Status == StayShare.Models.OccupancyStatus.Left || o.Status == StayShare.Models.OccupancyStatus.Rejected))
                .OrderByDescending(o => o.ExitDate ?? o.JoinedAt)
                .ToList() ?? new System.Collections.Generic.List<StayShare.Models.RoomOccupancy>();

            ViewBag.Room = room;
            ViewBag.Current = current;
            ViewBag.Upcoming = upcoming;
            ViewBag.Past = past;

            return View();
        }

        // GET: /Room/Create?propertyId=5
        [HttpGet]
        public async Task<IActionResult> Create(int propertyId)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Auth");
            }

            var role = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value ?? "";
            if (!(role == "Host" || role == "Owner"))
            {
                return RedirectToAction("Index1", "Home");
            }

            var property = await _unitOfWork.Properties.GetPropertyByIdAsync(propertyId);
            if (property == null)
            {
                return NotFound();
            }

            // Ensure ownership
            var email = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value ?? "";
            if (!string.Equals(property.OwnerContact?.Trim(), email?.Trim(), System.StringComparison.OrdinalIgnoreCase))
            {
                return Forbid();
            }

            ViewBag.PropertyId = propertyId;
            ViewBag.PropertyName = property.Name;
            return View();
        }

        // POST: /Room/Create
        [HttpPost]
        public async Task<IActionResult> Create(Room model, int propertyId)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Auth");
            }

            var role = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value ?? "";
            if (!(role == "Host" || role == "Owner"))
            {
                return RedirectToAction("Index1", "Home");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.PropertyId = propertyId;
                var property = await _unitOfWork.Properties.GetPropertyByIdAsync(propertyId);
                ViewBag.PropertyName = property?.Name;
                return View(model);
            }

            try
            {
                // Ensure property exists and owned by current user
                var property = await _unitOfWork.Properties.GetPropertyByIdAsync(propertyId);
                if (property == null)
                {
                    return NotFound();
                }
                var email = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value ?? "";
                if (!string.Equals(property.OwnerContact?.Trim(), email?.Trim(), System.StringComparison.OrdinalIgnoreCase))
                {
                    return Forbid();
                }

                model.PropertyId = propertyId;

                await _unitOfWork.Rooms.AddRoomAsync(model);
                await _unitOfWork.CommitAsync();

                TempData["SuccessMessage"] = "Room created successfully!";
                return RedirectToAction("Details", "Property", new { id = propertyId });
            }
            catch
            {
                ModelState.AddModelError("", "An error occurred while creating the room. Please try again.");
                ViewBag.PropertyId = propertyId;
                var property = await _unitOfWork.Properties.GetPropertyByIdAsync(propertyId);
                ViewBag.PropertyName = property?.Name;
                return View(model);
            }
        }

        // GET: /Room/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Auth");
            }

            var role = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value ?? "";
            if (!(role == "Host" || role == "Owner"))
            {
                return RedirectToAction("Index1", "Home");
            }

            var room = await _unitOfWork.Rooms.GetRoomByIdAsync(id);
            if (room == null)
            {
                return NotFound();
            }

            // Ensure ownership of room's property
            var property = await _unitOfWork.Properties.GetPropertyByIdAsync(room.PropertyId);
            var email = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value ?? "";
            if (!string.Equals(property?.OwnerContact?.Trim(), email?.Trim(), System.StringComparison.OrdinalIgnoreCase))
            {
                return Forbid();
            }

            return View(room);
        }

        // POST: /Room/Edit/5
        [HttpPost]
        public async Task<IActionResult> Edit(int id, Room model)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Auth");
            }

            var role = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value ?? "";
            if (!(role == "Host" || role == "Owner"))
            {
                return RedirectToAction("Index1", "Home");
            }

            if (id != model.RoomId)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var existingRoom = await _unitOfWork.Rooms.GetRoomByIdAsync(id);
                if (existingRoom == null)
                {
                    return NotFound();
                }

                // Ensure ownership
                var property = await _unitOfWork.Properties.GetPropertyByIdAsync(existingRoom.PropertyId);
                var email = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value ?? "";
                if (!string.Equals(property?.OwnerContact?.Trim(), email?.Trim(), System.StringComparison.OrdinalIgnoreCase))
                {
                    return Forbid();
                }

                existingRoom.RoomNumber = model.RoomNumber;
                existingRoom.RoomType = model.RoomType;
                existingRoom.RentPerMonth = model.RentPerMonth;
                existingRoom.Capacity = model.Capacity;
                existingRoom.Amenities = model.Amenities;
                existingRoom.IsAvailable = model.IsAvailable;

                await _unitOfWork.Rooms.UpdateRoomAsync(existingRoom);
                await _unitOfWork.CommitAsync();

                TempData["SuccessMessage"] = "Room updated successfully!";
                return RedirectToAction("Details", "Property", new { id = existingRoom.PropertyId });
            }
            catch
            {
                ModelState.AddModelError("", "An error occurred while updating the room. Please try again.");
                return View(model);
            }
        }

        // GET: /Room/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Auth");
            }

            var role = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value ?? "";
            if (!(role == "Host" || role == "Owner"))
            {
                return RedirectToAction("Index1", "Home");
            }

            var room = await _unitOfWork.Rooms.GetRoomByIdAsync(id);
            if (room == null)
            {
                return NotFound();
            }

            // Ensure ownership
            var property = await _unitOfWork.Properties.GetPropertyByIdAsync(room.PropertyId);
            var email = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value ?? "";
            if (!string.Equals(property?.OwnerContact?.Trim(), email?.Trim(), System.StringComparison.OrdinalIgnoreCase))
            {
                return Forbid();
            }

            return View(room);
        }

        // POST: /Room/Delete/5
        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Auth");
            }

            var role = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value ?? "";
            if (!(role == "Host" || role == "Owner"))
            {
                return RedirectToAction("Index1", "Home");
            }

            try
            {
                var room = await _unitOfWork.Rooms.GetRoomByIdAsync(id);
                if (room != null)
                {
                    // Ensure ownership
                    var property = await _unitOfWork.Properties.GetPropertyByIdAsync(room.PropertyId);
                    var email = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value ?? "";
                    if (!string.Equals(property?.OwnerContact?.Trim(), email?.Trim(), System.StringComparison.OrdinalIgnoreCase))
                    {
                        return Forbid();
                    }

                    await _unitOfWork.Rooms.DeleteRoomAsync(id);
                    await _unitOfWork.CommitAsync();
                    TempData["SuccessMessage"] = "Room deleted successfully!";
                }
                return RedirectToAction("Details", "Property", new { id = room?.PropertyId });
            }
            catch
            {
                TempData["ErrorMessage"] = "An error occurred while deleting the room.";
                return RedirectToAction("Index", "Property");
            }
        }
    }
}
