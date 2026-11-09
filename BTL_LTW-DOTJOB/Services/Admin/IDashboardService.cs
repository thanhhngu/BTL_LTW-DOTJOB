using BTL_LTW_DOTJOB.ViewModels.Admin;

namespace BTL_LTW_DOTJOB.Services.Admin
{
    public interface IDashboardService
    {
        // Lấy toàn bộ số liệu thống kê cho trang Tổng quan
        Task<DashboardViewModel> GetStatsAsync();
    }
}
