using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using StayShare.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace PGConnect.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
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
        public IActionResult Index1()
        {
            if (!User.Identity.IsAuthenticated)
            {
                // If user is not authenticated, redirect to home or login
                return RedirectToAction("Index");
            }

            // Retrieve user data from claims
            var fullName = User.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value ?? "Guest";
            var email = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value ?? "";
            var role = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value ?? "";

            // Pass data to the view using ViewBag
            ViewBag.FullName = fullName;
            ViewBag.Email = email;
            ViewBag.Role = role;

            return View();
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
