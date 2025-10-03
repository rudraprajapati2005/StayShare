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
        public async Task<IActionResult> Create(string q)
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

            // Only residents can send requests
            if (!string.Equals(user.Role?.Trim(), "Resident", StringComparison.OrdinalIgnoreCase))
            {
                return RedirectToAction("Index");
            }

            // Simplified email flow: no listing
            ViewBag.Parents = null;
            ViewBag.ParentsCount = 0;
            ViewBag.CurrentUser = user;
            ViewBag.Query = q;
            var pendingRequests = await _unitOfWork.ParentLinks.GetPendingRequestsForUserAsync(user.UserId);
            ViewBag.PendingRequests = pendingRequests;

            return View(new ParentLink { ChildId = user.UserId });
        }

        // POST: /ParentLink/Create
        [HttpPost]
        public async Task<IActionResult> Create(ParentLink model, string q, string parentEmail)
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

            // Only residents can send requests
            if (!string.Equals(user.Role?.Trim(), "Resident", StringComparison.OrdinalIgnoreCase))
            {
                return RedirectToAction("Index");
            }

            // Validate email input
            if (string.IsNullOrWhiteSpace(parentEmail))
            {
                ModelState.AddModelError("parentEmail", "Please enter a guardian email.");
            }
            var emailToFind = (parentEmail ?? string.Empty).Trim();
            var selectedParent = string.IsNullOrWhiteSpace(emailToFind) ? null : await _unitOfWork.Users.GetUserByEmailAsync(emailToFind);
            if (selectedParent == null && !string.IsNullOrWhiteSpace(parentEmail))
            {
                ModelState.AddModelError("parentEmail", "This email was not found.");
            }
            else if (selectedParent != null && !string.Equals(selectedParent.Role?.Trim(), "Guardian", StringComparison.OrdinalIgnoreCase))
            {
                ModelState.AddModelError("parentEmail", "This user is not registered as Guardian.");
            }
            else if (selectedParent != null && selectedParent.UserId == user.UserId)
            {
                ModelState.AddModelError("parentEmail", "You cannot enter your own email.");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Parents = null;
                ViewBag.ParentsCount = 0;
                ViewBag.CurrentUser = user;
                ViewBag.Query = q;
                var pendingRequests = await _unitOfWork.ParentLinks.GetPendingRequestsForUserAsync(user.UserId);
                ViewBag.PendingRequests = pendingRequests;
                return View(model);
            }

            // Check if a link/request already exists between these users
            var existingLink = await _unitOfWork.ParentLinks.GetExistingLinkAsync(selectedParent.UserId, user.UserId);
            if (existingLink != null)
            {
                if (existingLink.Status == ParentLinkStatus.Accepted)
                {
                    TempData["ErrorMessage"] = "You are already linked with this guardian.";
                }
                else if (existingLink.Status == ParentLinkStatus.Pending)
                {
                    TempData["ErrorMessage"] = "A pending request already exists with this guardian.";
                }
                else
                {
                    TempData["ErrorMessage"] = "A previous request exists between you and this guardian.";
                }
                return RedirectToAction("Index");
            }

            model.ChildId = user.UserId;
            model.ParentId = selectedParent.UserId;
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

            // Only guardians can accept
            if (!string.Equals(user.Role?.Trim(), "Guardian", StringComparison.OrdinalIgnoreCase) || parentLink.ParentId != user.UserId)
            {
                return Forbid();
            }
            if (parentLink.Status != ParentLinkStatus.Pending)
            {
                return BadRequest("Only pending requests can be accepted.");
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

            // Only guardians can decline
            if (!string.Equals(user.Role?.Trim(), "Guardian", StringComparison.OrdinalIgnoreCase) || parentLink.ParentId != user.UserId)
            {
                return Forbid();
            }
            if (parentLink.Status != ParentLinkStatus.Pending)
            {
                return BadRequest("Only pending requests can be declined.");
            }

            parentLink.Status = ParentLinkStatus.Declined;
            parentLink.RespondedAt = DateTime.UtcNow;

            await _unitOfWork.ParentLinks.UpdateAsync(parentLink);
            await _unitOfWork.CommitAsync();

            TempData["SuccessMessage"] = "Parent link request declined.";
            return RedirectToAction("Index");
        }

        // POST: /ParentLink/Delete/5 - Resident deletes their pending request
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
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

            // Only resident (child) who sent it can delete while pending
            if (!string.Equals(user.Role?.Trim(), "Resident", StringComparison.OrdinalIgnoreCase) || parentLink.ChildId != user.UserId || parentLink.Status != ParentLinkStatus.Pending)
            {
                return Forbid();
            }

            await _unitOfWork.ParentLinks.DeleteAsync(id);
            await _unitOfWork.CommitAsync();

            TempData["SuccessMessage"] = "Request deleted.";
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
            if (user == null || !string.Equals(user.Role?.Trim(), "Guardian", StringComparison.OrdinalIgnoreCase))
            {
                return RedirectToAction("Index1", "Home");
            }

            var acceptedLinks = await _unitOfWork.ParentLinks.GetAcceptedLinksForParentAsync(user.UserId);
            
            ViewBag.AcceptedLinks = acceptedLinks;
            ViewBag.CurrentUser = user;

            return View();
        }

        // GET: /ParentLink/CreateFromParent - Guardian sends request to resident by email
        public async Task<IActionResult> CreateFromParent()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Auth");
            }

            var userEmail = User.FindFirst(ClaimTypes.Email)?.Value ?? "";
            var user = await _unitOfWork.Users.GetUserByEmailAsync(userEmail);
            if (user == null || !string.Equals(user.Role?.Trim(), "Guardian", StringComparison.OrdinalIgnoreCase))
            {
                return RedirectToAction("Index1", "Home");
            }

            ViewBag.CurrentUser = user;
            return View(new ParentLink { ParentId = user.UserId });
        }

        // POST: /ParentLink/CreateFromParent - Guardian submits resident email
        [HttpPost]
        public async Task<IActionResult> CreateFromParent(ParentLink model, string childEmail)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Auth");
            }

            var userEmail = User.FindFirst(ClaimTypes.Email)?.Value ?? "";
            var user = await _unitOfWork.Users.GetUserByEmailAsync(userEmail);
            if (user == null || !string.Equals(user.Role?.Trim(), "Guardian", StringComparison.OrdinalIgnoreCase))
            {
                return RedirectToAction("Index1", "Home");
            }

            if (string.IsNullOrWhiteSpace(childEmail))
            {
                ModelState.AddModelError("childEmail", "Please enter a resident email.");
            }
            var emailToFind = (childEmail ?? string.Empty).Trim();
            var resident = string.IsNullOrWhiteSpace(emailToFind) ? null : await _unitOfWork.Users.GetUserByEmailAsync(emailToFind);
            if (resident == null && !string.IsNullOrWhiteSpace(childEmail))
            {
                ModelState.AddModelError("childEmail", "This email was not found.");
            }
            else if (resident != null && !string.Equals(resident.Role?.Trim(), "Resident", StringComparison.OrdinalIgnoreCase))
            {
                ModelState.AddModelError("childEmail", "This user is not registered as Resident.");
            }
            else if (resident != null && resident.UserId == user.UserId)
            {
                ModelState.AddModelError("childEmail", "You cannot enter your own email.");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.CurrentUser = user;
                return View(model);
            }

            var existingLink = await _unitOfWork.ParentLinks.GetExistingLinkAsync(user.UserId, resident.UserId);
            if (existingLink != null)
            {
                if (existingLink.Status == ParentLinkStatus.Accepted)
                {
                    TempData["ErrorMessage"] = "You are already linked with this resident.";
                }
                else if (existingLink.Status == ParentLinkStatus.Pending)
                {
                    TempData["ErrorMessage"] = "A pending request already exists with this resident.";
                }
                else
                {
                    TempData["ErrorMessage"] = "A previous request exists between you and this resident.";
                }
                return RedirectToAction("Index");
            }

            model.ParentId = user.UserId;
            model.ChildId = resident.UserId;
            model.Status = ParentLinkStatus.Pending;
            model.RequestedAt = DateTime.UtcNow;

            await _unitOfWork.ParentLinks.AddAsync(model);
            await _unitOfWork.CommitAsync();

            TempData["SuccessMessage"] = "Request sent to resident successfully.";
            return RedirectToAction("Index");
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
            if (user == null || !string.Equals(user.Role?.Trim(), "Guardian", StringComparison.OrdinalIgnoreCase))
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

        // GET: /ParentLink/CheckGuardian?email=...
        [HttpGet]
        public async Task<IActionResult> CheckGuardian(string email)
        {
            var result = new { exists = false, name = "", roleOk = false };
            if (string.IsNullOrWhiteSpace(email))
            {
                return Json(result);
            }
            var user = await _unitOfWork.Users.GetUserByEmailAsync(email.Trim());
            if (user == null)
            {
                return Json(result);
            }
            return Json(new { exists = true, name = user.FullName ?? user.Email, roleOk = string.Equals(user.Role?.Trim(), "Guardian", StringComparison.OrdinalIgnoreCase) });
        }
    }
}
