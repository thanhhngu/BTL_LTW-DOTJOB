using BTL_LTW_DOTJOB.Services.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BTL_LTW_DOTJOB.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Policy = "AdminOnly")] // Chỉ Admin được quản lý người dùng
    public class UserController : Controller
    {
        private readonly IUserManagementService _userService;

        public UserController(IUserManagementService userService)
        {
            _userService = userService;
        }

        // GET: /Admin/User?keyword=...&role=...&status=...&page=...
        public async Task<IActionResult> Index(string? keyword, string? role, string? status, int page = 1)
        {
            var model = await _userService.GetPagedAsync(keyword, role, status, page);
            return View(model);
        }

        // POST: /Admin/User/ToggleLock  (khóa <-> mở khóa)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleLock(string id, string? keyword, string? role, string? status, int page = 1)
        {
            // Id của Admin đang đăng nhập, lấy từ Claims trong cookie (luôn có, không phụ thuộc Session)
            var currentAdminId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;

            var (success, message) = await _userService.ToggleLockAsync(id, currentAdminId);

            if (success) TempData["Success"] = message;
            else TempData["Error"] = message;

            // Quay lại đúng trang và đúng bộ lọc đang xem
            return RedirectToAction(nameof(Index), new { keyword, role, status, page });
        }
    }
}