using BTL_LTW_DOTJOB.Models;
using BTL_LTW_DOTJOB.ViewModels.Job;

namespace BTL_LTW_DOTJOB.Services.Job
{
    public interface IJobService
    {
        Task<JobPosting?> GetJobByIdAsync(string id);

        Task<SearchJobViewModel> SearchJobsAsync(SearchJobViewModel filter);

        Task FillFilterOptionsAsync(SearchJobViewModel model);
    }
}
