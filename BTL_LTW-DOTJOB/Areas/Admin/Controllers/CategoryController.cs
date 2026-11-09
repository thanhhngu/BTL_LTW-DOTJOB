using BTL_LTW_DOTJOB.Services.Admin;
using BTL_LTW_DOTJOB.ViewModels.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BTL_LTW_DOTJOB.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Policy = "AdminOnly")]
    public class CategoryController : Controller
    {
        private readonly ICategoryService _categoryService;

        public CategoryController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        public async Task<IActionResult> Index(string? keyword)
        {
            ViewData["Keyword"] = keyword;
            var list = await _categoryService.GetAllAsync(keyword);
            return View(list);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View(new CategoryFormViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CategoryFormViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var (success, error) = await _categoryService.CreateAsync(model);
            if (!success)
            {
                ModelState.AddModelError(nameof(model.Name), error!);
                return View(model);
            }

            TempData["Success"] = "Đã thêm danh mục mới.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            var model = await _categoryService.GetByIdAsync(id);
            if (model == null) return NotFound();
            return View(model);
        }

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