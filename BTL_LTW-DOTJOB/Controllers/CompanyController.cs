using BTL_LTW_DOTJOB.Data;
using BTL_LTW_DOTJOB.Helpers;
using BTL_LTW_DOTJOB.ViewModels.Companies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BTL_LTW_DOTJOB.Controllers
{
    // Trang giới thiệu công ty cho mọi người xem (kể cả chưa đăng nhập), dữ liệu do HR nhập ở trang Hồ sơ
    [AllowAnonymous]
    [Route("companies")]
    public class CompanyController : Controller
    {
        private readonly AppDbContext _db;

        public CompanyController(AppDbContext db)
        {
            _db = db;
        }

        // GET: /companies/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> Detail(string id)
        {
            var company = await _db.Companies
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == id);

            if (company == null)
            {
                return NotFound();
            }

            var jobs = await _db.JobPostings
                .AsNoTracking()
                .Include(j => j.Company)
                .Where(j => j.CompanyId == id)
                .WhereOpen() // chỉ tin đang bật và còn hạn nộp, dùng chung với trang chủ và tìm kiếm
                .OrderByDescending(j => j.CreatedAt)
                .ThenBy(j => j.Id)
                .Take(20)
                .ToListAsync();

            return View(new CompanyDetailViewModel { Company = company, Jobs = jobs });
        }
    }
}
