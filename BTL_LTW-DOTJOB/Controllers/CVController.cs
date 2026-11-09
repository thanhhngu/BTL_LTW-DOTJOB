using Microsoft.AspNetCore.Mvc;

namespace BTL_LTW_DOTJOB.Controllers
{
    public class CVController : Controller
    {
        [HttpGet]
        [Route("cv/create")]
        public IActionResult Index()
        {
            return View();
        }

    }
}
