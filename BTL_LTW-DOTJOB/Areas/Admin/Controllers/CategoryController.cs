using BTL_LTW_DOTJOB.Services.Admin;
using BTL_LTW_DOTJOB.ViewModels.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BTL_LTW_DOTJOB.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Policy = "AdminOnly")] // Chỉ Admin được quản lý danh mục
    public class CategoryController : Controller
    {
        private readonly ICategoryService _categoryService;

        public CategoryController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        // GET: /Admin/Category?keyword=...  (danh sách + tìm kiếm)
        public async Task<IActionResult> Index(string? keyword)
        {
            ViewData["Keyword"] = keyword; // Giữ lại từ khóa trong ô tìm kiếm
            var list = await _categoryService.GetAllAsync(keyword);
            return View(list);
        }

        // GET: /Admin/Category/Create
        [HttpGet]
        public IActionResult Create()
        {
            return View(new CategoryFormViewModel());
        }

        // POST: /Admin/Category/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CategoryFormViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var (success, error) = await _categoryService.CreateAsync(model);
            if (!success)
            {
                // Lỗi nghiệp vụ (vd: trùng tên) gắn vào ô Name để hiện ngay dưới ô nhập
                ModelState.AddModelError(nameof(model.Name), error!);
                return View(model);
            }

            TempData["Success"] = "Đã thêm danh mục mới."; // Hiện 1 lần ở trang danh sách
            return RedirectToAction(nameof(Index));
        }

        // GET: /Admin/Category/Edit/{id}
        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            var model = await _categoryService.GetByIdAsync(id);
            if (model == null) return NotFound();
            return View(model);
        }

        // POST: /Admin/Category/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(CategoryFormViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var (success, error) = await _categoryService.UpdateAsync(model);
            if (!success)
            {
                ModelState.AddModelError(nameof(model.Name), error!);
                return View(model);
            }

            TempData["Success"] = "Đã cập nhật danh mục.";
            return RedirectToAction(nameof(Index));
        }

        // POST: /Admin/Category/Delete  (xóa bằng POST, không dùng GET để tránh bị xóa nhầm qua link)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id)
        {
            var (success, error) = await _categoryService.DeleteAsync(id);
            if (success) TempData["Success"] = "Đã xóa danh mục.";
            else TempData["Error"] = error;

            return RedirectToAction(nameof(Index));
        }
    }
}