namespace BTL_LTW_DOTJOB.Areas.Employer.ViewModels
{
    public class DashboardViewModel
    {
        public string CompanyName { get; set; } = string.Empty;
        public int OpenJobs { get; set; }
        public int TotalApplications { get; set; }
        public int PendingApplications { get; set; }
        public List<RecentApplicationItem> RecentApplications { get; set; } = new();
    }

    public class RecentApplicationItem
    {
        public string CandidateName { get; set; } = string.Empty;
        public string JobTitle { get; set; } = string.Empty;
        public DateTime AppliedDate { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}
