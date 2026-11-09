namespace BTL_LTW_DOTJOB.ViewModels.Admin
{
    // Dữ liệu cho cả trang danh sách: các dòng + bộ lọc + phân trang
    public class ReportListViewModel
    {
        public List<ReportListItemViewModel> Reports { get; set; } = new();

        // Bộ lọc đang chọn
        public string? Status { get; set; }      // "", Pending, Resolved, Dismissed
        public string? TargetType { get; set; }  // "", JobPosting, User

        public int PendingCount { get; set; }    // Số báo cáo chờ xử lý (hiện ở đầu trang)

        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalItems { get; set; }
        public int TotalPages => (int)Math.Ceiling(TotalItems / (double)PageSize);
    }
}