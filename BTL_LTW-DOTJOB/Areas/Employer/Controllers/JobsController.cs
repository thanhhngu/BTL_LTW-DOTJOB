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

        private readonly HtmlSanitizer _sanitizer = new();

        private const int PageSize = 10;

        public JobsController(AppDbContext db)
        {
            _db = db;
        }

        private async Task<string?> GetCompanyIdAsync()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return await _db.Companies
                .Where(c => c.UserId == userId)
                .Select(c => c.Id)
                .FirstOrDefaultAsync();
        }

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

        private bool IsAjax => Request.Headers["X-Requested-With"] == "XMLHttpRequest";

        private IActionResult Done(bool success, string message, string action = nameof(Index), object? routeValues = null)
        {
            var toIndex = action == nameof(Index);

            if (IsAjax)
            {
                if (!toIndex)
                {
                    TempData[success ? "Success" : "Error"] = message;
                }
                return Json(new { success, message, redirectUrl = toIndex ? null : Url.Action(action, routeValues) });
            }

            TempData[success ? "Success" : "Error"] = message;
            return RedirectToAction(action, routeValues);
        }

        [HttpGet]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public async Task<IActionResult> Index(string? keyword, string? status, string? categoryId, int page = 1)
        {
            var companyId = await GetCompanyIdAsync();
            if (companyId == null)
            {
                return Forbid();
            }

            var today = DateTime.Today;

            var jobs = _db.JobPostings
                .AsNoTracking()
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

            var kw = keyword?.Trim();
            if (!string.IsNullOrEmpty(kw))
            {
                jobs = jobs.Where(j => EF.Functions.Collate(j.Title, "Vietnamese_CI_AI").Contains(kw));
            }

            var totalCount = await jobs.CountAsync();
            var pageCount = Math.Max((int)Math.Ceiling(totalCount / (double)PageSize), 1);
            page = Math.Clamp(page, 1, pageCount);

            var pagedJobs = jobs
                .OrderByDescending(j => j.CreatedAt)
                .ThenBy(j => j.Id)
                .Select(j => new JobListItem
                {
                    Id = j.Id,
                    Title = j.Title,
                    CategoryName = j.Category != null ? j.Category.Name : null,
                    Location = j.Location,
                    CreatedAt = j.CreatedAt,
                    Deadline = j.Deadline,
                    IsActive = j.IsActive,
                    ApplicationCount = j.Applications.Count
                })
                .ToPagedList(page, PageSize);

            if (IsAjax)
            {
                return PartialView("_JobTable", pagedJobs);
            }

            ViewBag.Keyword = kw;

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

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var model = new JobCreateViewModel { Deadline = DateTime.Today.AddDays(30) };
            await FillOptionsAsync(model);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(JobCreateViewModel model)
        {
            var companyId = await GetCompanyIdAsync();
            if (companyId == null)
            {
                return Forbid();
            }

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
                Deadline = model.Deadline!.Value.Date.AddDays(1).AddSeconds(-1),
                Description = _sanitizer.Sanitize(model.Description),
                Requirements = _sanitizer.Sanitize(model.Requirements),
                CompanyId = companyId
            };

            _db.JobPostings.Add(job);
            await _db.SaveChangesAsync();

            TempData["Success"] = "Đăng tin tuyển dụng thành công.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            var companyId = await GetCompanyIdAsync();
            var job = await _db.JobPostings
                .AsNoTracking()
                .FirstOrDefaultAsync(j => j.Id == id && j.CompanyId == companyId);

            if (job == null)
            {
                return NotFound();
            }

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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, JobEditViewModel model)
        {
            var companyId = await GetCompanyIdAsync();
            var job = await _db.JobPostings
                .FirstOrDefaultAsync(j => j.Id == id && j.CompanyId == companyId);

            if (job == null)
            {
                return NotFound();
            }

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

            job.Title = model.Title.Trim();
            job.CategoryId = model.CategoryId;
            job.Location = model.Location;
            job.JobType = model.JobType;
            job.SalaryMin = model.SalaryMin;
            job.SalaryMax = model.SalaryMax;
            job.Deadline = model.Deadline!.Value.Date.AddDays(1).AddSeconds(-1);
            job.Description = _sanitizer.Sanitize(model.Description);
            job.Requirements = _sanitizer.Sanitize(model.Requirements);

            await _db.SaveChangesAsync();

            TempData["Success"] = "Cập nhật tin tuyển dụng thành công.";
            return RedirectToAction(nameof(Index));
        }

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

            var dangDong = !job.IsActive;
            if (dangDong && job.Deadline.HasValue && job.Deadline.Value.Date < DateTime.Today)
            {
                return Done(false, "Tin đã quá hạn nộp hồ sơ. Hãy sửa hạn nộp trước khi mở lại.", nameof(Edit), new { id });
            }

            job.IsActive = !job.IsActive;
            await _db.SaveChangesAsync();

            return Done(true, job.IsActive ? "Đã mở lại tin tuyển dụng." : "Đã đóng tin tuyển dụng.");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id)
        {
            var companyId = await GetCompanyIdAsync();
            var job = await _db.JobPostings
                .FirstOrDefaultAsync(j => j.Id == id && j.CompanyId == companyId);

            if (job == null)
            {
                return NotFound();
            }

            var coDon = await _db.Applications.AnyAsync(a => a.JobPostingId == id);
            if (coDon)
            {
                return Done(false, "Tin này đã có đơn ứng tuyển nên không thể xoá. Hãy dùng chức năng Đóng tin.");
            }

            _db.JobPostings.Remove(job);
            try
            {
                await _db.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                return Done(false, "Không thể xoá tin này vì đang có dữ liệu liên quan.");
            }

            return Done(true, "Đã xoá tin tuyển dụng.");
        }
    }
}
