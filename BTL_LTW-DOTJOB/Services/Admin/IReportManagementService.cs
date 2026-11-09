using BTL_LTW_DOTJOB.ViewModels.Admin;

namespace BTL_LTW_DOTJOB.Services.Admin
{
    public interface IReportManagementService
    {
        Task<ReportListViewModel> GetPagedAsync(string? status, string? targetType, int page);

        Task<ReportDetailViewModel?> GetDetailAsync(string id);

        Task<(bool Success, string Message)> HandleAsync(ReportHandleViewModel model);
        Task<(bool Success, string Message)> RestoreJobAsync(string reportId);
    }
}