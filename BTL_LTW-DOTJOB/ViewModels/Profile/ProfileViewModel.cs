using System.ComponentModel.DataAnnotations;

namespace BTL_LTW_DOTJOB.ViewModels.Profile
{
    // Dữ liệu trang "Hồ sơ cá nhân" dùng chung cho Admin, Employer, Candidate.
    // Phần công ty chỉ có ý nghĩa với Employer
    public class ProfileViewModel : IValidatableObject
    {
        // ===== Chỉ hiển thị, luôn lấy từ DB / Claim, không nhận từ form =====
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string? AvatarPath { get; set; }
        public string? LogoPath { get; set; }

        // ===== Thông tin cá nhân (mọi vai trò đều sửa được) =====
        [Required(ErrorMessage = "Vui lòng nhập họ và tên.")]
        [StringLength(100, ErrorMessage = "Họ và tên không được vượt quá 100 ký tự.")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập số điện thoại.")]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ.")]
        [StringLength(20)]
        public string Phone { get; set; } = string.Empty;

        public IFormFile? AvatarFile { get; set; }

        // ===== Thông tin công ty (chỉ Employer) =====
        [StringLength(255, ErrorMessage = "Tên công ty không được vượt quá 255 ký tự.")]
        public string? CompanyName { get; set; }

        [StringLength(255)]
        public string? WorkLocation { get; set; }

        [StringLength(500, ErrorMessage = "Địa chỉ không được vượt quá 500 ký tự.")]
        public string? CompanyAddress { get; set; }

        [Url(ErrorMessage = "Website không đúng định dạng đường dẫn.")]
        [StringLength(255)]
        public string? Website { get; set; }

        public string? Description { get; set; }

        public IFormFile? LogoFile { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            // Role do server gán từ Claim trước khi kiểm tra, không tin giá trị client gửi lên
            if (Role == "Employer")
            {
                if (string.IsNullOrWhiteSpace(CompanyName))
                    yield return new ValidationResult("Vui lòng nhập tên công ty.", new[] { nameof(CompanyName) });

                if (string.IsNullOrWhiteSpace(WorkLocation))
                    yield return new ValidationResult("Vui lòng nhập khu vực làm việc.", new[] { nameof(WorkLocation) });

                if (string.IsNullOrWhiteSpace(CompanyAddress))
                    yield return new ValidationResult("Vui lòng nhập địa chỉ công ty.", new[] { nameof(CompanyAddress) });
            }
        }
    }
}
