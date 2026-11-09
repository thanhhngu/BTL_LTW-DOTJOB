using BTL_LTW_DOTJOB.ViewModels.Admin;

namespace BTL_LTW_DOTJOB.Services.Admin
{
    public interface ICategoryService
    {
        // Danh sách danh mục, có thể tìm theo tên
        Task<List<CategoryListItemViewModel>> GetAllAsync(string? keyword);

        // Lấy 1 danh mục để đổ vào form Sửa (null nếu không tồn tại)
        Task<CategoryFormViewModel?> GetByIdAsync(string id);

        // Thêm mới. Trả về (thành công?, thông báo lỗi)
        Task<(bool Success, string? Error)> CreateAsync(CategoryFormViewModel model);

        // Cập nhật
        Task<(bool Success, string? Error)> UpdateAsync(CategoryFormViewModel model);

        // Xóa. Không cho xóa nếu danh mục còn tin tuyển dụng
        Task<(bool Success, string? Error)> DeleteAsync(string id);
    }
}