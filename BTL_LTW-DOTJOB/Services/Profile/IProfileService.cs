using BTL_LTW_DOTJOB.ViewModels.Profile;

namespace BTL_LTW_DOTJOB.Services.Profile
{
    public interface IProfileService
    {
        Task<ProfileViewModel?> GetProfileAsync(string userId);

        Task FillDisplayInfoAsync(ProfileViewModel model, string userId);

        Task<(bool Success, string Message)> UpdateProfileAsync(string userId, ProfileViewModel model);

        Task<(bool Success, string Message)> ChangePasswordAsync(string userId, ChangePasswordViewModel model);
    }
}
