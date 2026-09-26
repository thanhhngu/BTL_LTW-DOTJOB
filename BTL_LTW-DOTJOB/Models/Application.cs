using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BTL_LTW_DOTJOB.Models
{
    [Table("Application")]
    public class Application
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        // 
        [Required]
        public string JobPostingId { get; set; } = string.Empty;
        [ForeignKey("JobPostingId")]
        public virtual JobPosting? JobPosting { get; set; }

        // 
        [Required]
        public string UserId { get; set; } = string.Empty;
        [ForeignKey("UserId")]
        public virtual User? User { get; set; }

        [Required(ErrorMessage = "Bạn phải tải lên CV")]
        [StringLength(255)]
        public string CvFilePath { get; set; } = string.Empty;

        public string? CoverLetter { get; set; }

        public DateTime AppliedDate { get; set; } = DateTime.UtcNow;

        [StringLength(50)]
        public string Status { get; set; } = "Pending"; // Trạng thái: Pending, Accepted, Rejected...
    }
}