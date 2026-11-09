using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BTL_LTW_DOTJOB.Models
{
    // Báo cáo / góp ý dùng chung cho cả hệ thống (1 bảng cho mọi vai trò):
    //  - Candidate báo cáo tin tuyển dụng  => TargetType = JobPosting, điền JobPostingId
    //  - Employer báo cáo ứng viên         => TargetType = User,       điền TargetUserId
    //  - Admin chỉ đọc và xử lý            => cập nhật Status, AdminNote, HandledAt
    // Chỉ điền ĐÚNG MỘT trong hai cột đích (JobPostingId hoặc TargetUserId), được kiểm tra bằng CHECK ở DB
    [Table("Report")]
    public class Report
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        // Người báo cáo
        [Required]
        public string ReporterId { get; set; } = string.Empty;
        [ForeignKey("ReporterId")]
        public virtual User? Reporter { get; set; }

        // Loại đối tượng bị báo cáo: xem ReportTargetType
        [Required]
        [StringLength(20)]
        public string TargetType { get; set; } = ReportTargetType.JobPosting;

        // Đối tượng bị báo cáo (chỉ 1 trong 2 cột có giá trị)
        public string? JobPostingId { get; set; }
        [ForeignKey("JobPostingId")]
        public virtual JobPosting? JobPosting { get; set; }

        public string? TargetUserId { get; set; }
        [ForeignKey("TargetUserId")]
        public virtual User? TargetUser { get; set; }

        // Nội dung báo cáo do người dùng nhập
        [Required]
        [StringLength(1000)]
        public string Content { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Phần dành cho Admin xử lý
        [Required]
        [StringLength(20)]
        public string Status { get; set; } = ReportStatus.Pending;

        [StringLength(1000)]
        public string? AdminNote { get; set; }

        public DateTime? HandledAt { get; set; }
    }

    // Giá trị cột Report.TargetType
    public static class ReportTargetType
    {
        public const string JobPosting = "JobPosting";
        public const string User = "User";
    }

    // Giá trị cột Report.Status
    public static class ReportStatus
    {
        public const string Pending = "Pending";     // Chờ xử lý
        public const string Resolved = "Resolved";   // Đã xử lý
        public const string Dismissed = "Dismissed"; // Bác bỏ (báo cáo không hợp lệ)

        public static readonly string[] All = { Pending, Resolved, Dismissed };

        public static string Label(string status) => status switch
        {
            Resolved => "Đã xử lý",
            Dismissed => "Đã bác bỏ",
            _ => "Chờ xử lý"
        };
    }
}
