using BTL_LTW_DOTJOB.ViewModels.Admin;

namespace BTL_LTW_DOTJOB.Services.Admin
{
    public interface IDashboardService
    {
        Task<DashboardViewModel> GetStatsAsync();
    }
}
