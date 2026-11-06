using BTL_LTW_DOTJOB.Services.Application;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using System.Security.Claims;

namespace BTL_LTW_DOTJOB.Controllers
{
    // Tải/xem CV đã nộp. CV nằm ngoài wwwroot nên chỉ truy cập được qua action này (có kiểm tra quyền)
    [Authorize]
    public class CvFileController : Controller
    {
        private static readonly FileExtensionContentTypeProvider ContentTypes = new();

        private readonly IApplicationService _applicationService;

        public CvFileController(IApplicationService applicationService)
        {
            _applicationService = applicationService;
        }

        // GET: /cv-files/{applicationId}
        [HttpGet("cv-files/{applicationId}")]
        public async Task<IActionResult> Download(string applicationId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var role = User.FindFirstValue(ClaimTypes.Role);
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(role))
            {
                return Challenge();
            }

            var cv = await _applicationService.GetCvFileAsync(applicationId, userId, role);

            // Không có quyền cũng trả 404 để không lộ việc đơn đó có tồn tại hay không
            if (cv == null)
            {
                return NotFound();
            }

            var (physicalPath, fileName) = cv.Value;

            if (!ContentTypes.TryGetContentType(fileName, out var contentType))
            {
                contentType = "application/octet-stream";
            }

            // PDF mở xem trực tiếp trên trình duyệt, DOC/DOCX thì tải về
            if (Path.GetExtension(fileName).Equals(".pdf", StringComparison.OrdinalIgnoreCase))
            {
                return PhysicalFile(physicalPath, contentType);
            }

            return PhysicalFile(physicalPath, contentType, fileName);
        }
    }
}
