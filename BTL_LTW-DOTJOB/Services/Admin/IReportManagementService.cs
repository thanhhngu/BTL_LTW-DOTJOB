using BTL_LTW_DOTJOB.ViewModels.Admin;

namespace BTL_LTW_DOTJOB.Services.Admin
{
    public interface IReportManagementService
    {
        // Danh sách báo cáo theo bộ lọc, có phân trang
        Task<ReportListViewModel> GetPagedAsync(string? status, string? targetType, int page);

        // Chi tiết một báo cáo (null nếu không tồn tại)
        Task<ReportDetailViewModel?> GetDetailAsync(string id);

        // Admin xử lý báo cáo. Trả về (thành công?, thông báo)
        Task<(bool Success, string Message)> HandleAsync(ReportHandleViewModel model);
        // hiện lại tin tuyển dụng đã bị ẩn (từ trang chi tiết báo cáo)
        Task<(bool Success, string Message)> RestoreJobAsync(string reportId);
    }
}