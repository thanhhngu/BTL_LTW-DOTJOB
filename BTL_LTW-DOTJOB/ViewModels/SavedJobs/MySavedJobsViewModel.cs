using BTL_LTW_DOTJOB.Models;

namespace BTL_LTW_DOTJOB.ViewModels.SavedJobs
{
    public class MySavedJobsViewModel
    {
        public List<JobPosting> Items { get; set; } = new();
        public int Page { get; set; } = 1;
        public int PageCount { get; set; } = 1;
        public int TotalCount { get; set; }
    }
}
