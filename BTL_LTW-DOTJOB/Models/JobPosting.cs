using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BTL_LTW_DOTJOB.Models
{
    [Table("JobPosting")]
    public class JobPosting
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required(ErrorMessage = "Tiêu đề không được để trống")]
        [StringLength(255)]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mô tả công việc không được để trống")]
        public string Description { get; set; } = string.Empty; 

        public string? Requirements { get; set; }

        public decimal? SalaryMin { get; set; }
        public decimal? SalaryMax { get; set; }

        [StringLength(255)]
        public string? Location { get; set; }

        [StringLength(50)]
        public string? JobType { get; set; } 

        public DateTime? Deadline { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public bool IsActive { get; set; } = true;

        //
        [Required]
        public string CategoryId { get; set; } = string.Empty;
        [ForeignKey("CategoryId")]
        public virtual Category? Category { get; set; }

        //
        [Required]
        public string CompanyId { get; set; } = string.Empty;
        [ForeignKey("CompanyId")]
        public virtual Company? Company { get; set; }

        //
        public virtual ICollection<Application> Applications { get; set; } = new List<Application>();
    }
}