using BTL_LTW_DOTJOB.Data;
using BTL_LTW_DOTJOB.Models;
using BTL_LTW_DOTJOB.ViewModels.Admin;
using Microsoft.EntityFrameworkCore;

namespace BTL_LTW_DOTJOB.Services.Admin
{
    public class CategoryService : ICategoryService
    {
        private readonly AppDbContext _db;

        public CategoryService(AppDbContext db)
        {
            _db = db;
        }

        public async Task<List<CategoryListItemViewModel>> GetAllAsync(string? keyword)
        {
            var query = _db.Categories.AsQueryable();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                var kw = keyword.Trim();
                query = query.Where(c => c.Name.Contains(kw));
            }

            return await query
                .OrderBy(c => c.Name)
                .Select(c => new CategoryListItemViewModel
                {
                    Id = c.Id,
                    Name = c.Name,
                    Description = c.Description,
                    JobCount = c.JobPostings.Count
                })
                .ToListAsync();
        }

        public async Task<CategoryFormViewModel?> GetByIdAsync(string id)
        {
            return await _db.Categories
                .Where(c => c.Id == id)
                .Select(c => new CategoryFormViewModel
                {
                    Id = c.Id,
                    Name = c.Name,
                    Description = c.Description
                })
                .FirstOrDefaultAsync();
        }

        public async Task<(bool Success, string? Error)> CreateAsync(CategoryFormViewModel model)
        {
            var name = model.Name.Trim();

            if (await _db.Categories.AnyAsync(c => c.Name == name))
            {
                return (false, "Tên danh mục này đã tồn tại.");
            }

            _db.Categories.Add(new Category
            {
                Name = name,
                Description = model.Description?.Trim()
            });
            await _db.SaveChangesAsync();
            return (true, null);
        }

        public async Task<(bool Success, string? Error)> UpdateAsync(CategoryFormViewModel model)
        {
            var category = await _db.Categories.FindAsync(model.Id);
            if (category == null)
            {
                return (false, "Danh mục không tồn tại.");
            }

            var name = model.Name.Trim();

            if (await _db.Categories.AnyAsync(c => c.Name == name && c.Id != model.Id))
            {
                return (false, "Tên danh mục này đã tồn tại.");
            }

            category.Name = name;
            category.Description = model.Description?.Trim();
            await _db.SaveChangesAsync();
            return (true, null);
        }

        public async Task<(bool Success, string? Error)> DeleteAsync(string id)
        {
            var category = await _db.Categories.FindAsync(id);
            if (category == null)
            {
                return (false, "Danh mục không tồn tại.");
            }

            var jobCount = await _db.JobPostings.CountAsync(j => j.CategoryId == id);
            if (jobCount > 0)
            {
                return (false, $"Không thể xóa: còn {jobCount} tin tuyển dụng thuộc danh mục này.");
            }

            _db.Categories.Remove(category);
            await _db.SaveChangesAsync();
            return (true, null);
        }
    }
}