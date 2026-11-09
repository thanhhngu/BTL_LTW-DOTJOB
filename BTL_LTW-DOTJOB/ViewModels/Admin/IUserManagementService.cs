using BTL_LTW_DOTJOB.ViewModels.Admin;

namespace BTL_LTW_DOTJOB.Services.Admin
{
    public interface IUserManagementService
    {
        Task<UserListViewModel> GetPagedAsync(string? keyword, string? role, string? status, int page);

        Task<(bool Success, string Message)> ToggleLockAsync(string userId, string currentAdminId);
    }
}