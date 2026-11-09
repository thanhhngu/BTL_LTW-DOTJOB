using BTL_LTW_DOTJOB.Data;
using BTL_LTW_DOTJOB.Models;
using BTL_LTW_DOTJOB.ViewModels.SavedJobs;
using Microsoft.EntityFrameworkCore;

namespace BTL_LTW_DOTJOB.Services.SavedJobs
{
    public class SavedJobService : ISavedJobService
    {
        private const int PageSize = 12;

        private readonly AppDbContext _db;

        // Service đăng ký Scoped nên biến này chỉ sống trong 1 request:
        // nhiều thẻ việc làm trên cùng 1 trang chỉ tốn 1 truy vấn
        private HashSet<string>? _savedIdsCache;

        public SavedJobService(AppDbContext db)
        {
            _db = db;
        }

        public async Task<(bool Success, bool Saved, int Count, string Message)> ToggleAsync(string jobId, string userId)
        {
            var existing = await _db.SavedJobs
                .FirstOrDefaultAsync(s => s.JobPostingId == jobId && s.UserId == userId);

            bool saved;
            if (existing != null)
            {
                // Đã lưu rồi => bỏ lưu
                _db.SavedJobs.Remove(existing);
                await _db.SaveChangesAsync();
                saved = false;
            }
            else
            {
                var jobExists = await _db.JobPostings.AnyAsync(j => j.Id == jobId && j.IsActive);
                if (!jobExists)
                    return (false, false, 0, "Tin tuyển dụng không tồn tại hoặc đã bị ẩn.");

                try
                {
                    _db.SavedJobs.Add(new SavedJob { UserId = userId, JobPostingId = jobId });
                    await _db.SaveChangesAsync();
                }
                catch (DbUpdateException)
                {
                    // Bấm 2 lần cùng lúc bị trùng (chặn bởi unique index) => coi như đã lưu
                }
                saved = true;
            }

            _savedIdsCache = null;
            var count = await CountAsync(userId);
            return (true, saved, count, saved ? "Đã lưu tin." : "Đã bỏ lưu tin.");
        }

        public async Task<MySavedJobsViewModel> GetMySavedJobsAsync(string userId, int page)
        {
            // Luôn lọc theo userId của người đăng nhập, chỉ hiện tin còn hiển thị
            var query = _db.SavedJobs
                .AsNoTracking()
                .Where(s => s.UserId == userId && s.JobPosting!.IsActive);

            var totalCount = await query.CountAsync();
            var pageCount = Math.Max((int)Math.Ceiling(totalCount / (double)PageSize), 1);
            page = Math.Clamp(page, 1, pageCount);

            var saved = await query
                .Include(s => s.JobPosting)
                    .ThenInclude(job => job!.Company)
                .OrderByDescending(s => s.SavedAt)
                .Skip((page - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();

            var items = saved.Select(s => s.JobPosting!).ToList();

            return new MySavedJobsViewModel
            {
                Items = items,
                Page = page,
                PageCount = pageCount,
                TotalCount = totalCount
            };
        }

        public async Task<HashSet<string>> GetSavedJobIdsAsync(string? userId)
        {
            if (string.IsNullOrEmpty(userId))
                return new HashSet<string>();

            if (_savedIdsCache == null)
            {
                var ids = await _db.SavedJobs
                    .AsNoTracking()
                    .Where(s => s.UserId == userId && s.JobPosting!.IsActive)
                    .Select(s => s.JobPostingId)
                    .ToListAsync();
                _savedIdsCache = new HashSet<string>(ids);
            }

            return _savedIdsCache;
        }

        private Task<int> CountAsync(string userId) =>
            _db.SavedJobs.CountAsync(s => s.UserId == userId && s.JobPosting!.IsActive);
    }
}
