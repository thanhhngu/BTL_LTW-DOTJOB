using BTL_LTW_DOTJOB.Models;

namespace BTL_LTW_DOTJOB.ViewModels.Companies
{
    // Dữ liệu cho trang công ty công khai: thông tin công ty và các tin đang tuyển
    public class CompanyDetailViewModel
    {
        public Models.Company Company { get; set; } = new();
        public List<JobPosting> Jobs { get; set; } = new();
    }
}
