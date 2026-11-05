using BTL_LTW_DOTJOB.Services.Application;
using BTL_LTW_DOTJOB.ViewModels.Application;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BTL_LTW_DOTJOB.Controllers
{
    public class ApplicationController : Controller
    {

        private readonly IApplicationService _applicationService;

        public ApplicationController(IApplicationService applicationService)
        {
            _applicationService = applicationService;
        }
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Apply(ApplyJobViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var first = ModelState.Values.SelectMany(v => v.Errors).First().ErrorMessage;
                return BadRequest(first);
            }

            // Lấy ID của User đang đăng nhập thông qua Claims
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("Vui lòng đăng nhập lại.");
            }

            // Gọi Service xử lý logic
            var (success, message) = await _applicationService.ApplyJobAsync(model, userId);

            if (!success)
            {
                return BadRequest(message);
            }

            return Ok(new { message });
        }

    }
}
