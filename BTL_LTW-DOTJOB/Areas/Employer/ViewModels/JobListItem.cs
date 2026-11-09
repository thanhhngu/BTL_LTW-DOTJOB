namespace BTL_LTW_DOTJOB.Areas.Employer.ViewModels
{
    public class JobListItem
    {
        public string Id { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string? CategoryName { get; set; }
        public string? Location { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? Deadline { get; set; }
        public bool IsActive { get; set; }
        public int ApplicationCount { get; set; }
    }
}
