using BTL_LTW_DOTJOB.ViewModels.Admin;

namespace BTL_LTW_DOTJOB.Services.Admin
{
    public interface ICategoryService
    {
        Task<List<CategoryListItemViewModel>> GetAllAsync(string? keyword);

        Task<CategoryFormViewModel?> GetByIdAsync(string id);

        Task<(bool Success, string? Error)> CreateAsync(CategoryFormViewModel model);

        Task<(bool Success, string? Error)> UpdateAsync(CategoryFormViewModel model);

        Task<(bool Success, string? Error)> DeleteAsync(string id);
    }
}