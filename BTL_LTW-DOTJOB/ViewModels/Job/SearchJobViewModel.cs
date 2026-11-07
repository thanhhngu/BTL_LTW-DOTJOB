using BTL_LTW_DOTJOB.Models;

namespace BTL_LTW_DOTJOB.ViewModels.Job
{
    // Điều kiện lọc (nhận từ query string) + kết quả tìm kiếm + dữ liệu cho các ô chọn
    public class SearchJobViewModel
    {
        // 1. Điều kiện lọc
        public string? Keyword { get; set; }
        public string? CategoryId { get; set; }
        public string? Location { get; set; }
        public string? SalaryRange { get; set; } // lt10 | 10-20 | 20-30 | gt30
        public string? JobType { get; set; }
        public int Page { get; set; } = 1;

        // 2. Kết quả (phân trang)
        public List<JobPosting> Items { get; set; } = new();
        public int PageCount { get; set; } = 1;
        public int TotalCount { get; set; }

        // 3. Dữ liệu đổ vào các ô chọn (lấy từ DB)
        public List<Category> Categories { get; set; } = new();
        public List<string> Locations { get; set; } = new();
        public List<string> JobTypes { get; set; } = new();

        // Có đang lọc theo điều kiện nào không (để hiện nút "Xóa bộ lọc")
        public bool HasFilter =>
            !string.IsNullOrWhiteSpace(Keyword)
            || !string.IsNullOrEmpty(CategoryId)
            || !string.IsNullOrEmpty(Location)
            || !string.IsNullOrEmpty(SalaryRange)
            || !string.IsNullOrEmpty(JobType);
    }
}
