using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BTL_LTW_DOTJOB.Models
{
    [Table("Company")]
    public class Company
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required(ErrorMessage = "Tên công ty không được để trống")]
        [StringLength(255, ErrorMessage = "Tên công ty không được vượt quá 255 ký tự")]
        public string CompanyName { get; set; } = string.Empty;

        [StringLength(255)]
        public string WorkLocation { get; set; }

        [StringLength(500)]
        public string CompanyAddress { get; set; }

        [StringLength(255)]
        public string? Logo { get; set; }

        public string? Description { get; set; }

        [Url(ErrorMessage = "Website không đúng định dạng đường dẫn")]
        [StringLength(255)]
        public string? Website { get; set; }

        //
        [Required]
        public string UserId { get; set; } = string.Empty;

        [ForeignKey("UserId")]
        public virtual User? User { get; set; }

        public virtual ICollection<JobPosting> JobPostings { get; set; } = new List<JobPosting>();
    }
}