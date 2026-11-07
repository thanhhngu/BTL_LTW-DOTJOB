using BTL_LTW_DOTJOB.ViewModels.Profile;

namespace BTL_LTW_DOTJOB.Services.Profile
{
    public interface IProfileService
    {
        // Hồ sơ của chính người đang đăng nhập (null nếu tài khoản không còn tồn tại)
        Task<ProfileViewModel?> GetProfileAsync(string userId);

        // Điền lại phần chỉ-đọc (email, vai trò, ảnh) khi form bị trả về vì lỗi nhập liệu
        Task FillDisplayInfoAsync(ProfileViewModel model, string userId);

        // Cập nhật hồ sơ. Employer sửa thêm được thông tin công ty, các vai trò khác bỏ qua phần đó
        Task<(bool Success, string Message)> UpdateProfileAsync(string userId, ProfileViewModel model);

        Task<(bool Success, string Message)> ChangePasswordAsync(string userId, ChangePasswordViewModel model);
    }
}
