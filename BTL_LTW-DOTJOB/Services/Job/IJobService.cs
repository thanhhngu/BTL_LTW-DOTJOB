using BTL_LTW_DOTJOB.Models;
using BTL_LTW_DOTJOB.ViewModels.Job;

namespace BTL_LTW_DOTJOB.Services.Job
{
    public interface IJobService
    {
        Task<JobPosting?> GetJobByIdAsync(string id);

        // Tìm kiếm và lọc việc làm (từ khóa, ngành, địa điểm, mức lương, hình thức) có phân trang
        Task<SearchJobViewModel> SearchJobsAsync(SearchJobViewModel filter);

        // Dữ liệu cho các ô chọn Danh mục, Địa điểm, Hình thức làm việc
        Task FillFilterOptionsAsync(SearchJobViewModel model);
    }
}
