using BTL_LTW_DOTJOB.Areas.Employer.ViewModels;
using BTL_LTW_DOTJOB.Data;
using BTL_LTW_DOTJOB.Models;
using Ganss.Xss;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using X.PagedList;
using X.PagedList.Extensions;

namespace BTL_LTW_DOTJOB.Areas.Employer.Controllers
{
    [Area("Employer")]
    [Authorize(Roles = "Employer")]
    public class JobsController : Controller
    {
        private readonly AppDbContext _db;

        // Làm sạch HTML từ editor (xóa <script>, onerror=...) trước khi lưu
        private readonly HtmlSanitizer _sanitizer = new();

        private const int PageSize = 10;

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

        // ===== M1-3: Danh sách tin của HR =====

        // GET: /Employer/Jobs
        // Gọi bằng fetch (header X-Requested-With) thì chỉ trả bảng tin, không kèm layout
        [HttpGet]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public async Task<IActionResult> Index(string? status, string? categoryId, int page = 1)
        {
            var companyId = await GetCompanyIdAsync();
            if (companyId == null)
            {
                return Forbid();
            }

            var today = DateTime.Today;

            // Chỉ lấy tin của công ty mình
            var jobs = _db.JobPostings
                .AsNoTracking()
                .Include(j => j.Category)
                .Include(j => j.Applications)
                .Where(j => j.CompanyId == companyId);

            if (status == "open")
            {
                jobs = jobs.Where(j => j.IsActive && (j.Deadline == null || j.Deadline >= today));
            }
            else if (status == "expired")
            {
                jobs = jobs.Where(j => j.IsActive && j.Deadline != null && j.Deadline < today);
            }
            else if (status == "closed")
            {
                jobs = jobs.Where(j => !j.IsActive);
            }

            if (!string.IsNullOrEmpty(categoryId))
            {
                jobs = jobs.Where(j => j.CategoryId == categoryId);
            }

            var pagedJobs = jobs
                .OrderByDescending(j => j.CreatedAt)
                .ToPagedList(Math.Max(page, 1), PageSize);

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_JobTable", pagedJobs);
            }

            ViewBag.Categories = new SelectList(
                await _db.Categories.AsNoTracking().OrderBy(c => c.Name).ToListAsync(), "Id", "Name", categoryId);
            ViewBag.StatusOptions = new SelectList(new[]
            {
                new { Value = "", Text = "Tất cả trạng thái" },
                new { Value = "open", Text = "Đang hiển thị" },
                new { Value = "expired", Text = "Hết hạn" },
                new { Value = "closed", Text = "Đã đóng" }
            }, "Value", "Text", status);

            return View(pagedJobs);
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
            return RedirectToAction(nameof(Index));
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
            job.Description = _sanitizer.Sanitize(model.Description);
            job.Requirements = _sanitizer.Sanitize(model.Requirements);

            await _db.SaveChangesAsync();

            TempData["Success"] = "Cập nhật tin tuyển dụng thành công.";
            return RedirectToAction(nameof(Index));
        }

        // ===== M2-2: Đóng / Mở lại tin và Xoá tin =====

        // POST: /Employer/Jobs/ToggleActive/{id}
        // Đang mở thì đóng, đang đóng thì mở lại (đổi IsActive)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleActive(string id)
        {
            var companyId = await GetCompanyIdAsync();
            var job = await _db.JobPostings
                .FirstOrDefaultAsync(j => j.Id == id && j.CompanyId == companyId);

            if (job == null)
            {
                return NotFound();
            }

            // Mở lại tin đã quá hạn thì ứng viên vẫn không nộp được, nên bắt HR sửa hạn nộp trước
            var dangDong = !job.IsActive;
            if (dangDong && job.Deadline.HasValue && job.Deadline.Value.Date < DateTime.Today)
            {
                TempData["Error"] = "Tin đã quá hạn nộp hồ sơ. Hãy sửa hạn nộp trước khi mở lại.";
                return RedirectToAction(nameof(Edit), new { id });
            }

            job.IsActive = !job.IsActive;
            await _db.SaveChangesAsync();

            TempData["Success"] = job.IsActive ? "Đã mở lại tin tuyển dụng." : "Đã đóng tin tuyển dụng.";
            return RedirectToAction(nameof(Index));
        }
    }
}
