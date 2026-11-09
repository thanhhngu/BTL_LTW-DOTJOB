using BTL_LTW_DOTJOB.Models;

namespace BTL_LTW_DOTJOB.ViewModels.Companies
{
    public class CompanyDetailViewModel
    {
        public Models.Company Company { get; set; } = new();
        public List<JobPosting> Jobs { get; set; } = new();
    }
}
