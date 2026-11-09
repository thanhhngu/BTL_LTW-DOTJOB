namespace BTL_LTW_DOTJOB.ViewModels.Admin
{
    public class ReportListItemViewModel
    {
        public string Id { get; set; } = string.Empty;
        public string? ReporterName { get; set; }
        public string? ReporterEmail { get; set; }

        public string TargetType { get; set; } = string.Empty;
        public string? TargetTitle { get; set; }
        public string? TargetSub { get; set; }

        public string Content { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public string Status { get; set; } = string.Empty;

        public string ContentPreview => Content.Length > 80 ? Content.Substring(0, 80) + "…" : Content;
    }
}