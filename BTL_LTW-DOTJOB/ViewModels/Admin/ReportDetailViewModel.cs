namespace BTL_LTW_DOTJOB.ViewModels.Admin
{
    // Dữ liệu trang chi tiết một báo cáo
    public class ReportDetailViewModel
    {
        public string Id { get; set; } = string.Empty;

        // Người báo cáo
        public string? ReporterName { get; set; }
        public string? ReporterEmail { get; set; }

        public string TargetType { get; set; } = string.Empty;

        // Nếu báo cáo một tin tuyển dụng
        public string? JobPostingId { get; set; }
        public string? JobTitle { get; set; }
        public string? CompanyName { get; set; }
        public bool JobIsActive { get; set; }   // Tin đang hiển thị hay đã bị ẩn

        // Nếu báo cáo một người dùng
        public string? TargetUserId { get; set; }
        public string? TargetUserName { get; set; }
        public string? TargetUserEmail { get; set; }
        public bool TargetUserIsActive { get; set; } // false = tài khoản đã bị khóa
        public string? TargetUserRole { get; set; }

        public string Content { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }

        // Phần xử lý
        public string Status { get; set; } = string.Empty;
        public string? AdminNote { get; set; }
        public DateTime? HandledAt { get; set; }
    }
}