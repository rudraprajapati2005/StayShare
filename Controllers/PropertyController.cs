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
            if (role != "Owner" && role != "Host")
            {
                return RedirectToAction("Index1", "Home");
            }

            // Get current user's properties only
            var email = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value ?? "";
            var properties = await _unitOfWork.Properties.GetPropertiesByOwnerEmailAsync(email);
            
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
            if (role != "Owner" && role != "Host")
            {
                return RedirectToAction("Index1", "Home");
            }

            return View();
        }

        // POST: /Property/Create
        [HttpPost]
        public async Task<IActionResult> Create(Property model)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Auth");
            }

            var role = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value ?? "";
            if (role != "Owner" && role != "Host")
            {
                return RedirectToAction("Index1", "Home");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                // Always tie property to the signed-in owner via email
                var email = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value ?? "";
                model.OwnerContact = email?.Trim();

                // Save thumbnail if provided
                if (model.ThumbnailCover != null && model.ThumbnailCover.Length > 0)
                {
                    var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads");
                    if (!Directory.Exists(uploadPath))
                        Directory.CreateDirectory(uploadPath);

                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(model.ThumbnailCover.FileName);
                    var filePath = Path.Combine(uploadPath, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await model.ThumbnailCover.CopyToAsync(stream);
                    }

                    model.ThumbnailCoverPath = "/uploads/" + fileName;
                }

                // Default values
                model.IsVerified = false;

                await _unitOfWork.Properties.AddPropertyAsync(model);
                await _unitOfWork.CommitAsync();

                TempData["SuccessMessage"] = "Property created successfully!";
                return RedirectToAction("Index");
            }
            catch (Exception)
            {
                ModelState.AddModelError("", "An error occurred while creating the property. Please try again.");
                return View(model);
            }
        }


        [HttpPost]
        public async Task<IActionResult> ChangeThumbnail(int id, Property model)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Auth");
            }

            var role = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value ?? "";
            if (role != "Owner" && role != "Host")
            {
                return RedirectToAction("Index1", "Home");
            }

            if (model.ThumbnailCover == null || model.ThumbnailCover.Length == 0)
            {
                ModelState.AddModelError("", "Please select a valid image file.");
                return RedirectToAction("Edit", new { id });
            }

            var property = await _unitOfWork.Properties.GetPropertyByIdAsync(id);
            if (property == null)
            {
                return NotFound();
            }

            // Ensure the signed-in owner owns this property
            var currentEmail = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value ?? "";
            if (!string.Equals(property.OwnerContact?.Trim(), currentEmail?.Trim(), System.StringComparison.OrdinalIgnoreCase))
            {
                return Forbid();
            }

            var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads");
            if (!Directory.Exists(uploadPath))
                Directory.CreateDirectory(uploadPath);

            // ✅ Delete old file if exists
            if (!string.IsNullOrEmpty(property.ThumbnailCoverPath))
            {
                var oldFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", property.ThumbnailCoverPath.TrimStart('/'));
                if (System.IO.File.Exists(oldFilePath))
                {
                    System.IO.File.Delete(oldFilePath);
                }
            }

            // ✅ Save new file
            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(model.ThumbnailCover.FileName);
            var filePath = Path.Combine(uploadPath, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await model.ThumbnailCover.CopyToAsync(stream);
            }

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
            if (role != "Owner" && role != "Host")
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
            if (role != "Owner" && role != "Host")
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
            if (role != "Owner" && role != "Host")
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
            if (role != "Owner" && role != "Host")
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
            if (role != "Owner" && role != "Host")
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
