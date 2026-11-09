namespace BTL_LTW_DOTJOB.ViewModels.Admin
{
    public class ReportDetailViewModel
    {
        public string Id { get; set; } = string.Empty;

        public string? ReporterName { get; set; }
        public string? ReporterEmail { get; set; }

        public string TargetType { get; set; } = string.Empty;

        public string? JobPostingId { get; set; }
        public string? JobTitle { get; set; }
        public string? CompanyName { get; set; }
        public bool JobIsActive { get; set; }

        public string? TargetUserId { get; set; }
        public string? TargetUserName { get; set; }
        public string? TargetUserEmail { get; set; }
        public bool TargetUserIsActive { get; set; }
        public string? TargetUserRole { get; set; }

        public string Content { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }

        public string Status { get; set; } = string.Empty;
        public string? AdminNote { get; set; }
        public DateTime? HandledAt { get; set; }
    }
}