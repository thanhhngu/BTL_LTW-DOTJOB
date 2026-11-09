using BTL_LTW_DOTJOB.ViewModels.SavedJobs;

namespace BTL_LTW_DOTJOB.Services.SavedJobs
{
    public interface ISavedJobService
    {
        Task<(bool Success, bool Saved, int Count, string Message)> ToggleAsync(string jobId, string userId);

        Task<MySavedJobsViewModel> GetMySavedJobsAsync(string userId, int page);

        Task<HashSet<string>> GetSavedJobIdsAsync(string? userId);
    }
}
