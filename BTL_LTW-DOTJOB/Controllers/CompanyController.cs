using BTL_LTW_DOTJOB.Data;
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

            var today = DateTime.Today; // cùng cách tính "đang hiển thị" với trang HR

            var jobs = await _db.JobPostings
                .AsNoTracking()
                .Include(j => j.Company)
                .Where(j => j.CompanyId == id && j.IsActive && (j.Deadline == null || j.Deadline >= today))
                .OrderByDescending(j => j.CreatedAt)
                .ThenBy(j => j.Id)
                .Take(20)
                .ToListAsync();

            return View(new CompanyDetailViewModel { Company = company, Jobs = jobs });
        }
    }
}
