using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BTL_LTW_DOTJOB.Models
{
    [Table("Report")]
    public class Report
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required]
        public string ReporterId { get; set; } = string.Empty;
        [ForeignKey("ReporterId")]
        public virtual User? Reporter { get; set; }

        [Required]
        [StringLength(20)]
        public string TargetType { get; set; } = ReportTargetType.JobPosting;

        public string? JobPostingId { get; set; }
        [ForeignKey("JobPostingId")]
        public virtual JobPosting? JobPosting { get; set; }

        public string? TargetUserId { get; set; }
        [ForeignKey("TargetUserId")]
        public virtual User? TargetUser { get; set; }

        [Required]
        [StringLength(1000)]
        public string Content { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Required]
        [StringLength(20)]
        public string Status { get; set; } = ReportStatus.Pending;

        [StringLength(1000)]
        public string? AdminNote { get; set; }

        public DateTime? HandledAt { get; set; }
    }

    public static class ReportTargetType
    {
        public const string JobPosting = "JobPosting";
        public const string User = "User";
    }

    public static class ReportStatus
    {
        public const string Pending = "Pending";
        public const string Resolved = "Resolved";
        public const string Dismissed = "Dismissed";

        public static readonly string[] All = { Pending, Resolved, Dismissed };

        public static string Label(string status) => status switch
        {
            Resolved => "Đã xử lý",
            Dismissed => "Đã bác bỏ",
            _ => "Chờ xử lý"
        };
    }
}
