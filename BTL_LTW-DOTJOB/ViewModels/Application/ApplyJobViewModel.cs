using System.ComponentModel.DataAnnotations;

namespace BTL_LTW_DOTJOB.ViewModels.Application
{
    public class ApplyJobViewModel
    {
        public string Id { get; set; }
        public string? JobTitle { get; set; }   // chỉ để hiển thị trên popup

        [Required(ErrorMessage = "Vui lòng nhập họ tên")]
        [StringLength(100)]
        public string FullName { get; set; } = "";

        [Required(ErrorMessage = "Vui lòng nhập email")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string Email { get; set; } = "";

        [Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        public string Phone { get; set; } = "";

        //[Required(ErrorMessage = "Vui lòng chọn địa điểm")]
        //public string DesiredLocation { get; set; } = "";

        [StringLength(2000)]
        public string? CoverLetter { get; set; }

        public IFormFile? CvFile { get; set; }

        //public bool AllowAiAnalysis { get; set; }

        [Range(typeof(bool), "true", "true", ErrorMessage = "Bạn cần đồng ý thỏa thuận sử dụng dữ liệu")]
        public bool AgreeTerms { get; set; }
    }
}
