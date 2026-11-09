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

        [HttpPut("{id}/status")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(string id, [FromBody] UpdateStatusRequest request)
        {
            if (!ApplicationStatus.All.Contains(request.Status))
            {
                return BadRequest(new { message = "Trạng thái không hợp lệ." });
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

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

    public class UpdateStatusRequest
    {
        [Required(ErrorMessage = "Vui lòng cung cấp trạng thái.")]
        public string Status { get; set; } = string.Empty;
    }
}
