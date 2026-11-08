using BTL_LTW_DOTJOB.Models;
using BTL_LTW_DOTJOB.ViewModels.Application;

namespace BTL_LTW_DOTJOB.Services.Application
{
    public interface IApplicationService
    {
        Task<(bool Success, string Message)> ApplyJobAsync(ApplyJobViewModel model, string userId);

        // Candidate này đã nộp hồ sơ vào tin này chưa (để trang chi tiết hiện nút "Đã ứng tuyển")
        Task<bool> HasAppliedAsync(string jobId, string userId);

        // Danh sách đơn ứng tuyển của chính Candidate đang đăng nhập (lọc theo trạng thái, phân trang)
        Task<MyApplicationsViewModel> GetMyApplicationsAsync(string userId, string? status, int page);

        // Lấy file CV của 1 đơn. Chỉ trả về khi người xem là chủ đơn (Candidate), Employer sở hữu tin hoặc Admin
        Task<(string PhysicalPath, string FileName)?> GetCvFileAsync(string applicationId, string userId, string role);
    }
}
