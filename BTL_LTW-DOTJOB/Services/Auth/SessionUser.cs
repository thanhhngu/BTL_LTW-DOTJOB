namespace BTL_LTW_DOTJOB.Services.Auth
{
    // Thông tin tối thiểu của người đăng nhập, lưu trong Session["User"].
    // Chỉ lưu dữ liệu cần dùng nhanh, KHÔNG lưu mật khẩu/PasswordHash.
    public class SessionUser
    {
        public string UserId { get; set; } = string.Empty; // User.Id (GUID dạng string)
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;   // Admin / Employer / Candidate
    }
}
