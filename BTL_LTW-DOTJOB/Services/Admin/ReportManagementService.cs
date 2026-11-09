using BTL_LTW_DOTJOB.Data;
using BTL_LTW_DOTJOB.Models;
using BTL_LTW_DOTJOB.ViewModels.Admin;
using Microsoft.EntityFrameworkCore;

namespace BTL_LTW_DOTJOB.Services.Admin
{
    public class ReportManagementService : IReportManagementService
    {
        private readonly AppDbContext _db;

        public ReportManagementService(AppDbContext db)
        {
            _db = db;
        }

        public async Task<ReportListViewModel> GetPagedAsync(string? status, string? targetType, int page)
        {
            var query = _db.Reports.AsNoTracking().AsQueryable();

            // Chỉ lọc khi giá trị hợp lệ (tránh người dùng tự sửa URL)
            if (!string.IsNullOrEmpty(status) && ReportStatus.All.Contains(status))
            {
                query = query.Where(r => r.Status == status);
            }
            if (targetType == ReportTargetType.JobPosting || targetType == ReportTargetType.User)
            {
                query = query.Where(r => r.TargetType == targetType);
            }

            var model = new ReportListViewModel
            {
                Status = status,
                TargetType = targetType,
                Page = page < 1 ? 1 : page,
                PendingCount = await _db.Reports.CountAsync(r => r.Status == ReportStatus.Pending)
            };

            model.TotalItems = await query.CountAsync();

            model.Reports = await query
                // Chờ xử lý đứng trước, trong mỗi nhóm thì mới nhất đứng trước
                .OrderBy(r => r.Status == ReportStatus.Pending ? 0 : 1)
                .ThenByDescending(r => r.CreatedAt)
                .Skip((model.Page - 1) * model.PageSize)
                .Take(model.PageSize)
                .Select(r => new ReportListItemViewModel
                {
                    Id = r.Id,
                    ReporterName = r.Reporter!.FullName,
                    ReporterEmail = r.Reporter!.Email,
                    TargetType = r.TargetType,
                    // Tùy loại báo cáo mà lấy tên từ tin hoặc từ người dùng
                    TargetTitle = r.TargetType == ReportTargetType.JobPosting
                        ? r.JobPosting!.Title
                        : r.TargetUser!.FullName,
                    TargetSub = r.TargetType == ReportTargetType.JobPosting
                        ? r.JobPosting!.Company!.CompanyName
                        : r.TargetUser!.Email,
                    Content = r.Content,
                    CreatedAt = r.CreatedAt,
                    Status = r.Status
                })
                .ToListAsync();

            return model;
        }

        public async Task<ReportDetailViewModel?> GetDetailAsync(string id)
        {
            return await _db.Reports
                .AsNoTracking()
                .Where(r => r.Id == id)
                .Select(r => new ReportDetailViewModel
                {
                    Id = r.Id,
                    ReporterName = r.Reporter!.FullName,
                    ReporterEmail = r.Reporter!.Email,
                    TargetType = r.TargetType,

                    JobPostingId = r.JobPostingId,
                    JobTitle = r.JobPosting!.Title,
                    CompanyName = r.JobPosting!.Company!.CompanyName,
                    JobIsActive = r.JobPosting != null && r.JobPosting.IsActive,

                    TargetUserId = r.TargetUserId,
                    TargetUserName = r.TargetUser!.FullName,
                    TargetUserEmail = r.TargetUser!.Email,

                    Content = r.Content,
                    CreatedAt = r.CreatedAt,
                    Status = r.Status,
                    AdminNote = r.AdminNote,
                    HandledAt = r.HandledAt
                })
                .FirstOrDefaultAsync();
        }

        public async Task<(bool Success, string Message)> HandleAsync(ReportHandleViewModel model)
        {
            // Admin chỉ được chuyển sang Đã xử lý hoặc Bác bỏ
            if (model.Status != ReportStatus.Resolved && model.Status != ReportStatus.Dismissed)
            {
                return (false, "Kết quả xử lý không hợp lệ.");
            }

            var report = await _db.Reports
                .Include(r => r.JobPosting)
                .FirstOrDefaultAsync(r => r.Id == model.Id);

            if (report == null)
            {
                return (false, "Báo cáo không tồn tại.");
            }

            // Giữ lịch sử: báo cáo đã xử lý thì không đổi lại
            if (report.Status != ReportStatus.Pending)
            {
                return (false, "Báo cáo này đã được xử lý trước đó.");
            }

            report.Status = model.Status;
            report.AdminNote = string.IsNullOrWhiteSpace(model.AdminNote) ? null : model.AdminNote.Trim();
            report.HandledAt = DateTime.UtcNow; // Lưu giờ UTC giống CreatedAt

            // Báo cáo tin hợp lệ + Admin chọn ẩn => tin biến mất khỏi trang công khai
            var hidden = false;
            if (model.HideJob
                && model.Status == ReportStatus.Resolved
                && report.TargetType == ReportTargetType.JobPosting
                && report.JobPosting != null
                && report.JobPosting.IsActive)
            {
                report.JobPosting.IsActive = false;
                hidden = true;
            }

            await _db.SaveChangesAsync();

            var message = $"Đã cập nhật báo cáo: {ReportStatus.Label(report.Status)}.";
            if (hidden) message += " Tin tuyển dụng đã được ẩn.";
            return (true, message);
        }
        // ===== Hiện lại tin đã ẩn =====
        public async Task<(bool Success, string Message)> RestoreJobAsync(string reportId)
        {
            var report = await _db.Reports
                .Include(r => r.JobPosting)
                .FirstOrDefaultAsync(r => r.Id == reportId);

            if (report == null)
            {
                return (false, "Báo cáo không tồn tại.");
            }

            // Chỉ báo cáo về tin tuyển dụng mới có tin để hiện lại
            if (report.TargetType != ReportTargetType.JobPosting || report.JobPosting == null)
            {
                return (false, "Báo cáo này không liên quan đến tin tuyển dụng.");
            }

            if (report.JobPosting.IsActive)
            {
                return (false, "Tin tuyển dụng đang hiển thị, không cần hiện lại.");
            }

            report.JobPosting.IsActive = true; // Tin xuất hiện lại ở trang công khai
            await _db.SaveChangesAsync();

            return (true, "Đã hiện lại tin tuyển dụng.");
        }
    }
}