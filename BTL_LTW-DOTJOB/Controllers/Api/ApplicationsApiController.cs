using BTL_LTW_DOTJOB.Data;
using BTL_LTW_DOTJOB.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace BTL_LTW_DOTJOB.Controllers.Api
{
    [ApiController]
    [Route("api/applications")]
    [Authorize(Roles = "Employer")]
    public class ApplicationsApiController : ControllerBase
    {
        private readonly AppDbContext _db;

        public ApplicationsApiController(AppDbContext db)
        {
            _db = db;
        }

        // PUT: /api/applications/{id}/status
        // Body JSON: { "status": "Accepted" | "Rejected" | "Pending" }
        // Cookie đăng nhập tự gửi kèm nên phải có anti-forgery token (header RequestVerificationToken) để chống CSRF
        [HttpPut("{id}/status")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(string id, [FromBody] UpdateStatusRequest request)
        {
            // Thiếu body hoặc thiếu status: [ApiController] tự trả 400.
            // Status phải là một trong các hằng số đã chốt, không nhận giá trị tự chế
            if (!ApplicationStatus.All.Contains(request.Status))
            {
                return BadRequest(new { message = "Trạng thái không hợp lệ." });
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Chỉ tìm đơn nộp vào tin của công ty thuộc HR đang đăng nhập.
            // Đơn của HR khác trả 404 như không tồn tại để không lộ việc đơn đó có thật
            var application = await _db.Applications
                .FirstOrDefaultAsync(a => a.Id == id && a.JobPosting!.Company!.UserId == userId);

            if (application == null)
            {
                return NotFound(new { message = "Không tìm thấy đơn ứng tuyển." });
            }

            application.Status = request.Status;
            await _db.SaveChangesAsync();

            return Ok(new
            {
                id = application.Id,
                status = application.Status,
                label = ApplicationStatus.Label(application.Status)
            });
        }
    }

    // Dữ liệu client gửi lên khi đổi trạng thái đơn
    public class UpdateStatusRequest
    {
        [Required(ErrorMessage = "Vui lòng cung cấp trạng thái.")]
        public string Status { get; set; } = string.Empty;
    }
}
