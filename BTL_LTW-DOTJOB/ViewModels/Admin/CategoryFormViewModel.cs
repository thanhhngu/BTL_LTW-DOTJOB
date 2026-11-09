using System.ComponentModel.DataAnnotations;

namespace BTL_LTW_DOTJOB.ViewModels.Admin
{
    // Dữ liệu của form Thêm / Sửa danh mục
    public class CategoryFormViewModel
    {
        public string? Id { get; set; } // null khi thêm mới, có giá trị khi sửa

        [Required(ErrorMessage = "Vui lòng nhập tên danh mục.")]
        [StringLength(100, ErrorMessage = "Tên danh mục tối đa 100 ký tự.")]
        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }
    }
}