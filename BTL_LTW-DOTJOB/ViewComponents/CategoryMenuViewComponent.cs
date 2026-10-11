using BTL_LTW_DOTJOB.Models.Mock;
using Microsoft.AspNetCore.Mvc;

namespace BTL_LTW_DOTJOB.ViewComponents
{
    public class CategoryMenuViewComponent : ViewComponent
    {
        CateMockData CateMockData = new CateMockData();
        public IViewComponentResult Invoke(int page = 1)
        {
            const int pageSize = 6;
            var categories = CateMockData.GetMockCategories();
            var pageCount = (int)Math.Ceiling(categories.Count / (double)pageSize);
            page = Math.Clamp(page, 1, Math.Max(pageCount, 1));

            ViewBag.CurrentPage = page;
            ViewBag.PageCount = pageCount;

            return View(categories.Skip((page - 1) * pageSize).Take(pageSize));
        }
    }
}