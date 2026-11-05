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

        [HttpPost]
        [Route("cv/save")]
        public IActionResult SaveCV([FromBody] object cvData)
        {
            return Json(new { success = true, message = "Lưu CV thành công!" });
        }
    }
}
