using BTL_LTW_DOTJOB.ViewModels.Auth;

namespace BTL_LTW_DOTJOB.Services.Auth
{
    public interface IAuthService
    {
        bool RegisterUser(RegisterViewModel model);
    }
}
