namespace BTL_LTW_DOTJOB.Services.Reports
{
    public interface IReportService
    {
        // Candidate báo cáo 1 tin tuyển dụng. Mỗi người chỉ có 1 báo cáo "Chờ xử lý" cho mỗi tin
        Task<(bool Success, string Message)> CreateJobReportAsync(string reporterId, string jobId, string content);
    }
}
