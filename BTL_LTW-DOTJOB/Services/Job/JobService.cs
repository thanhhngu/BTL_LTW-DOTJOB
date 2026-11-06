using BTL_LTW_DOTJOB.Data;
using BTL_LTW_DOTJOB.Models;
using Microsoft.EntityFrameworkCore;

namespace BTL_LTW_DOTJOB.Services.Job
{
    public class JobService : IJobService
    {
        private readonly AppDbContext _db;

        public JobService(AppDbContext db)
        {
            _db = db;
        }

        public async Task<JobPosting?> GetJobByIdAsync(string id)
        {
            // FindAsync không nạp navigation => phải Include Company, Category để trang chi tiết hiển thị đúng
            // Chỉ trả về tin đang hiển thị (IsActive)
            return await _db.JobPostings
                .AsNoTracking()
                .Include(job => job.Company)
                .Include(job => job.Category)
                .FirstOrDefaultAsync(job => job.Id == id && job.IsActive);
        }
    }
}
