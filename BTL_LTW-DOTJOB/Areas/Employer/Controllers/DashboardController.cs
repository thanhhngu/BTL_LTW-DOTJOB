using BTL_LTW_DOTJOB.Areas.Employer.ViewModels;
using BTL_LTW_DOTJOB.Data;
using BTL_LTW_DOTJOB.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace BTL_LTW_DOTJOB.Areas.Employer.Controllers
{
    [Area("Employer")]
    [Authorize(Roles = "Employer")]
    public class DashboardController : Controller
    {
        private readonly AppDbContext _db;

        public DashboardController(AppDbContext db)
        {
            _db = db;
        }

        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var company = await _db.Companies
                .AsNoTracking()
                .Where(c => c.UserId == userId)
                .Select(c => new { c.Id, c.CompanyName })
                .FirstOrDefaultAsync();

            if (company == null)
            {
                return Forbid();
            }

            var today = DateTime.Today;

            var applications = _db.Applications
                .AsNoTracking()
                .Where(a => a.JobPosting!.CompanyId == company.Id);

            var model = new DashboardViewModel
            {
                CompanyName = company.CompanyName,
                OpenJobs = await _db.JobPostings.CountAsync(j =>
                    j.CompanyId == company.Id && j.IsActive && (j.Deadline == null || j.Deadline >= today)),
                TotalApplications = await applications.CountAsync(),
                PendingApplications = await applications.CountAsync(a => a.Status == ApplicationStatus.Pending),
                RecentApplications = await applications
                    .OrderByDescending(a => a.AppliedDate)
                    .Take(5)
                    .Select(a => new RecentApplicationItem
                    {
                        CandidateName = a.User!.FullName,
                        JobTitle = a.JobPosting!.Title,
                        AppliedDate = a.AppliedDate,
                        Status = a.Status
                    })
                    .ToListAsync()
            };

            return View(model);
        }
    }
}
