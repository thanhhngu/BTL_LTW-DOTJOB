using BTL_LTW_DOTJOB.Services.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BTL_LTW_DOTJOB.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Policy = "AdminOnly")]
    public class DashboardController : Controller
    {
        private readonly IDashboardService _dashboardService;

        // Nhận Service qua Dependency Injection (đã đăng ký ở Program.cs)
        public DashboardController(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        // GET: /Admin/Dashboard
        public async Task<IActionResult> Index()
        {
            var stats = await _dashboardService.GetStatsAsync();
            return View(stats); // Truyền số liệu sang View
        }
    }
}