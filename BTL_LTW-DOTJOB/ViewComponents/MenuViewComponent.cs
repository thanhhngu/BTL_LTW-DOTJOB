using Microsoft.AspNetCore.Mvc;

namespace BTL_LTW_DOTJOB.ViewComponents
{
    public class MenuViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke()
        {
            return View();
        }
    }
}
