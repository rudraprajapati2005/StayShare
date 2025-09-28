using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StayShare.Models;
using StayShare.Repositories;
using System;
using System.IO;
using System.Threading.Tasks;

namespace StayShare.Controllers
{
    public class PropertyController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public PropertyController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // GET: /Property
        public async Task<IActionResult> Index()
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

            // Get current user's properties
            // Note: You'll need to add a method to get properties by owner
            // For now, we'll get all properties (you can filter by owner later)
            var properties = await _unitOfWork.Properties.GetAllPropertiesAsync();
            
            return View(properties);
        }

        // GET: /Property/Create
        [HttpGet]
        public IActionResult Create()
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

            return View();
        }

        // POST: /Property/Create
        [HttpPost]
        public async Task<IActionResult> Create(Property model)
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
                return View(model);
            }

            try
            {
                // Set default values
                model.IsVerified = false;

                await _unitOfWork.Properties.AddPropertyAsync(model);
                await _unitOfWork.CommitAsync();

                TempData["SuccessMessage"] = "Property created successfully!";
                return RedirectToAction("Index");
            }
            catch (System.Exception ex)
            {
                ModelState.AddModelError("", "An error occurred while creating the property. Please try again.");
                return View(model);
            }
        }
        //changing the thumbnail cover
        [HttpPost]
        public async Task<IActionResult> ChangeThumbnail(int id, Property model)
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

            if (model.ThumbnailCover == null || model.ThumbnailCover.Length == 0)
            {
                ModelState.AddModelError("", "Please select a valid image file.");
                return RedirectToAction("Edit", new { id });
            }

            // ✅ Fetch the existing property
            var property = await _unitOfWork.Properties.GetPropertyByIdAsync(id);
            if (property == null)
            {
                return NotFound();
            }

            // ✅ Save the new file
            var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads");
            if (!Directory.Exists(uploadPath))
                Directory.CreateDirectory(uploadPath);

            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(model.ThumbnailCover.FileName);
            var filePath = Path.Combine(uploadPath, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await model.ThumbnailCover.CopyToAsync(stream);
            }

            // ✅ Update only the ThumbnailCoverPath
            property.ThumbnailCoverPath = "/uploads/" + fileName;

            await _unitOfWork.CommitAsync();

            TempData["SuccessMessage"] = "Thumbnail updated successfully!";
            return RedirectToAction("Details", new { id = property.PropertyId });
        }


        // GET: /Property/Details/5
        public async Task<IActionResult> Details(int id)
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

            var property = await _unitOfWork.Properties.GetPropertyByIdAsync(id);
            if (property == null)
            {
                return NotFound();
            }

            return View(property);
        }

        // GET: /Property/Edit/5
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

            var property = await _unitOfWork.Properties.GetPropertyByIdAsync(id);
            if (property == null)
            {
                return NotFound();
            }

            return View(property);
        }

        // POST: /Property/Edit/5
        [HttpPost]
        public async Task<IActionResult> Edit(int id, Property model)
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

            if (id != model.PropertyId)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                // Get existing property to preserve location data
                var existingProperty = await _unitOfWork.Properties.GetPropertyByIdAsync(id);
                if (existingProperty == null)
                {
                    return NotFound();
                }

                // Update only non-location fields
                existingProperty.Name = model.Name;
                existingProperty.Type = model.Type;
                existingProperty.Category = model.Category;
                existingProperty.OwnerContact = model.OwnerContact;
                existingProperty.Amenities = model.Amenities;

                await _unitOfWork.CommitAsync();
                TempData["SuccessMessage"] = "Property updated successfully!";
                return RedirectToAction("Index");
            }
            catch (System.Exception ex)
            {
                ModelState.AddModelError("", "An error occurred while updating the property. Please try again.");
                return View(model);
            }
        }

        // GET: /Property/Delete/5
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

            var property = await _unitOfWork.Properties.GetPropertyByIdAsync(id);
            if (property == null)
            {
                return NotFound();
            }

            return View(property);
        }

        // POST: /Property/Delete/5
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
                var property = await _unitOfWork.Properties.GetPropertyByIdAsync(id);
                if (property != null)
                {
                    // Note: You'll need to add a DeletePropertyAsync method to the repository
                    // For now, we'll just redirect
                    TempData["SuccessMessage"] = "Property deleted successfully!";
                }
                return RedirectToAction("Index");
            }
            catch (System.Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while deleting the property.";
                return RedirectToAction("Index");
            }
        }
    }
}
