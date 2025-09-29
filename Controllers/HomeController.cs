using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using StayShare.Models;
using StayShare.Repositories;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims;
using Microsoft.AspNetCore.Hosting;
using System.IO;

namespace StayShare.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public HomeController(ILogger<HomeController> logger, IUnitOfWork unitOfWork, IWebHostEnvironment webHostEnvironment)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
            _webHostEnvironment = webHostEnvironment;
        }

        public IActionResult Index()
        {
            if (User.Identity.IsAuthenticated)
            {
                // User is logged in, redirect to Dashboard
                return RedirectToAction("Index1");
            }

            // Otherwise, show the default Home/Index view
            return View();
        }
        public async Task<IActionResult> Index1()
        {
            if (!User.Identity.IsAuthenticated)
            {
                // If user is not authenticated, redirect to home or login
                return RedirectToAction("Index");
            }

            // Retrieve user data from claims
            var fullName = User.FindFirst(ClaimTypes.Name)?.Value ?? "Guest";
            var email = User.FindFirst(ClaimTypes.Email)?.Value ?? "";
            var role = User.FindFirst(ClaimTypes.Role)?.Value ?? "";

            // Get user profile data
            var user = await _unitOfWork.Users.GetUserByEmailAsync(email);
            UserProfile profile = null;
            
            if (user != null)
            {
                profile = user.Profile;
            }
            else
            {
                // If user not found in database, create a basic user object from claims
                user = new User
                {
                    FullName = fullName,
                    Email = email,
                    Role = role,
                    CreatedAt = DateTime.UtcNow
                };
            }

            // Debug information
            TempData["DebugInfo"] = $"Email: {email}, User Found: {user != null}, Profile Found: {profile != null}, FullName: {fullName}, Role: {role}, CreatedAt: {user?.CreatedAt}";

            // Pass data to the view using ViewBag
            ViewBag.FullName = fullName;
            ViewBag.Email = email;
            ViewBag.Role = role;
            ViewBag.User = user;
            ViewBag.Profile = profile;
            ViewBag.UserCreatedAt = user?.CreatedAt;

            return View();
        }

        // GET: Profile/Edit
        public async Task<IActionResult> EditProfile()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index");
            }

            var email = User.FindFirst(ClaimTypes.Email)?.Value ?? "";
            var user = await _unitOfWork.Users.GetUserByEmailAsync(email);
            
            if (user == null)
            {
                return RedirectToAction("Index1");
            }

            // Create profile if it doesn't exist
            if (user.Profile == null)
            {
                user.Profile = new UserProfile
                {
                    UserId = user.UserId,
                    CreatedAt = DateTime.UtcNow
                };
            }

            return View(user);
        }

        // POST: Profile/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProfile(User model, IFormFile profileImageFile)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index");
            }

            var email = User.FindFirst(ClaimTypes.Email)?.Value ?? "";
            var user = await _unitOfWork.Users.GetUserByEmailAsync(email);
            
            if (user == null)
            {
                return RedirectToAction("Index1");
            }

            // Server-side validation for basic info
            if (string.IsNullOrWhiteSpace(model.FullName))
            {
                ModelState.AddModelError("FullName", "Full name is required.");
            }
            else if (model.FullName.Length > 100)
            {
                ModelState.AddModelError("FullName", "Full name cannot exceed 100 characters.");
            }

            // Email validation is not needed since it's read-only
            // We'll use the existing user's email from the database

            // Server-side validation for profile
            if (model.Profile != null)
            {
                if (string.IsNullOrWhiteSpace(model.Profile.Gender))
                {
                    ModelState.AddModelError("Profile.Gender", "Gender is required.");
                }
                else if (!new[] { "Male", "Female", "Other" }.Contains(model.Profile.Gender))
                {
                    ModelState.AddModelError("Profile.Gender", "Gender must be Male, Female, or Other.");
                }

                if (model.Profile.DateOfBirth >= DateTime.Now)
                {
                    ModelState.AddModelError("Profile.DateOfBirth", "Date of birth must be in the past.");
                }

                if (string.IsNullOrWhiteSpace(model.Profile.ContactNumber))
                {
                    ModelState.AddModelError("Profile.ContactNumber", "Contact number is required.");
                }

                if (model.Profile.MaxBudget < 0)
                {
                    ModelState.AddModelError("Profile.MaxBudget", "Budget cannot be negative.");
                }
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                // Update basic user information (email cannot be changed)
                user.FullName = model.FullName;
                // Email is not updated - it remains the same as registered

                // Handle profile image upload
                string profileImageUrl = model.Profile?.ProfileImageUrl;
                if (profileImageFile != null && profileImageFile.Length > 0)
                {
                    profileImageUrl = await SaveProfileImageAsync(profileImageFile, user.UserId);
                }

                // Update or create profile
                if (user.Profile == null)
                {
                    model.Profile.UserId = user.UserId;
                    model.Profile.CreatedAt = DateTime.UtcNow;
                    if (!string.IsNullOrEmpty(profileImageUrl))
                    {
                        model.Profile.ProfileImageUrl = profileImageUrl;
                    }
                    user.Profile = model.Profile;
                }
                else
                {
                    user.Profile.Gender = model.Profile.Gender;
                    user.Profile.DateOfBirth = model.Profile.DateOfBirth;
                    user.Profile.ContactNumber = model.Profile.ContactNumber;
                    if (!string.IsNullOrEmpty(profileImageUrl))
                    {
                        user.Profile.ProfileImageUrl = profileImageUrl;
                    }
                    else
                    {
                        user.Profile.ProfileImageUrl = model.Profile.ProfileImageUrl;
                    }
                    user.Profile.PreferredGender = model.Profile.PreferredGender;
                    user.Profile.MaxBudget = model.Profile.MaxBudget;
                    user.Profile.PreferredLocation = model.Profile.PreferredLocation;
                    user.Profile.Interests = model.Profile.Interests;
                    user.Profile.Bio = model.Profile.Bio;
                    user.Profile.IsCollegeVerified = model.Profile.IsCollegeVerified;
                    user.Profile.CollegeIdImageUrl = model.Profile.CollegeIdImageUrl;
                    user.Profile.LastUpdated = DateTime.UtcNow;
                }

                await _unitOfWork.CommitAsync();
                TempData["SuccessMessage"] = "Profile updated successfully!";
                return RedirectToAction("Index1");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "An error occurred while updating your profile. Please try again.");
                return View(model);
            }
        }


        // Test action to verify routing
        public IActionResult Test()
        {
            return Content("HomeController is working!");
        }

        // Helper method to save profile image
        private async Task<string> SaveProfileImageAsync(IFormFile file, int userId)
        {
            try
            {
                // Validate file
                if (file == null || file.Length == 0)
                    return null;

                // Check file size (5MB limit)
                if (file.Length > 5 * 1024 * 1024)
                    throw new InvalidOperationException("File size cannot exceed 5MB");

                // Check file type
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
                if (!allowedExtensions.Contains(fileExtension))
                    throw new InvalidOperationException("Only JPG, PNG, and GIF files are allowed");

                // Create uploads directory if it doesn't exist
                var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "profiles");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                // Generate unique filename
                var fileName = $"profile_{userId}_{DateTime.Now:yyyyMMddHHmmss}{fileExtension}";
                var filePath = Path.Combine(uploadsFolder, fileName);

                // Save file
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                // Return relative URL
                return $"/uploads/profiles/{fileName}";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving profile image for user {UserId}", userId);
                throw;
            }
        }

        // Helper method to validate email format
        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
