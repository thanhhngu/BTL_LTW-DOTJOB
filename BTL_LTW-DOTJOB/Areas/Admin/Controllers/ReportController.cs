using BTL_LTW_DOTJOB.Services.Admin;
using BTL_LTW_DOTJOB.ViewModels.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BTL_LTW_DOTJOB.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Policy = "AdminOnly")]
    public class ReportController : Controller
    {
        private readonly IReportManagementService _reportService;

        public ReportController(IReportManagementService reportService)
        {
            _reportService = reportService;
        }

        public async Task<IActionResult> Index(string? status, string? targetType, int page = 1)
        {
            var model = await _reportService.GetPagedAsync(status, targetType, page);
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Details(string id)
        {
            var model = await _reportService.GetDetailAsync(id);
            if (model == null) return NotFound();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Handle(ReportHandleViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .FirstOrDefault() ?? "Dữ liệu không hợp lệ.";
                return RedirectToAction(nameof(Details), new { id = model.Id });
            }

            var (success, message) = await _reportService.HandleAsync(model);

            if (success) TempData["Success"] = message;
            else TempData["Error"] = message;

            return RedirectToAction(nameof(Details), new { id = model.Id });
        }
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