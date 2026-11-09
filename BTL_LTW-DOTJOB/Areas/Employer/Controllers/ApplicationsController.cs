using BTL_LTW_DOTJOB.Data;
using BTL_LTW_DOTJOB.Models;
using BTL_LTW_DOTJOB.Services.Reports;
using BTL_LTW_DOTJOB.ViewModels.Reports;
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
    public class ApplicationsController : Controller
    {
        private readonly AppDbContext _db;
        private readonly IReportService _reportService;

        private const int PageSize = 10;

        public ApplicationsController(AppDbContext db, IReportService reportService)
        {
            _db = db;
            _reportService = reportService;
        }

        // Lấy Id công ty của HR đang đăng nhập (không nhận từ form hay URL)
        private async Task<string?> GetCompanyIdAsync()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return await _db.Companies
                .Where(c => c.UserId == userId)
                .Select(c => c.Id)
                .FirstOrDefaultAsync();
        }

        // GET: /Employer/Applications?jobId=...&status=...&page=...
        // Danh sách đơn ứng tuyển vào các tin của công ty mình, lọc theo tin và trạng thái.
        // Gọi bằng fetch (header X-Requested-With) thì chỉ trả bảng, không kèm layout
        [HttpGet]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public async Task<IActionResult> Index(string? jobId, string? status, int page = 1)
        {
            var companyId = await GetCompanyIdAsync();
            if (companyId == null)
            {
                return Forbid();
            }

            // Tin được chọn phải thuộc công ty mình, đổi jobId trên URL sang tin của HR khác sẽ bị NotFound
            if (!string.IsNullOrEmpty(jobId))
            {
                var jobIsMine = await _db.JobPostings.AnyAsync(j => j.Id == jobId && j.CompanyId == companyId);
                if (!jobIsMine)
                {
                    return NotFound();
                }
            }

            // Chỉ lấy đơn nộp vào tin của công ty mình
            var applications = _db.Applications
                .AsNoTracking()
                .Include(a => a.User)
                .Include(a => a.JobPosting)
                .Where(a => a.JobPosting!.CompanyId == companyId);

            if (!string.IsNullOrEmpty(jobId))
            {
                applications = applications.Where(a => a.JobPostingId == jobId);
            }

            if (!string.IsNullOrEmpty(status) && ApplicationStatus.All.Contains(status))
            {
                applications = applications.Where(a => a.Status == status);
            }

            // Giữ số trang trong khoảng hợp lệ (vd: ?page=999 thì về trang cuối thay vì hiện bảng trống không có phân trang)
            var totalCount = await applications.CountAsync();
            var pageCount = Math.Max((int)Math.Ceiling(totalCount / (double)PageSize), 1);
            page = Math.Clamp(page, 1, pageCount);

            var pagedApplications = applications
                .OrderByDescending(a => a.AppliedDate)
                .ToPagedList(page, PageSize);

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_ApplicationTable", pagedApplications);
            }

            ViewBag.Jobs = new SelectList(
                await _db.JobPostings
                    .AsNoTracking()
                    .Where(j => j.CompanyId == companyId)
                    .OrderByDescending(j => j.CreatedAt)
                    .ToListAsync(),
                "Id", "Title", jobId);

            ViewBag.StatusOptions = new SelectList(new[]
            {
                new { Value = "", Text = "Tất cả trạng thái" },
                new { Value = ApplicationStatus.Pending, Text = ApplicationStatus.Label(ApplicationStatus.Pending) },
                new { Value = ApplicationStatus.Accepted, Text = ApplicationStatus.Label(ApplicationStatus.Accepted) },
                new { Value = ApplicationStatus.Rejected, Text = ApplicationStatus.Label(ApplicationStatus.Rejected) }
            }, "Value", "Text", status);

            return View(pagedApplications);
        }

        // POST: /Employer/Applications/ReportCandidate/{id} - HR báo cáo ứng viên của 1 đơn ứng tuyển (vd: nộp CV linh tinh)
        // Gọi bằng fetch, anti-forgery token gửi trong header RequestVerificationToken. Chỉ Admin mới có quyền khóa tài khoản
        [HttpPost]
        public async Task<IActionResult> ReportCandidate(string id, [FromForm] CreateReportViewModel model)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("Vui lòng đăng nhập lại.");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState.Values.SelectMany(v => v.Errors).First().ErrorMessage);
            }

            var (success, message) = await _reportService.CreateUserReportAsync(userId, id, model.Content);

            if (!success)
            {
                return BadRequest(message);
            }

            return Ok(new { message });
        }
    }
}
