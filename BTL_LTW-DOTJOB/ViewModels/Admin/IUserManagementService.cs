using BTL_LTW_DOTJOB.ViewModels.Admin;

namespace BTL_LTW_DOTJOB.Services.Admin
{
    public interface IUserManagementService
    {
        // Lấy danh sách theo bộ lọc, có phân trang
        Task<UserListViewModel> GetPagedAsync(string? keyword, string? role, string? status, int page);

        // Khóa hoặc mở khóa tài khoản. Trả về (thành công?, thông báo)
        Task<(bool Success, string Message)> ToggleLockAsync(string userId, string currentAdminId);
    }
}