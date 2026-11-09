using BTL_LTW_DOTJOB.Models;

namespace BTL_LTW_DOTJOB.ViewModels.Job
{
    public class SearchJobViewModel
    {
        public string? Keyword { get; set; }
        public string? CategoryId { get; set; }
        public string? Location { get; set; }
        public string? SalaryRange { get; set; }
        public string? JobType { get; set; }
        public int Page { get; set; } = 1;

        public List<JobPosting> Items { get; set; } = new();
        public int PageCount { get; set; } = 1;
        public int TotalCount { get; set; }

        public List<Category> Categories { get; set; } = new();
        public List<string> Locations { get; set; } = new();
        public List<string> JobTypes { get; set; } = new();

        public bool HasFilter =>
            !string.IsNullOrWhiteSpace(Keyword)
            || !string.IsNullOrEmpty(CategoryId)
            || !string.IsNullOrEmpty(Location)
            || !string.IsNullOrEmpty(SalaryRange)
            || !string.IsNullOrEmpty(JobType);
    }
}
