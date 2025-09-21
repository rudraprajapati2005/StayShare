using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using StayShare.Models;
using StayShare.Repositories;
using System;
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
            if (!ModelState.IsValid)
                return View(model);

            var existingUser = await _unitOfWork.Users.GetUserByEmailAsync(model.Email);
            if (existingUser != null)
            {
                ModelState.AddModelError("", "Email already registered.");
                return View(model);
            }

            // Hash password
            model.PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.PasswordHash);

            // Set created date
            model.CreatedAt = DateTime.UtcNow;

            // Add user
            await _unitOfWork.Users.AddUserAsync(model);
            await _unitOfWork.CommitAsync();

            // Auto-login
            await SignInUser(model);

            // Map Role for redirection
            return model.Role switch
            {
                "Owner" => RedirectToAction("Create", "Room"),
                _ => RedirectToAction("Index", "Room")
            };
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
                ? RedirectToAction("Create", "Room")
                : RedirectToAction("Index", "Room");
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
