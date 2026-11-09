using BTL_LTW_DOTJOB.Services.Reports;
using BTL_LTW_DOTJOB.ViewModels.Reports;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BTL_LTW_DOTJOB.Controllers
{
    [Authorize(Roles = "Candidate")] // Phía Candidate chỉ báo cáo tin tuyển dụng
    [Route("reports")]
    public class ReportController : Controller
    {
        private readonly IReportService _reportService;

        public ReportController(IReportService reportService)
        {
            _reportService = reportService;
        }

        // POST: /reports/job/{jobId} - Gửi báo cáo cho 1 tin tuyển dụng (gọi bằng fetch từ trang chi tiết)
        // Anti-forgery token gửi trong header RequestVerificationToken (xem meta csrf-token ở _Layout)
        [HttpPost("job/{jobId}")]
        public async Task<IActionResult> ReportJob(string jobId, [FromForm] CreateReportViewModel model)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("Vui lòng đăng nhập lại.");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState.Values.SelectMany(v => v.Errors).First().ErrorMessage);
            }

            var (success, message) = await _reportService.CreateJobReportAsync(userId, jobId, model.Content);

            if (!success)
            {
                return BadRequest(message);
            }

            return Ok(new { message });
        }
    }
}
