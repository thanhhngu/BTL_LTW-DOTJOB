using BTL_LTW_DOTJOB.Services.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BTL_LTW_DOTJOB.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Policy = "AdminOnly")]
    public class UserController : Controller
    {
        private readonly IUserManagementService _userService;

        public UserController(IUserManagementService userService)
        {
            _userService = userService;
        }

        public async Task<IActionResult> Index(string? keyword, string? role, string? status, int page = 1)
        {
            var model = await _userService.GetPagedAsync(keyword, role, status, page);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleLock(string id, string? keyword, string? role, string? status, int page = 1)
        {
            var currentAdminId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;

            var (success, message) = await _userService.ToggleLockAsync(id, currentAdminId);

            if (success) TempData["Success"] = message;
            else TempData["Error"] = message;

            return RedirectToAction(nameof(Index), new { keyword, role, status, page });
        }
    }
}