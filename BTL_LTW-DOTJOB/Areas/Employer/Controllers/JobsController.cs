using BTL_LTW_DOTJOB.Areas.Employer.ViewModels;
using BTL_LTW_DOTJOB.Data;
using BTL_LTW_DOTJOB.Models;
using Ganss.Xss;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace BTL_LTW_DOTJOB.Areas.Employer.Controllers
{
    [Area("Employer")]
    [Authorize(Roles = "Employer")]
    public class JobsController : Controller
    {
        private readonly AppDbContext _db;

        // Làm sạch HTML từ editor (xóa <script>, onerror=...) trước khi lưu
        private readonly HtmlSanitizer _sanitizer = new();

        public JobsController(AppDbContext db)
        {
            _db = db;
        }

        // ===== Hàm dùng chung =====

        // Lấy Id công ty của HR đang đăng nhập (không nhận từ form hay URL)
        private async Task<string?> GetCompanyIdAsync()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return await _db.Companies
                .Where(c => c.UserId == userId)
                .Select(c => c.Id)
                .FirstOrDefaultAsync();
        }

        // Nạp dữ liệu cho 3 ô chọn: Ngành nghề, Hình thức, Địa điểm
        private async Task FillOptionsAsync(JobCreateViewModel model)
        {
            model.Categories = await _db.Categories
                .AsNoTracking()
                .OrderBy(c => c.Name)
                .Select(c => new SelectListItem { Value = c.Id, Text = c.Name })
                .ToListAsync();

            model.JobTypes = JobOptions.JobTypes
                .Select(t => new SelectListItem { Value = t, Text = t })
                .ToList();

            model.Locations = JobOptions.Locations
                .Select(l => new SelectListItem { Value = l, Text = l })
                .ToList();
        }

        // ===== M1-2: Tạo tin tuyển dụng =====

        // GET: /Employer/Jobs/Create
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var model = new JobCreateViewModel { Deadline = DateTime.Today.AddDays(30) };
            await FillOptionsAsync(model);
            return View(model);
        }

        // POST: /Employer/Jobs/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(JobCreateViewModel model)
        {
            var companyId = await GetCompanyIdAsync();
            if (companyId == null)
            {
                return Forbid();
            }

            // Ngành nghề phải có thật trong DB (chống POST giá trị tự chế)
            var categoryExists = await _db.Categories.AnyAsync(c => c.Id == model.CategoryId);
            if (!categoryExists)
            {
                ModelState.AddModelError(nameof(model.CategoryId), "Ngành nghề không hợp lệ.");
            }

            if (!ModelState.IsValid)
            {
                await FillOptionsAsync(model);
                return View(model);
            }

            var job = new JobPosting
            {
                Title = model.Title.Trim(),
                CategoryId = model.CategoryId,
                Location = model.Location,
                JobType = model.JobType,
                SalaryMin = model.SalaryMin,
                SalaryMax = model.SalaryMax,
                // Lưu cuối ngày (23:59:59) để tin còn nhận hồ sơ hết ngày đã chọn
                Deadline = model.Deadline!.Value.Date.AddDays(1).AddSeconds(-1),
                Description = _sanitizer.Sanitize(model.Description),
                Requirements = _sanitizer.Sanitize(model.Requirements),
                CompanyId = companyId // lấy từ tài khoản đăng nhập, không nhận từ form
            };

            _db.JobPostings.Add(job);
            await _db.SaveChangesAsync();

            TempData["Success"] = "Đăng tin tuyển dụng thành công.";
            // Tạm về Dashboard, đổi sang danh sách tin ở #9
            return RedirectToAction("Index", "Dashboard");
        }
    }
}
