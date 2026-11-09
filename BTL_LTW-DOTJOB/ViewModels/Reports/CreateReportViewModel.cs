using System.ComponentModel.DataAnnotations;

namespace BTL_LTW_DOTJOB.ViewModels.Reports
{
    public class CreateReportViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập nội dung báo cáo.")]
        [StringLength(1000, MinimumLength = 10, ErrorMessage = "Nội dung báo cáo từ 10 đến 1000 ký tự.")]
        public string Content { get; set; } = string.Empty;
    }
}
