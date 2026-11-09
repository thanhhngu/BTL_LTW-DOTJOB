namespace BTL_LTW_DOTJOB.Services.Reports
{
    public interface IReportService
    {
        Task<(bool Success, string Message)> CreateJobReportAsync(string reporterId, string jobId, string content);

        Task<(bool Success, string Message)> CreateUserReportAsync(string reporterId, string applicationId, string content);
    }
}
