using BTL_LTW_DOTJOB.Data;
using BTL_LTW_DOTJOB.Models;
using Microsoft.EntityFrameworkCore;

namespace BTL_LTW_DOTJOB.Services.Reports
{
    public class ReportService : IReportService
    {
        private readonly AppDbContext _db;

        public ReportService(AppDbContext db)
        {
            _db = db;
        }

        public async Task<(bool Success, string Message)> CreateJobReportAsync(string reporterId, string jobId, string content)
        {
            content = content.Trim();
            if (content.Length < 10 || content.Length > 1000)
                return (false, "Nội dung báo cáo từ 10 đến 1000 ký tự.");

            // Chỉ báo cáo được tin đang hiển thị
            var jobExists = await _db.JobPostings.AnyAsync(j => j.Id == jobId && j.IsActive);
            if (!jobExists)
                return (false, "Tin tuyển dụng không tồn tại hoặc đã bị ẩn.");

            // Chặn gửi lặp: đã có báo cáo của người này cho tin này mà Admin chưa xử lý
            var alreadyReported = await _db.Reports.AnyAsync(r =>
                r.ReporterId == reporterId
                && r.JobPostingId == jobId
                && r.Status == ReportStatus.Pending);

            if (alreadyReported)
                return (false, "Bạn đã báo cáo tin này, quản trị viên đang xem xét.");

            _db.Reports.Add(new Report
            {
                ReporterId = reporterId,
                TargetType = ReportTargetType.JobPosting,
                JobPostingId = jobId,
                Content = content
            });

            try
            {
                await _db.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                Console.WriteLine(ex.Message);
                return (false, "Không thể gửi báo cáo. Vui lòng thử lại.");
            }

            return (true, "Đã gửi báo cáo. Cảm ơn bạn đã góp ý!");
        }
    }
}
