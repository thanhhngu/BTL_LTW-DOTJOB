using BTL_LTW_DOTJOB.ViewModels.SavedJobs;

namespace BTL_LTW_DOTJOB.Services.SavedJobs
{
    public interface ISavedJobService
    {
        // Bấm trái tim: chưa lưu thì lưu, đã lưu thì bỏ lưu. Trả về trạng thái mới và tổng số tin đã lưu
        Task<(bool Success, bool Saved, int Count, string Message)> ToggleAsync(string jobId, string userId);

        // Danh sách tin đã lưu của chính Candidate đang đăng nhập (phân trang)
        Task<MySavedJobsViewModel> GetMySavedJobsAsync(string userId, int page);

        // Tập Id các tin đã lưu để View tô màu trái tim. userId = null (chưa đăng nhập) thì trả về tập rỗng
        Task<HashSet<string>> GetSavedJobIdsAsync(string? userId);
    }
}
