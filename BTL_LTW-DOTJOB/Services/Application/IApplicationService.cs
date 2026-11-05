using BTL_LTW_DOTJOB.Models;
using BTL_LTW_DOTJOB.ViewModels.Application;

namespace BTL_LTW_DOTJOB.Services.Application
{
    public interface IApplicationService
    {
        Task<(bool Success, string Message)> ApplyJobAsync(ApplyJobViewModel model, string userId);

    }
}
