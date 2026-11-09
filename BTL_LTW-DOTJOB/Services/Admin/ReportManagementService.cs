using BTL_LTW_DOTJOB.Data;
using BTL_LTW_DOTJOB.Models;
using BTL_LTW_DOTJOB.ViewModels.Admin;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace BTL_LTW_DOTJOB.Services.Admin
{
    public class ReportManagementService : IReportManagementService
    {
        private readonly AppDbContext _db;
        private readonly IMemoryCache _cache;

        public ReportManagementService(AppDbContext db, IMemoryCache cache)
        {
            _db = db;
            _cache = cache;
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
                    TargetUserIsActive = r.TargetUser != null && r.TargetUser.IsActive,
                    TargetUserRole = r.TargetUser!.Role!.Name,

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
                .Include(r => r.TargetUser).ThenInclude(u => u!.Role)
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

            // Khóa tài khoản chỉ áp dụng khi chấp nhận báo cáo về người dùng; không khóa được Admin
            var banned = false;
            if (model.BanUser)
            {
                if (model.Status != ReportStatus.Resolved
                    || report.TargetType != ReportTargetType.User
                    || report.TargetUser == null)
                {
                    return (false, "Chỉ khóa được tài khoản khi chấp nhận báo cáo về người dùng.");
                }
                if (report.TargetUser.Role?.Name == "Admin")
                {
                    return (false, "Không thể khóa tài khoản Admin.");
                }

                if (report.TargetUser.IsActive)
                {
                    report.TargetUser.IsActive = false;
                    banned = true;

                    // Các báo cáo khác đang chờ về cùng tài khoản này coi như đã xử lý
                    var others = await _db.Reports
                        .Where(r => r.TargetUserId == report.TargetUserId && r.Status == ReportStatus.Pending && r.Id != report.Id)
                        .ToListAsync();
                    foreach (var other in others)
                    {
                        other.Status = ReportStatus.Resolved;
                        other.AdminNote = "Tài khoản đã bị khóa theo báo cáo khác.";
                        other.HandledAt = DateTime.UtcNow;
                    }
                }
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

            // Người vừa bị khóa bị đăng xuất ngay ở request kế tiếp (xem OnValidatePrincipal trong Program.cs)
            if (banned) _cache.Remove($"user-active:{report.TargetUserId}");

            var message = $"Đã cập nhật báo cáo: {ReportStatus.Label(report.Status)}.";
            if (hidden) message += " Tin tuyển dụng đã được ẩn.";
            if (banned) message += $" Đã khóa tài khoản {report.TargetUser!.Email}.";
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