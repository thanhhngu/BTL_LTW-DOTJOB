using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BTL_LTW_DOTJOB.Models
{
    [Table("SavedJob")]
    public class SavedJob
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required]
        public string UserId { get; set; } = string.Empty;
        [ForeignKey("UserId")]
        public virtual User? User { get; set; }

        [Required]
        public string JobPostingId { get; set; } = string.Empty;
        [ForeignKey("JobPostingId")]
        public virtual JobPosting? JobPosting { get; set; }

        public DateTime SavedAt { get; set; } = DateTime.UtcNow;
    }
}
