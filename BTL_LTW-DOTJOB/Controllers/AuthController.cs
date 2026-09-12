using Microsoft.AspNetCore.Mvc;

namespace BTL_LTW_DOTJOB.Controllers
{
    public class AuthController : Controller
    {
        public IActionResult Login()
        {
            return View();
        }

        public IActionResult Create()
        {
            return View();
        }
    }
}
