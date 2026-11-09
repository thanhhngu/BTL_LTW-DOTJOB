using System.ComponentModel.DataAnnotations;

namespace BTL_LTW_DOTJOB.ViewModels.Admin
{
    public class ReportHandleViewModel
    {
        [Required]
        public string Id { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng chọn kết quả xử lý.")]
        public string Status { get; set; } = string.Empty;

        [StringLength(1000, ErrorMessage = "Ghi chú tối đa 1000 ký tự.")]
        public string? AdminNote { get; set; }

        public bool HideJob { get; set; }

        public bool BanUser { get; set; }
    }
}