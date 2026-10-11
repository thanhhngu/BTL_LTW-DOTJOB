using BTL_LTW_DOTJOB.Models;

namespace BTL_LTW_DOTJOB.Services.Job
{
    public interface IJobService
    {
        Task<JobPosting?> GetJobByIdAsync(string id);
    }
}
