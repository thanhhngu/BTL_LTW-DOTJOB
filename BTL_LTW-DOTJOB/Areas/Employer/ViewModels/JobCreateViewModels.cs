using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace BTL_LTW_DOTJOB.Areas.Employer.ViewModels
{
    public class JobCreateViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập tiêu đề tin tuyển dụng.")]
        [StringLength(255, MinimumLength = 10, ErrorMessage = "Tiêu đề phải từ {2} đến {1} ký tự.")]
        [Display(Name = "Tiêu đề")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng chọn ngành nghề.")]
        [Display(Name = "Ngành nghề")]
        public string CategoryId { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng chọn địa điểm làm việc.")]
        [Display(Name = "Địa điểm")]
        public string? Location { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn hình thức làm việc.")]
        [Display(Name = "Hình thức")]
        public string? JobType { get; set; }

        [Range(typeof(decimal), "0", "1000000000", ErrorMessage = "Lương tối thiểu phải từ 0 đến 1.000.000.000 đồng.")]
        [Display(Name = "Lương tối thiểu (VNĐ)")]
        public decimal? SalaryMin { get; set; }

        [Range(typeof(decimal), "0", "1000000000", ErrorMessage = "Lương tối đa phải từ 0 đến 1.000.000.000 đồng.")]
        [Display(Name = "Lương tối đa (VNĐ)")]
        public decimal? SalaryMax { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn hạn nộp hồ sơ.")]
        [DataType(DataType.Date)]
        [Display(Name = "Hạn nộp hồ sơ")]
        public DateTime? Deadline { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập mô tả công việc.")]
        [Display(Name = "Mô tả công việc")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập yêu cầu ứng viên.")]
        [Display(Name = "Yêu cầu ứng viên")]
        public string Requirements { get; set; } = string.Empty;

        // Dữ liệu cho các ô chọn, không đến từ form. Phải có [ValidateNever] vì
        // kiểu không-null được MVC mặc định coi là bắt buộc
        [ValidateNever]
        public IEnumerable<SelectListItem> Categories { get; set; } = Enumerable.Empty<SelectListItem>();

        [ValidateNever]
        public IEnumerable<SelectListItem> JobTypes { get; set; } = Enumerable.Empty<SelectListItem>();

        [ValidateNever]
        public IEnumerable<SelectListItem> Locations { get; set; } = Enumerable.Empty<SelectListItem>();
    }
}
