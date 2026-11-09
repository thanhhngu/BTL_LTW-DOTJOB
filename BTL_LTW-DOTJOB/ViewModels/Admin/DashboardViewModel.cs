namespace BTL_LTW_DOTJOB.ViewModels.Admin
{
    // Dữ liệu thống kê hiển thị ở trang Tổng quan của Admin
    public class DashboardViewModel
    {
        public int TotalUsers { get; set; }       // Tổng số tài khoản
        public int TotalCandidates { get; set; }  // Số tài khoản Ứng viên
        public int TotalEmployers { get; set; }   // Số tài khoản Nhà tuyển dụng
        public int TotalJobs { get; set; }        // Tổng số tin tuyển dụng
        public int TotalApplications { get; set; } // Tổng số đơn ứng tuyển
        public int TotalCategories { get; set; }  // Số danh mục ngành nghề
        public int PendingReports { get; set; }   // Báo cáo đang chờ xử lý
        public int HiddenJobs { get; set; }       // Tin tuyển dụng đang bị ẩn
    }
}
