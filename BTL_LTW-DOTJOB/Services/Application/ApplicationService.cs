using BTL_LTW_DOTJOB.Data;
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

            var job = await _db.JobPostings.FindAsync(model.Id);
            if (job == null || !job.IsActive)
                return (false, "Tin tuyển dụng không tồn tại hoặc đã bị ẩn.");

            if (job.Deadline < DateTime.UtcNow)
                return (false, "Tin tuyển dụng đã hết hạn ứng tuyển.");

            // 2. Kiểm tra xem user đã ứng tuyển job này trước đó chưa
            var existingApp = await _db.Applications
                .FirstOrDefaultAsync(a => a.JobPostingId == model.Id && a.UserId == userId);

            if (existingApp != null)
                return (false, "Bạn đã nộp hồ sơ cho vị trí này rồi.");

            // 3. Lưu file CV vào thư mục wwwroot/uploads/cvs
            var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", "cvs");
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
            var application = new BTL_LTW_DOTJOB.Models.Application
            {
                JobPostingId = model.Id,
                UserId = userId,
                CvFilePath = $"/uploads/cvs/{uniqueFileName}",
                CoverLetter = model.CoverLetter ?? "...",
                AppliedDate = DateTime.UtcNow,
                Status = "Pending"
            };

            _db.Applications.Add(application);
            await _db.SaveChangesAsync();

            return (true, "Nộp hồ sơ thành công!");
        }

    }
}
