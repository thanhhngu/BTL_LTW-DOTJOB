namespace BTL_LTW_DOTJOB.ViewModels.Application
{
    // Một dòng trong trang "Việc làm đã ứng tuyển"
    public class MyApplicationItemViewModel
    {
        public string ApplicationId { get; set; } = string.Empty;
        public string JobId { get; set; } = string.Empty;
        public string JobTitle { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;
        public string? Location { get; set; }
        public DateTime AppliedDate { get; set; }
        public string Status { get; set; } = "Pending";
    }

    // Dữ liệu cả trang (kèm lọc theo trạng thái và phân trang)
    public class MyApplicationsViewModel
    {
        public List<MyApplicationItemViewModel> Items { get; set; } = new();
        public string? Status { get; set; }
        public int Page { get; set; } = 1;
        public int PageCount { get; set; } = 1;
        public int TotalCount { get; set; }
    }
}
