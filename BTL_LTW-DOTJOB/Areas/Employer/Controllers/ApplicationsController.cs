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

        private async Task<string?> GetCompanyIdAsync()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return await _db.Companies
                .Where(c => c.UserId == userId)
                .Select(c => c.Id)
                .FirstOrDefaultAsync();
        }

        [HttpGet]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public async Task<IActionResult> Index(string? jobId, string? status, int page = 1)
        {
            var companyId = await GetCompanyIdAsync();
            if (companyId == null)
            {
                return Forbid();
            }

            if (!string.IsNullOrEmpty(jobId))
            {
                var jobIsMine = await _db.JobPostings.AnyAsync(j => j.Id == jobId && j.CompanyId == companyId);
                if (!jobIsMine)
                {
                    return NotFound();
                }
            }

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
