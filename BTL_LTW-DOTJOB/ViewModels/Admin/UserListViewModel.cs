namespace BTL_LTW_DOTJOB.ViewModels.Admin
{
    // Dữ liệu cho cả trang danh sách: các dòng + bộ lọc đang chọn + phân trang
    public class UserListViewModel
    {
        public List<UserListItemViewModel> Users { get; set; } = new();

        // Bộ lọc (giữ lại để hiển thị lại trong form tìm kiếm)
        public string? Keyword { get; set; }
        public string? Role { get; set; }    // "", Admin, Employer, Candidate
        public string? Status { get; set; }  // "", active, locked

        // Phân trang
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalItems { get; set; }
        public int TotalPages => (int)Math.Ceiling(TotalItems / (double)PageSize);
    }
}