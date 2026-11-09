using BTL_LTW_DOTJOB.Services.Admin;
using BTL_LTW_DOTJOB.ViewModels.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BTL_LTW_DOTJOB.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Policy = "AdminOnly")] // Chỉ Admin được xử lý báo cáo
    public class ReportController : Controller
    {
        private readonly IReportManagementService _reportService;

        public ReportController(IReportManagementService reportService)
        {
            _reportService = reportService;
        }

        // GET: /Admin/Report?status=...&targetType=...&page=...
        public async Task<IActionResult> Index(string? status, string? targetType, int page = 1)
        {
            var model = await _reportService.GetPagedAsync(status, targetType, page);
            return View(model);
        }

        // GET: /Admin/Report/Details/{id}
        [HttpGet]
        public async Task<IActionResult> Details(string id)
        {
            var model = await _reportService.GetDetailAsync(id);
            if (model == null) return NotFound();
            return View(model);
        }

        // POST: /Admin/Report/Handle  (Admin xử lý báo cáo)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Handle(ReportHandleViewModel model)
        {
            if (!ModelState.IsValid)
            {
                // Lấy thông báo lỗi đầu tiên để hiện lại ở trang chi tiết
                TempData["Error"] = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .FirstOrDefault() ?? "Dữ liệu không hợp lệ.";
                return RedirectToAction(nameof(Details), new { id = model.Id });
            }

            var (success, message) = await _reportService.HandleAsync(model);

            if (success) TempData["Success"] = message;
            else TempData["Error"] = message;

            // Ở lại trang chi tiết để Admin thấy ngay kết quả
            return RedirectToAction(nameof(Details), new { id = model.Id });
        }
        // POST: /Admin/Report/RestoreJob  (hiện lại tin đã bị ẩn)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RestoreJob(string id)
        {
            var (success, message) = await _reportService.RestoreJobAsync(id);

            if (success) TempData["Success"] = message;
            else TempData["Error"] = message;

            return RedirectToAction(nameof(Details), new { id });
        }
    }
}