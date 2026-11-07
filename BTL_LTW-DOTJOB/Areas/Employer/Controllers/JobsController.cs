using BTL_LTW_DOTJOB.Areas.Employer.ViewModels;
using BTL_LTW_DOTJOB.Data;
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

        // ===== M2-1: Sửa tin tuyển dụng =====

        // GET: /Employer/Jobs/Edit/{id}
        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            // Chỉ lấy tin thuộc công ty của HR đang đăng nhập
            var companyId = await GetCompanyIdAsync();
            var job = await _db.JobPostings
                .AsNoTracking()
                .FirstOrDefaultAsync(j => j.Id == id && j.CompanyId == companyId);

            if (job == null)
            {
                return NotFound();
            }

            // Đổ dữ liệu từ entity vào ViewModel
            var model = new JobEditViewModel
            {
                Id = job.Id,
                Title = job.Title,
                CategoryId = job.CategoryId,
                Location = job.Location,
                JobType = job.JobType,
                SalaryMin = job.SalaryMin,
                SalaryMax = job.SalaryMax,
                Deadline = job.Deadline,
                Description = job.Description,
                Requirements = job.Requirements ?? string.Empty
            };

            await FillOptionsAsync(model);
            return View(model);
        }

        // POST: /Employer/Jobs/Edit/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, JobEditViewModel model)
        {
            // Kiểm tra chủ sở hữu ở cả POST: đổi id trên URL sang tin của HR khác sẽ bị NotFound
            var companyId = await GetCompanyIdAsync();
            var job = await _db.JobPostings
                .FirstOrDefaultAsync(j => j.Id == id && j.CompanyId == companyId);

            if (job == null)
            {
                return NotFound();
            }

            // Ngành nghề phải có thật trong DB (chống POST giá trị tự chế)
            var categoryExists = await _db.Categories.AnyAsync(c => c.Id == model.CategoryId);
            if (!categoryExists)
            {
                ModelState.AddModelError(nameof(model.CategoryId), "Ngành nghề không hợp lệ.");
            }

            if (!ModelState.IsValid)
            {
                model.Id = id;
                await FillOptionsAsync(model);
                return View(model);
            }

            // Đổ dữ liệu từ ViewModel ngược vào entity
            job.Title = model.Title.Trim();
            job.CategoryId = model.CategoryId;
            job.Location = model.Location;
            job.JobType = model.JobType;
            job.SalaryMin = model.SalaryMin;
            job.SalaryMax = model.SalaryMax;
            // Lưu cuối ngày (23:59:59) để tin còn nhận hồ sơ hết ngày đã chọn
            job.Deadline = model.Deadline!.Value.Date.AddDays(1).AddSeconds(-1);
            job.Description = model.Description;
            job.Requirements = model.Requirements;

            await _db.SaveChangesAsync();

            TempData["Success"] = "Cập nhật tin tuyển dụng thành công.";
            return RedirectToAction(nameof(Index));
        }
    }
}
