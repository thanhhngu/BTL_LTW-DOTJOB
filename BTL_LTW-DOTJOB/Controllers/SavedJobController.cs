using BTL_LTW_DOTJOB.Services.SavedJobs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BTL_LTW_DOTJOB.Controllers
{
    [Authorize(Roles = "Candidate")] // Chỉ Ứng viên mới lưu tin
    public class SavedJobController : Controller
    {
        private readonly ISavedJobService _savedJobService;

        public SavedJobController(ISavedJobService savedJobService)
        {
            _savedJobService = savedJobService;
        }

        // GET: /saved-jobs - Việc làm đã lưu của tôi
        [HttpGet("saved-jobs")]
        public async Task<IActionResult> Index(int page = 1)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Challenge();
            }

            var model = await _savedJobService.GetMySavedJobsAsync(userId, page);
            return View(model);
        }

        // POST: /saved-jobs/{jobId}/toggle - Lưu / bỏ lưu (gọi bằng fetch từ nút trái tim)
        [HttpPost("saved-jobs/{jobId}/toggle")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Toggle(string jobId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("Vui lòng đăng nhập lại.");
            }

            var (success, saved, count, message) = await _savedJobService.ToggleAsync(jobId, userId);

            if (!success)
            {
                return BadRequest(message);
            }

            return Ok(new { saved, count, message });
        }
    }
}
