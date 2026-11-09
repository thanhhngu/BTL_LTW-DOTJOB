namespace BTL_LTW_DOTJOB.ViewModels.Admin
{
    // Một dòng trong bảng danh sách báo cáo
    public class ReportListItemViewModel
    {
        public string Id { get; set; } = string.Empty;
        public string? ReporterName { get; set; }
        public string? ReporterEmail { get; set; }

        public string TargetType { get; set; } = string.Empty; // JobPosting / User
        public string? TargetTitle { get; set; }  // Tiêu đề tin hoặc tên người bị báo cáo
        public string? TargetSub { get; set; }    // Tên công ty hoặc email người bị báo cáo

        public string Content { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }   // Giờ UTC lưu trong DB
        public string Status { get; set; } = string.Empty;

        // Rút gọn nội dung để hiển thị trong bảng
        public string ContentPreview => Content.Length > 80 ? Content.Substring(0, 80) + "…" : Content;
    }
}