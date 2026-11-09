using BTL_LTW_DOTJOB.Data;
using BTL_LTW_DOTJOB.Helpers;
using BTL_LTW_DOTJOB.Models;
using BTL_LTW_DOTJOB.ViewModels.Application;
using Microsoft.EntityFrameworkCore;

namespace BTL_LTW_DOTJOB.Services.Application
{
    public class ApplicationService : IApplicationService
    {
        private readonly AppDbContext _db;
        private readonly IWebHostEnvironment _env;
        private static readonly string[] AllowedExt = { ".pdf", ".doc", ".docx" };
        private const long MaxSize = 5 * 1024 * 1024;
        private const int PageSize = 10;
        public ApplicationService(AppDbContext db, IWebHostEnvironment env)
        {
            _env = env;
            _db = db;
        }
        public async Task<(bool Success, string Message)> ApplyJobAsync(ApplyJobViewModel model, string userId)
        {
            var file = model.CvFile;
            if (file == null || file.Length == 0)
                return (false, "File CV không hợp lệ.");

            if (file.Length > MaxSize)
                return (false, "File CV không được vượt quá 5MB.");

            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!AllowedExt.Contains(ext))
                return (false, "Chỉ chấp nhận file PDF, DOC, DOCX.");

            // Đuôi file có thể bị đổi tên (vd: virus.exe -> cv.pdf) nên kiểm tra thêm nội dung thật của file
            if (!await UploadSignature.MatchesExtensionAsync(file, ext))
                return (false, "Nội dung file không đúng định dạng PDF, DOC hoặc DOCX.");

            var job = await _db.JobPostings.FindAsync(model.Id);
            if (job == null || !job.IsActive)
                return (false, "Tin tuyển dụng không tồn tại hoặc đã bị ẩn.");

            // Hạn nộp do HR chọn theo giờ máy (23:59:59), nên so với giờ máy chứ không phải UTC
            if (job.Deadline < DateTime.Now)
                return (false, "Tin tuyển dụng đã hết hạn ứng tuyển.");

            // 2. Kiểm tra xem user đã ứng tuyển job này trước đó chưa
            if (await HasAppliedAsync(model.Id, userId))
                return (false, "Bạn đã nộp hồ sơ cho vị trí này rồi.");

            // 3. Lưu file CV vào AppData/cvs (nằm ngoài wwwroot nên không thể truy cập trực tiếp bằng URL)
            var uploadsFolder = Path.Combine(_env.ContentRootPath, "AppData", "cvs");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            var uniqueFileName = $"{Guid.NewGuid()}{ext}";
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // 4. Lưu thông tin vào Database
            var application = new Models.Application
            {
                JobPostingId = model.Id,
                UserId = userId,
                CvFilePath = $"cvs/{uniqueFileName}", // Đường dẫn tương đối trong App_Data, tải qua CvFileController
                CoverLetter = string.IsNullOrWhiteSpace(model.CoverLetter) ? null : model.CoverLetter.Trim(),
                AppliedDate = DateTime.UtcNow,
                Status = ApplicationStatus.Pending
            };

            try
            {
                _db.Applications.Add(application);
                await _db.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                // Lưu DB lỗi (vd: bấm nộp 2 lần cùng lúc bị trùng đơn) => xóa file vừa lưu để không bị file rác
                Console.WriteLine(ex.Message);
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
                return (false, "Không thể lưu hồ sơ (có thể bạn đã nộp vị trí này rồi). Vui lòng thử lại.");
            }

            return (true, "Nộp hồ sơ thành công!");
        }

        public async Task<bool> HasAppliedAsync(string jobId, string userId)
        {
            return await _db.Applications
                .AsNoTracking()
                .AnyAsync(a => a.JobPostingId == jobId && a.UserId == userId);
        }

        public async Task<MyApplicationsViewModel> GetMyApplicationsAsync(string userId, string? status, int page)
        {
            // Luôn lọc theo userId của người đăng nhập, không nhận userId từ client (chống xem đơn của người khác)
            var query = _db.Applications
                .AsNoTracking()
                .Where(a => a.UserId == userId);

            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(a => a.Status == status);
            }

            var totalCount = await query.CountAsync();
            var pageCount = Math.Max((int)Math.Ceiling(totalCount / (double)PageSize), 1);
            page = Math.Clamp(page, 1, pageCount);

            var items = await query
                .OrderByDescending(a => a.AppliedDate)
                .Skip((page - 1) * PageSize)
                .Take(PageSize)
                .Select(a => new MyApplicationItemViewModel
                {
                    ApplicationId = a.Id,
                    JobId = a.JobPostingId,
                    JobTitle = a.JobPosting!.Title,
                    CompanyName = a.JobPosting!.Company!.CompanyName,
                    Location = a.JobPosting!.Location,
                    AppliedDate = a.AppliedDate,
                    Status = a.Status
                })
                .ToListAsync();

            return new MyApplicationsViewModel
            {
                Items = items,
                Status = status,
                Page = page,
                PageCount = pageCount,
                TotalCount = totalCount
            };
        }

        public async Task<(string PhysicalPath, string FileName)?> GetCvFileAsync(string applicationId, string userId, string role)
        {
            var info = await _db.Applications
                .AsNoTracking()
                .Where(a => a.Id == applicationId)
                .Select(a => new
                {
                    a.UserId,
                    a.CvFilePath,
                    EmployerUserId = a.JobPosting!.Company!.UserId
                })
                .FirstOrDefaultAsync();

            if (info == null)
            {
                return null;
            }

            // Phân quyền xem CV: Admin xem tất cả, Candidate chỉ xem CV của mình, Employer chỉ xem CV nộp vào tin của công ty mình
            var allowed = role switch
            {
                "Admin" => true,
                "Candidate" => info.UserId == userId,
                "Employer" => info.EmployerUserId == userId,
                _ => false
            };

            if (!allowed)
            {
                return null;
            }

            var physicalPath = ResolveCvPath(info.CvFilePath);
            if (physicalPath == null || !File.Exists(physicalPath))
            {
                return null;
            }

            return (physicalPath, $"CV-{applicationId[..8]}{Path.GetExtension(physicalPath)}");
        }

        // Đổi đường dẫn lưu trong DB thành đường dẫn vật lý
        private string? ResolveCvPath(string storedPath)
        {
            var fullBase = Path.GetFullPath(Path.Combine(_env.ContentRootPath, "AppData"));
            var fullPath = Path.GetFullPath(Path.Combine(fullBase, storedPath.TrimStart('/', '\\')));

            return fullPath.StartsWith(fullBase + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase)
                ? fullPath
                : null;
        }

    }
}
