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
        public async Task<IActionResult> Register(User model)
        {
            model.CreatedAt = DateTime.UtcNow;
            model.IsVerified = false;

            // Remove navigation properties from model state validation
            ModelState.Remove(nameof(model.UserId));
            ModelState.Remove(nameof(model.Profile));
            ModelState.Remove(nameof(model.RoomOccupancies));
            ModelState.Remove(nameof(model.ParentLinks));

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
                return RedirectToAction("Success");
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
        public async Task<IActionResult> Login(string email, string password)
        {
            var user = await _unitOfWork.Users.GetUserByEmailAsync(email);
            if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            {
                ModelState.AddModelError("", "Invalid email or password.");
                return View();
            }

            await SignInUser(user);

            return user.Role == "Owner"
                ? RedirectToAction("Index1", "Home")
                : RedirectToAction("Auth", "LogIn");
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
    }
}
