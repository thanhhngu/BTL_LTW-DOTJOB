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

        public async Task<(bool Success, string Message)> CreateUserReportAsync(string reporterId, string applicationId, string content)
        {
            content = content.Trim();
            if (content.Length < 10 || content.Length > 1000)
                return (false, "Nội dung báo cáo từ 10 đến 1000 ký tự.");

            // Đơn phải nộp vào tin của công ty do chính HR này sở hữu (không tin Id ứng viên từ client)
            var targetUserId = await _db.Applications
                .AsNoTracking()
                .Where(a => a.Id == applicationId && a.JobPosting!.Company!.UserId == reporterId)
                .Select(a => a.UserId)
                .FirstOrDefaultAsync();

            if (targetUserId == null)
                return (false, "Đơn ứng tuyển không tồn tại hoặc không thuộc công ty của bạn.");

            // Tài khoản ứng viên đã bị khóa thì không cần báo cáo nữa
            var targetActive = await _db.Users.AnyAsync(u => u.Id == targetUserId && u.IsActive);
            if (!targetActive)
                return (false, "Tài khoản ứng viên này đã bị khóa.");

            // Chặn gửi lặp: HR này đã báo cáo ứng viên này mà Admin chưa xử lý
            var alreadyReported = await _db.Reports.AnyAsync(r =>
                r.ReporterId == reporterId
                && r.TargetUserId == targetUserId
                && r.Status == ReportStatus.Pending);

            if (alreadyReported)
                return (false, "Bạn đã báo cáo ứng viên này, quản trị viên đang xem xét.");

            _db.Reports.Add(new Report
            {
                ReporterId = reporterId,
                TargetType = ReportTargetType.User,
                TargetUserId = targetUserId,
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

            return (true, "Đã gửi báo cáo ứng viên tới quản trị viên.");
        }
    }
}
