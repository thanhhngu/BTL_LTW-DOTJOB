using System.ComponentModel.DataAnnotations;

namespace BTL_LTW_DOTJOB.ViewModels.Auth
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập email.")]
        [EmailAddress(ErrorMessage = "Định dạng email không hợp lệ.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu.")]
        [MinLength(6, ErrorMessage = "Mật khẩu tối thiểu 6 ký tự.")]
        public string Password { get; set; }

    }
}
