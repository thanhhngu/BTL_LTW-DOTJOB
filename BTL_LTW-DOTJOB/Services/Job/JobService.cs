using BTL_LTW_DOTJOB.Data;
using BTL_LTW_DOTJOB.Models;

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
            return await _db.JobPostings.FindAsync(id);
        }
    }
}
