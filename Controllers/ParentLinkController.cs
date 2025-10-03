using Microsoft.AspNetCore.Mvc;
using StayShare.Models;
using StayShare.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace StayShare.Controllers
{
    public class ParentLinkController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public ParentLinkController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // GET: /ParentLink/Index - Show parent link requests and messages
        public async Task<IActionResult> Index()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Auth");
            }

            var userEmail = User.FindFirst(ClaimTypes.Email)?.Value ?? "";
            var user = await _unitOfWork.Users.GetUserByEmailAsync(userEmail);
            if (user == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            var pendingRequests = await _unitOfWork.ParentLinks.GetPendingRequestsForUserAsync(user.UserId);
            var sentRequests = await _unitOfWork.ParentLinks.GetSentRequestsByUserAsync(user.UserId);
            var acceptedLinks = await _unitOfWork.ParentLinks.GetAcceptedLinksForUserAsync(user.UserId);

            ViewBag.PendingRequests = pendingRequests;
            ViewBag.SentRequests = sentRequests;
            ViewBag.AcceptedLinks = acceptedLinks;
            ViewBag.CurrentUser = user;

            return View();
        }

        // GET: /ParentLink/Create - Show form to create parent link request
        public async Task<IActionResult> Create()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Auth");
            }

            var userEmail = User.FindFirst(ClaimTypes.Email)?.Value ?? "";
            var user = await _unitOfWork.Users.GetUserByEmailAsync(userEmail);
            if (user == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            // Get all users with Parent role
            var parents = await _unitOfWork.ParentLinks.GetUsersByRoleAsync("Parent");
            
            ViewBag.Parents = parents;
            ViewBag.CurrentUser = user;

            return View(new ParentLink { ChildId = user.UserId });
        }

        // POST: /ParentLink/Create
        [HttpPost]
        public async Task<IActionResult> Create(ParentLink model)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Auth");
            }

            var userEmail = User.FindFirst(ClaimTypes.Email)?.Value ?? "";
            var user = await _unitOfWork.Users.GetUserByEmailAsync(userEmail);
            if (user == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            if (!ModelState.IsValid)
            {
                var parents = await _unitOfWork.ParentLinks.GetUsersByRoleAsync("Parent");
                ViewBag.Parents = parents;
                ViewBag.CurrentUser = user;
                return View(model);
            }

            // Check if request already exists
            var existingRequest = await _unitOfWork.ParentLinks.HasExistingRequestAsync(model.ParentId, user.UserId);
            if (existingRequest)
            {
                TempData["ErrorMessage"] = "A request already exists between you and this parent.";
                return RedirectToAction("Create");
            }

            model.ChildId = user.UserId;
            model.Status = ParentLinkStatus.Pending;
            model.RequestedAt = DateTime.UtcNow;

            await _unitOfWork.ParentLinks.AddAsync(model);
            await _unitOfWork.CommitAsync();

            TempData["SuccessMessage"] = "Parent link request sent successfully.";
            return RedirectToAction("Index");
        }

        // GET: /ParentLink/CreateFromParent - Show form for parent to create request to child
        public async Task<IActionResult> CreateFromParent()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Auth");
            }

            var userEmail = User.FindFirst(ClaimTypes.Email)?.Value ?? "";
            var user = await _unitOfWork.Users.GetUserByEmailAsync(userEmail);
            if (user == null || user.Role != "Parent")
            {
                return RedirectToAction("Index1", "Home");
            }

            // Get all users with Resident role
            var residents = await _unitOfWork.ParentLinks.GetUsersByRoleAsync("Resident");
            
            ViewBag.Residents = residents;
            ViewBag.CurrentUser = user;

            return View(new ParentLink { ParentId = user.UserId });
        }

        // POST: /ParentLink/CreateFromParent
        [HttpPost]
        public async Task<IActionResult> CreateFromParent(ParentLink model)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Auth");
            }

            var userEmail = User.FindFirst(ClaimTypes.Email)?.Value ?? "";
            var user = await _unitOfWork.Users.GetUserByEmailAsync(userEmail);
            if (user == null || user.Role != "Parent")
            {
                return RedirectToAction("Index1", "Home");
            }

            if (!ModelState.IsValid)
            {
                var residents = await _unitOfWork.ParentLinks.GetUsersByRoleAsync("Resident");
                ViewBag.Residents = residents;
                ViewBag.CurrentUser = user;
                return View(model);
            }

            // Check if request already exists
            var existingRequest = await _unitOfWork.ParentLinks.HasExistingRequestAsync(user.UserId, model.ChildId);
            if (existingRequest)
            {
                TempData["ErrorMessage"] = "A request already exists between you and this resident.";
                return RedirectToAction("CreateFromParent");
            }

            model.ParentId = user.UserId;
            model.Status = ParentLinkStatus.Pending;
            model.RequestedAt = DateTime.UtcNow;

            await _unitOfWork.ParentLinks.AddAsync(model);
            await _unitOfWork.CommitAsync();

            TempData["SuccessMessage"] = "Parent link request sent successfully.";
            return RedirectToAction("Index");
        }

        // POST: /ParentLink/Accept/5
        [HttpPost]
        public async Task<IActionResult> Accept(int id)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Auth");
            }

            var userEmail = User.FindFirst(ClaimTypes.Email)?.Value ?? "";
            var user = await _unitOfWork.Users.GetUserByEmailAsync(userEmail);
            if (user == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            var parentLink = await _unitOfWork.ParentLinks.GetByIdAsync(id);
            if (parentLink == null)
            {
                return NotFound();
            }

            // Check if user is authorized to accept this request
            if (parentLink.ParentId != user.UserId && parentLink.ChildId != user.UserId)
            {
                return Forbid();
            }

            parentLink.Status = ParentLinkStatus.Accepted;
            parentLink.RespondedAt = DateTime.UtcNow;
            parentLink.LinkedAt = DateTime.UtcNow;

            await _unitOfWork.ParentLinks.UpdateAsync(parentLink);
            await _unitOfWork.CommitAsync();

            TempData["SuccessMessage"] = "Parent link request accepted successfully.";
            return RedirectToAction("Index");
        }

        // POST: /ParentLink/Decline/5
        [HttpPost]
        public async Task<IActionResult> Decline(int id)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Auth");
            }

            var userEmail = User.FindFirst(ClaimTypes.Email)?.Value ?? "";
            var user = await _unitOfWork.Users.GetUserByEmailAsync(userEmail);
            if (user == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            var parentLink = await _unitOfWork.ParentLinks.GetByIdAsync(id);
            if (parentLink == null)
            {
                return NotFound();
            }

            // Check if user is authorized to decline this request
            if (parentLink.ParentId != user.UserId && parentLink.ChildId != user.UserId)
            {
                return Forbid();
            }

            parentLink.Status = ParentLinkStatus.Declined;
            parentLink.RespondedAt = DateTime.UtcNow;

            await _unitOfWork.ParentLinks.UpdateAsync(parentLink);
            await _unitOfWork.CommitAsync();

            TempData["SuccessMessage"] = "Parent link request declined.";
            return RedirectToAction("Index");
        }

        // GET: /ParentLink/Dashboard - Parent dashboard to view child details
        public async Task<IActionResult> Dashboard()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Auth");
            }

            var userEmail = User.FindFirst(ClaimTypes.Email)?.Value ?? "";
            var user = await _unitOfWork.Users.GetUserByEmailAsync(userEmail);
            if (user == null || user.Role != "Parent")
            {
                return RedirectToAction("Index1", "Home");
            }

            var acceptedLinks = await _unitOfWork.ParentLinks.GetAcceptedLinksForParentAsync(user.UserId);
            
            ViewBag.AcceptedLinks = acceptedLinks;
            ViewBag.CurrentUser = user;

            return View();
        }

        // GET: /ParentLink/ChildDetails/5 - View specific child details
        public async Task<IActionResult> ChildDetails(int id)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Auth");
            }

            var userEmail = User.FindFirst(ClaimTypes.Email)?.Value ?? "";
            var user = await _unitOfWork.Users.GetUserByEmailAsync(userEmail);
            if (user == null || user.Role != "Parent")
            {
                return RedirectToAction("Index1", "Home");
            }

            var parentLink = await _unitOfWork.ParentLinks.GetByIdAsync(id);
            if (parentLink == null || parentLink.ParentId != user.UserId || parentLink.Status != ParentLinkStatus.Accepted)
            {
                return NotFound();
            }

            // Get child's occupancy history
            var occupancies = await _unitOfWork.Occupancies.GetOccupanciesByUserIdAsync(parentLink.ChildId);
            var currentStay = occupancies?.FirstOrDefault(o => o.IsActive && o.JoinedAt.HasValue && o.JoinedAt.Value <= DateTime.UtcNow);
            var pastStays = occupancies?.Where(o => !o.IsActive && (o.Status == OccupancyStatus.Left || o.ExitDate.HasValue))
                .OrderByDescending(o => o.ExitDate ?? o.JoinedAt)
                .ToList();

            // Get current roommates
            var roommates = new List<User>();
            if (currentStay != null)
            {
                var roomOccupancies = await _unitOfWork.Occupancies.GetOccupanciesByRoomIdAsync(currentStay.RoomId);
                roommates = roomOccupancies?.Where(o => o.IsActive && o.UserId != parentLink.ChildId && o.JoinedAt.HasValue && o.JoinedAt.Value <= DateTime.UtcNow)
                    .Select(o => o.User)
                    .Where(u => u != null)
                    .ToList() ?? new List<User>();
            }

            ViewBag.ParentLink = parentLink;
            ViewBag.CurrentStay = currentStay;
            ViewBag.PastStays = pastStays ?? new List<RoomOccupancy>();
            ViewBag.Roommates = roommates;
            ViewBag.CurrentUser = user;

            return View();
        }

        // GET: /ParentLink/PendingCount - Get count of pending requests for navigation
        [HttpGet]
        public async Task<IActionResult> PendingCount()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return Json(new { count = 0 });
            }

            var userEmail = User.FindFirst(ClaimTypes.Email)?.Value ?? "";
            var user = await _unitOfWork.Users.GetUserByEmailAsync(userEmail);
            if (user == null)
            {
                return Json(new { count = 0 });
            }

            var pendingRequests = await _unitOfWork.ParentLinks.GetPendingRequestsForUserAsync(user.UserId);
            var pendingCount = pendingRequests?.Count() ?? 0;
            
            return Json(new { count = pendingCount });
        }
    }
}
