using BTL_LTW_DOTJOB.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;

namespace BTL_LTW_DOTJOB.ViewComponents
{
    public class CategoryMenuViewComponent : ViewComponent
    {
        private readonly AppDbContext _db;

        public CategoryMenuViewComponent(AppDbContext db)
        {
            _db = db;
        }

        public IViewComponentResult Invoke(int page = 1)
        {
            const int pageSize = 6;
            var categoriesQuery = _db.Categories
                .AsNoTracking()
                .OrderBy(category => category.Name);
            var totalCount = categoriesQuery.Count();
            var pageCount = (int)Math.Ceiling(totalCount / (double)pageSize);
            page = Math.Clamp(page, 1, Math.Max(pageCount, 1));
            var categories = categoriesQuery
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.CurrentPage = page;
            ViewBag.PageCount = pageCount;

            return View(categories);
        }
    }
}