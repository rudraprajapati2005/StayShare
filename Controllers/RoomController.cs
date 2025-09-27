using Microsoft.AspNetCore.Mvc;
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

        // GET: /Room/Create?propertyId=5
        [HttpGet]
        public async Task<IActionResult> Create(int propertyId)
        {
            // Check if user is authenticated and has owner role
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Auth");
            }

            var role = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value ?? "";
            if (role != "Owner")
            {
                return RedirectToAction("Index1", "Home");
            }

            // Get the property to ensure it exists
            var property = await _unitOfWork.Properties.GetPropertyByIdAsync(propertyId);
            if (property == null)
            {
                return NotFound();
            }

            ViewBag.PropertyId = propertyId;
            ViewBag.PropertyName = property.Name;
            return View();
        }

        // POST: /Room/Create
        [HttpPost]
        public async Task<IActionResult> Create(Room model, int propertyId)
        {
            // Check if user is authenticated and has owner role
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Auth");
            }

            var role = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value ?? "";
            if (role != "Owner")
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
                // Set the property ID
                model.PropertyId = propertyId;

                await _unitOfWork.Rooms.AddRoomAsync(model);
                await _unitOfWork.CommitAsync();

                TempData["SuccessMessage"] = "Room created successfully!";
                return RedirectToAction("Details", "Property", new { id = propertyId });
            }
            catch (System.Exception ex)
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
            if (role != "Owner")
            {
                return RedirectToAction("Index1", "Home");
            }

            var room = await _unitOfWork.Rooms.GetRoomByIdAsync(id);
            if (room == null)
            {
                return NotFound();
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
            if (role != "Owner")
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
                // Get existing room to preserve property relationship
                var existingRoom = await _unitOfWork.Rooms.GetRoomByIdAsync(id);
                if (existingRoom == null)
                {
                    return NotFound();
                }

                // Update room details
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
            catch (System.Exception ex)
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
            if (role != "Owner")
            {
                return RedirectToAction("Index1", "Home");
            }

            var room = await _unitOfWork.Rooms.GetRoomByIdAsync(id);
            if (room == null)
            {
                return NotFound();
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
            if (role != "Owner")
            {
                return RedirectToAction("Index1", "Home");
            }

            try
            {
                var room = await _unitOfWork.Rooms.GetRoomByIdAsync(id);
                if (room != null)
                {
                    await _unitOfWork.Rooms.DeleteRoomAsync(id);
                    await _unitOfWork.CommitAsync();
                    TempData["SuccessMessage"] = "Room deleted successfully!";
                }
                return RedirectToAction("Details", "Property", new { id = room?.PropertyId });
            }
            catch (System.Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while deleting the room.";
                return RedirectToAction("Index", "Property");
            }
        }
    }
}
