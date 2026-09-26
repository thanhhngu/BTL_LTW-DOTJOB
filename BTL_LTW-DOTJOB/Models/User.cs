using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using static System.Net.Mime.MediaTypeNames;

namespace BTL_LTW_DOTJOB.Models
{
    [Table("User")]
    public class User
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required(ErrorMessage = "Họ và tên không được để trống")]
        [StringLength(100, ErrorMessage = "Họ và tên không được vượt quá 100 ký tự")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email không được để trống")]
        [EmailAddress(ErrorMessage = "Email không đúng định dạng")]
        [StringLength(100)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(255)]
        public string PasswordHash { get; set; } = string.Empty;

        [StringLength(255)]
        public string? Avatar { get; set; }

        [StringLength(20)]
        [Phone(ErrorMessage = "Số điện thoại không đúng định dạng")] 
        public string Phone { get; set; }

        [Required]
        public string RoleId { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public bool IsActive { get; set; } = true;


        //relation
        public virtual Company? Company { get; set; }
        
        [ForeignKey("RoleId")]
        public virtual Role? Role { get; set; }

        public virtual ICollection<Application> Applications { get; set; } = new List<Application>();
    }
}