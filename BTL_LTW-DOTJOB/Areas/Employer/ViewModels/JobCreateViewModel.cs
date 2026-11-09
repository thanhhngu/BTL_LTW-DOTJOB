using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Text.RegularExpressions;

namespace BTL_LTW_DOTJOB.Areas.Employer.ViewModels
{
    public class JobCreateViewModel : IValidatableObject
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

        [ValidateNever]
        public IEnumerable<SelectListItem> Categories { get; set; } = Enumerable.Empty<SelectListItem>();

        [ValidateNever]
        public IEnumerable<SelectListItem> JobTypes { get; set; } = Enumerable.Empty<SelectListItem>();

        [ValidateNever]
        public IEnumerable<SelectListItem> Locations { get; set; } = Enumerable.Empty<SelectListItem>();

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var title = Title ?? string.Empty;
            if (title.Length >= 10 && title.Trim().Length < 10)
            {
                yield return new ValidationResult(
                    "Tiêu đề phải từ 10 đến 255 ký tự (không tính khoảng trắng ở hai đầu).", new[] { nameof(Title) });
            }

            if (SalaryMin.HasValue != SalaryMax.HasValue)
            {
                var thieu = SalaryMin.HasValue ? nameof(SalaryMax) : nameof(SalaryMin);
                yield return new ValidationResult(
                    "Vui lòng nhập cả lương tối thiểu và tối đa, hoặc để trống cả hai nếu thỏa thuận.", new[] { thieu });
            }

            if (SalaryMin.HasValue && SalaryMax.HasValue && SalaryMax < SalaryMin)
            {
                yield return new ValidationResult(
                    "Lương tối đa phải lớn hơn hoặc bằng lương tối thiểu.", new[] { nameof(SalaryMax) });
            }

            if (SalaryMax.HasValue && SalaryMax <= 0)
            {
                yield return new ValidationResult(
                    "Lương tối đa phải lớn hơn 0, hoặc để trống cả hai ô nếu thỏa thuận.", new[] { nameof(SalaryMax) });
            }

            if (Deadline.HasValue && Deadline.Value.Date < DateTime.Today)
            {
                yield return new ValidationResult(
                    "Hạn nộp hồ sơ không được ở quá khứ.", new[] { nameof(Deadline) });
            }

            if (!string.IsNullOrEmpty(JobType) && !JobOptions.JobTypes.Contains(JobType))
            {
                yield return new ValidationResult("Hình thức làm việc không hợp lệ.", new[] { nameof(JobType) });
            }

            if (!string.IsNullOrEmpty(Location) && !JobOptions.Locations.Contains(Location))
            {
                yield return new ValidationResult("Địa điểm làm việc không hợp lệ.", new[] { nameof(Location) });
            }

            if (IsHtmlBlank(Description))
            {
                yield return new ValidationResult("Vui lòng nhập mô tả công việc.", new[] { nameof(Description) });
            }

            if (IsHtmlBlank(Requirements))
            {
                yield return new ValidationResult("Vui lòng nhập yêu cầu ứng viên.", new[] { nameof(Requirements) });
            }
        }

        private static bool IsHtmlBlank(string? html)
        {
            if (string.IsNullOrWhiteSpace(html)) return true;

            var text = WebUtility.HtmlDecode(Regex.Replace(html, "<[^>]*>", string.Empty));
            return string.IsNullOrWhiteSpace(text.Replace('\u00A0', ' '));
        }
    }
}
