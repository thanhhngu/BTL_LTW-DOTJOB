namespace BTL_LTW_DOTJOB.Services.Reports
{
    public interface IReportService
    {
        // Candidate báo cáo 1 tin tuyển dụng. Mỗi người chỉ có 1 báo cáo "Chờ xử lý" cho mỗi tin
        Task<(bool Success, string Message)> CreateJobReportAsync(string reporterId, string jobId, string content);

        // Employer báo cáo 1 ứng viên (vd: nộp CV linh tinh, spam). Nhận Id đơn ứng tuyển thay vì Id ứng viên
        // để server tự kiểm tra đơn đó nộp vào tin của công ty mình. Mỗi HR chỉ có 1 báo cáo "Chờ xử lý" cho mỗi ứng viên
        Task<(bool Success, string Message)> CreateUserReportAsync(string reporterId, string applicationId, string content);
    }
}
