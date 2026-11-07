using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BTL_LTW_DOTJOB.Areas.Employer.Controllers
{
    [Area("Employer")]
    [Authorize(Roles = "Employer")]
    public class DashboardController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
