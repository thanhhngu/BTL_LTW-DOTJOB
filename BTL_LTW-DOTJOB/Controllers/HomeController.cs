using BTL_LTW_DOTJOB.Data;
using BTL_LTW_DOTJOB.Helpers;
using BTL_LTW_DOTJOB.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

using X.PagedList;
using X.PagedList.Extensions;


namespace BTL_LTW_DOTJOB.Controllers
{
    [AllowAnonymous] // Trang chủ công khai, chưa đăng nhập vẫn xem được việc làm
    public class HomeController : Controller
    {
        private readonly AppDbContext _db;
        public HomeController(AppDbContext db)
        {
            _db = db;
        }
        public IActionResult Index(int? page)
        {
            int pageSize = 12; // 3 x 4
            int pageNumber = page ?? 1;

            var jobs = _db.JobPostings
                .AsNoTracking()
                .Include(job => job.Company)
                .WhereOpen() // chỉ tin đang bật và còn hạn nộp
                .OrderByDescending(job => job.CreatedAt);
            var pagedJobs = jobs.ToPagedList(pageNumber, pageSize);

            return View(pagedJobs);
        }

        [HttpGet]
        public IActionResult LoadFeaturedJobs(int page = 1)
        {
            int pageSize = 12;

            var jobs = _db.JobPostings
                .AsNoTracking()
                .Include(job => job.Company)
                .WhereOpen() // chỉ tin đang bật và còn hạn nộp
                .OrderByDescending(job => job.CreatedAt);
            var pagedJobs = jobs.ToPagedList(page, pageSize);

            return PartialView("_FeaturedJobs", pagedJobs);
        }

        [HttpGet]
        public IActionResult LoadCategoryMenu(int page = 1)
        {
            return ViewComponent("CategoryMenu", new { page });
        }

        public IActionResult Privacy()
        {
            return View();
        }

        // Trang báo lỗi thân thiện cho các mã 400/403/404/405... (được UseStatusCodePagesWithReExecute gọi lại).
        // Re-execute giữ nguyên phương thức gốc (có thể là POST) nên action này bỏ qua kiểm tra anti-forgery.
        // Không đặt tên là StatusCode vì trùng với phương thức StatusCode(int) có sẵn của Controller
        [IgnoreAntiforgeryToken]
        public IActionResult Status(int code)
        {
            ViewBag.Code = code;
            ViewBag.Message = code switch
            {
                400 => "Yêu cầu không hợp lệ. Có thể phiên làm việc đã hết hạn, hãy tải lại trang rồi thử lại.",
                403 => "Bạn không có quyền truy cập trang này.",
                404 => "Không tìm thấy trang bạn cần. Đường dẫn có thể sai hoặc nội dung đã bị xóa.",
                405 => "Thao tác này không được hỗ trợ khi mở trực tiếp bằng đường dẫn.",
                _ => "Có lỗi xảy ra khi xử lý yêu cầu."
            };
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

    }
}



