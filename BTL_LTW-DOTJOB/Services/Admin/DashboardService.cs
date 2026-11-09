using BTL_LTW_DOTJOB.Data;
using BTL_LTW_DOTJOB.Models;
using BTL_LTW_DOTJOB.ViewModels.Admin;
using Microsoft.EntityFrameworkCore;

namespace BTL_LTW_DOTJOB.Services.Admin
{
    public class DashboardService : IDashboardService
    {
        private readonly AppDbContext _db;

        public DashboardService(AppDbContext db)
        {
            _db = db;
        }

        public async Task<DashboardViewModel> GetStatsAsync()
        {
            return new DashboardViewModel
            {
                TotalUsers = await _db.Users.CountAsync(),
                TotalCandidates = await _db.Users.CountAsync(u => u.Role!.Name == "Candidate"),
                TotalEmployers = await _db.Users.CountAsync(u => u.Role!.Name == "Employer"),
                TotalJobs = await _db.JobPostings.CountAsync(),
                TotalApplications = await _db.Applications.CountAsync(),
                TotalCategories = await _db.Categories.CountAsync(),
                PendingReports = await _db.Reports.CountAsync(r => r.Status == ReportStatus.Pending),
                HiddenJobs = await _db.JobPostings.CountAsync(j => !j.IsActive)
            };
        }
    }
}