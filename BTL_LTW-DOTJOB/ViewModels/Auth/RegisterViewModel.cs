using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BTL_LTW_DOTJOB.ViewModels.Auth
{
    public class RegisterViewModel : IValidatableObject
    {
        [Required(ErrorMessage = "Vui lòng chọn vai trò.")]
        public string Role { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên đầy đủ của bạn.")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập email.")]
        [EmailAddress(ErrorMessage = "Định dạng email không hợp lệ.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập số điện thoại.")]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ.")]
        //[RegularExpression(@"^(03|05|07|08|09)\d{8}$", ErrorMessage = "Số điện thoại phải gồm 10 số và bắt đầu bằng 03, 05, 07, 08 hoặc 09.")]
        public string Phone { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu.")]
        [MinLength(6, ErrorMessage = "Mật khẩu tối thiểu 6 ký tự.")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập lại mật khẩu.")]
        [Compare("Password", ErrorMessage = "Mật khẩu nhập lại không khớp.")]
        public string PasswordConfirmation { get; set; }

        public string? CompanyName { get; set; }
        public string? WorkLocation { get; set; }
        public string? CompanyAddress { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (Role == "Employer")
            {
                if (string.IsNullOrWhiteSpace(CompanyName))
                {
                    yield return new ValidationResult("Vui lòng nhập tên công ty.", new[] { nameof(CompanyName) });
                }

                if (string.IsNullOrWhiteSpace(WorkLocation))
                {
                    yield return new ValidationResult("Vui lòng chọn khu vực làm việc.", new[] { nameof(WorkLocation) });
                }

                if (string.IsNullOrWhiteSpace(CompanyAddress))
                {
                    yield return new ValidationResult("Vui lòng nhập địa chỉ công ty.", new[] { nameof(CompanyAddress) });
                }
            }
        }
    }
}
