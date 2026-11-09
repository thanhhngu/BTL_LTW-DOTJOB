namespace BTL_LTW_DOTJOB.ViewModels.Admin
{
    // Một dòng trong bảng danh sách người dùng
    public class UserListItemViewModel
    {
        public string Id { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string RoleName { get; set; } = string.Empty; // Admin / Employer / Candidate
        public bool IsActive { get; set; }                   // true = đang hoạt động, false = bị khóa
        public DateTime CreatedAt { get; set; }
    }
}