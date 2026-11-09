using System.ComponentModel.DataAnnotations;

namespace BTL_LTW_DOTJOB.ViewModels.Admin
{
    // Dữ liệu form Admin xử lý báo cáo
    public class ReportHandleViewModel
    {
        [Required]
        public string Id { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng chọn kết quả xử lý.")]
        public string Status { get; set; } = string.Empty; // Resolved hoặc Dismissed

        [StringLength(1000, ErrorMessage = "Ghi chú tối đa 1000 ký tự.")]
        public string? AdminNote { get; set; }

        // Chỉ có tác dụng với báo cáo tin tuyển dụng đã xử lý: ẩn tin khỏi trang công khai
        public bool HideJob { get; set; }
    }
}