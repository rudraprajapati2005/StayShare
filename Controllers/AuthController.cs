using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using StayShare.Models;
using StayShare.Repositories;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace StayShare.Controllers
{
    public class AuthController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public AuthController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // GET: /Auth/Register
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        // POST: /Auth/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(User model)
        {
            model.CreatedAt = DateTime.UtcNow;
            model.IsVerified = false;

            // Remove navigation properties from model state validation
            ModelState.Remove(nameof(model.UserId));
            ModelState.Remove(nameof(model.Profile));
            ModelState.Remove(nameof(model.RoomOccupancies));
            ModelState.Remove(nameof(model.ParentLinks));

            // Server-side validation
            if (string.IsNullOrWhiteSpace(model.FullName))
            {
                ModelState.AddModelError("FullName", "Full name is required.");
            }
            else if (model.FullName.Length > 100)
            {
                ModelState.AddModelError("FullName", "Full name cannot exceed 100 characters.");
            }

            if (string.IsNullOrWhiteSpace(model.Email))
            {
                ModelState.AddModelError("Email", "Email is required.");
            }
            else if (!IsValidEmail(model.Email))
            {
                ModelState.AddModelError("Email", "Invalid email format.");
            }
            else if (model.Email.Length > 255)
            {
                ModelState.AddModelError("Email", "Email cannot exceed 255 characters.");
            }

            if (string.IsNullOrWhiteSpace(model.PasswordHash))
            {
                ModelState.AddModelError("PasswordHash", "Password is required.");
            }
            else if (model.PasswordHash.Length < 6)
            {
                ModelState.AddModelError("PasswordHash", "Password must be at least 6 characters long.");
            }

            if (string.IsNullOrWhiteSpace(model.Role))
            {
                ModelState.AddModelError("Role", "Role is required.");
            }
            else if (!new[] { "Host", "Resident", "Guardian" }.Contains(model.Role))
            {
                ModelState.AddModelError("Role", "Role must be Host, Resident, or Guardian.");
            }

            if (!ModelState.IsValid)
            {
                return View("Register", model);
            }

            try
            {
                var existingUser = await _unitOfWork.Users.GetUserByEmailAsync(model.Email);
                if (existingUser != null)
                {
                    ModelState.AddModelError("Email", "Email already registered.");
                    return View("Register", model);
                }

                model.PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.PasswordHash);

                await _unitOfWork.Users.AddUserAsync(model);
                var result = await _unitOfWork.CommitAsync();

                if (result <= 0)
                {
                    ModelState.AddModelError("", "Failed to save user. Check database connection.");
                    return View("Register", model);
                }

                await SignInUser(model);
                return RedirectToAction("Index1", "Home");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Unexpected error: " + ex.Message);
                return View("Register", model);
            }
        }


        // GET: /Auth/Login
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        // POST: /Auth/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string email, string password)
        {
            // Server-side validation
            if (string.IsNullOrWhiteSpace(email))
            {
                ModelState.AddModelError("", "Email is required.");
            }
            else if (!IsValidEmail(email))
            {
                ModelState.AddModelError("", "Invalid email format.");
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                ModelState.AddModelError("", "Password is required.");
            }

            if (!ModelState.IsValid)
            {
                return View();
            }

            try
            {
                var user = await _unitOfWork.Users.GetUserByEmailAsync(email);
                if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
                {
                    ModelState.AddModelError("", "Invalid email or password.");
                    return View();
                }

                await SignInUser(user);

                return RedirectToAction("Index1", "Home");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "An error occurred during login. Please try again.");
                return View();
            }
        }

        // GET: /Auth/Profile
        [HttpGet]
        public IActionResult Profile()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login");
            }

            ViewBag.FullName = User.FindFirst(ClaimTypes.Name)?.Value ?? "";
            ViewBag.Email = User.FindFirst(ClaimTypes.Email)?.Value ?? "";
            ViewBag.Role = User.FindFirst(ClaimTypes.Role)?.Value ?? "";

            return View();
        }

        // GET: /Auth/Success
        [HttpGet]
        public IActionResult Success()
        {
            return View();
        }

        // POST: /Auth/Logout
        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }

        // Helper to sign in user
        private async Task SignInUser(User user)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.Name, user.FullName),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role)
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);
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
    }
}
