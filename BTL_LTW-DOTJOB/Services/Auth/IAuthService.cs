using BTL_LTW_DOTJOB.Models;
using BTL_LTW_DOTJOB.ViewModels.Auth;

namespace BTL_LTW_DOTJOB.Services.Auth
{
    public interface IAuthService
    {
        Task<User> AuthenticateAsync(string username, string password);
        Task<bool> RegisterUserAsync(RegisterViewModel model);
    }
}
