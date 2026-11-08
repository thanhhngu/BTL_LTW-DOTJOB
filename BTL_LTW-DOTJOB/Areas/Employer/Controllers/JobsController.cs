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

        // Request gửi bằng fetch kèm header X-Requested-With: XMLHttpRequest
        private bool IsAjax => Request.Headers["X-Requested-With"] == "XMLHttpRequest";

        // Kết quả của thao tác đóng/mở/xoá: AJAX nhận JSON (JS tự hiện thông báo và tải lại danh sách
        // đúng trang đang xem), form thường thì lưu TempData rồi chuyển hướng như cũ
        private IActionResult Done(bool success, string message, string action = nameof(Index), object? routeValues = null)
        {
            var toIndex = action == nameof(Index);

            if (IsAjax)
            {
                // Nếu phải sang trang khác (vd: Edit) thì trang đó hiện thông báo từ TempData
                if (!toIndex)
                {
                    TempData[success ? "Success" : "Error"] = message;
                }
                return Json(new { success, message, redirectUrl = toIndex ? null : Url.Action(action, routeValues) });
            }

            TempData[success ? "Success" : "Error"] = message;
            return RedirectToAction(action, routeValues);
        }

        // ===== M1-3: Danh sách tin của HR =====

        // GET: /Employer/Jobs
        // Gọi bằng fetch (header X-Requested-With) thì chỉ trả bảng tin, không kèm layout
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

            // Chỉ lấy tin của công ty mình
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

            // Tìm theo tiêu đề, không phân biệt hoa thường và dấu (giống ô tìm kiếm phía ứng viên)
            var kw = keyword?.Trim();
            if (!string.IsNullOrEmpty(kw))
            {
                jobs = jobs.Where(j => EF.Functions.Collate(j.Title, "Vietnamese_CI_AI").Contains(kw));
            }

            // Giữ số trang trong khoảng hợp lệ (vd: vừa xoá hết tin ở trang cuối thì lùi về trang trước)
            var totalCount = await jobs.CountAsync();
            var pageCount = Math.Max((int)Math.Ceiling(totalCount / (double)PageSize), 1);
            page = Math.Clamp(page, 1, pageCount);

            // ThenBy(Id) giữ thứ tự ổn định khi nhiều tin cùng giờ đăng; Select chỉ lấy cột cần dùng và đếm số CV bằng SQL
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
                return Done(false, "Tin đã quá hạn nộp hồ sơ. Hãy sửa hạn nộp trước khi mở lại.", nameof(Edit), new { id });
            }

            job.IsActive = !job.IsActive;
            await _db.SaveChangesAsync();

            return Done(true, job.IsActive ? "Đã mở lại tin tuyển dụng." : "Đã đóng tin tuyển dụng.");
        }

        // POST: /Employer/Jobs/Delete/{id}
        // Chỉ xoá khi tin chưa có đơn ứng tuyển (xoá tin sẽ xoá luôn đơn, ứng viên mất lịch sử)
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
                // Lượt "lưu tin" của ứng viên (SavedJob) bị xoá theo vì đã cấu hình Cascade
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
