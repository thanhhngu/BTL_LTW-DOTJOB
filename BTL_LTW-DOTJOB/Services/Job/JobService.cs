using BTL_LTW_DOTJOB.Data;
using BTL_LTW_DOTJOB.Models;
using BTL_LTW_DOTJOB.ViewModels.Job;
using Microsoft.EntityFrameworkCore;

namespace BTL_LTW_DOTJOB.Services.Job
{
    public class JobService : IJobService
    {
        private const int PageSize = 12;

        private readonly AppDbContext _db;

        public JobService(AppDbContext db)
        {
            _db = db;
        }

        public async Task<JobPosting?> GetJobByIdAsync(string id)
        {
            // FindAsync không nạp navigation => phải Include Company, Category để trang chi tiết hiển thị đúng
            // Chỉ trả về tin đang hiển thị (IsActive)
            return await _db.JobPostings
                .AsNoTracking()
                .Include(job => job.Company)
                .Include(job => job.Category)
                .FirstOrDefaultAsync(job => job.Id == id && job.IsActive);
        }

        public async Task<SearchJobViewModel> SearchJobsAsync(SearchJobViewModel filter)
        {
            var query = _db.JobPostings
                .AsNoTracking()
                .Include(job => job.Company)
                .Where(job => job.IsActive);

            // 1. Từ khóa: tìm trong tên vị trí hoặc tên công ty, không phân biệt hoa thường và dấu
            var keyword = filter.Keyword?.Trim();
            if (!string.IsNullOrEmpty(keyword))
            {
                query = query.Where(job =>
                    EF.Functions.Collate(job.Title, "Vietnamese_CI_AI").Contains(keyword)
                    || EF.Functions.Collate(job.Company!.CompanyName, "Vietnamese_CI_AI").Contains(keyword));
            }

            // 2. Ngành nghề
            var categoryId = filter.CategoryId;
            if (!string.IsNullOrEmpty(categoryId))
            {
                query = query.Where(job => job.CategoryId == categoryId);
            }

            // 3. Địa điểm
            var location = filter.Location;
            if (!string.IsNullOrEmpty(location))
            {
                query = query.Where(job => job.Location == location);
            }

            // 4. Hình thức làm việc
            var jobType = filter.JobType;
            if (!string.IsNullOrEmpty(jobType))
            {
                query = query.Where(job => job.JobType == jobType);
            }

            // 5. Mức lương: lấy tin có khoảng lương giao với khoảng đang chọn (tin "Thỏa thuận" không có lương thì bị loại)
            var (salaryFrom, salaryTo) = GetSalaryRange(filter.SalaryRange);
            if (salaryFrom.HasValue)
            {
                var from = salaryFrom.Value;
                query = query.Where(job => job.SalaryMin != null && (job.SalaryMax ?? job.SalaryMin) >= from);

                if (salaryTo.HasValue)
                {
                    var to = salaryTo.Value;
                    query = query.Where(job => job.SalaryMin <= to);
                }
            }

            // 6. Phân trang (luôn nằm trong khoảng 1..tổng số trang)
            var totalCount = await query.CountAsync();
            var pageCount = Math.Max((int)Math.Ceiling(totalCount / (double)PageSize), 1);
            var page = Math.Clamp(filter.Page, 1, pageCount);

            var items = await query
                .OrderByDescending(job => job.CreatedAt)
                .ThenBy(job => job.Id)
                .Skip((page - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();

            filter.Keyword = keyword;
            filter.Items = items;
            filter.Page = page;
            filter.PageCount = pageCount;
            filter.TotalCount = totalCount;

            await FillFilterOptionsAsync(filter);

            return filter;
        }

        public async Task FillFilterOptionsAsync(SearchJobViewModel model)
        {
            model.Categories = await _db.Categories
                .AsNoTracking()
                .OrderBy(category => category.Name)
                .ToListAsync();

            // Địa điểm và hình thức lấy từ chính các tin đang hiển thị
            model.Locations = await _db.JobPostings
                .AsNoTracking()
                .Where(job => job.IsActive && job.Location != null)
                .Select(job => job.Location!)
                .Distinct()
                .OrderBy(location => location)
                .ToListAsync();

            model.JobTypes = await _db.JobPostings
                .AsNoTracking()
                .Where(job => job.IsActive && job.JobType != null)
                .Select(job => job.JobType!)
                .Distinct()
                .OrderBy(type => type)
                .ToListAsync();
        }

        // Đổi mã khoảng lương thành (từ, đến), đơn vị đồng. Giá trị null = không giới hạn
        private static (decimal? From, decimal? To) GetSalaryRange(string? salaryRange)
        {
            return salaryRange switch
            {
                "lt10" => (0m, 10_000_000m),
                "10-20" => (10_000_000m, 20_000_000m),
                "20-30" => (20_000_000m, 30_000_000m),
                "gt30" => (30_000_000m, null),
                _ => (null, null)
            };
        }
    }
}
