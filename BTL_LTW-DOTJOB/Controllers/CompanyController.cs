using BTL_LTW_DOTJOB.Data;
using BTL_LTW_DOTJOB.Helpers;
using BTL_LTW_DOTJOB.ViewModels.Companies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BTL_LTW_DOTJOB.Controllers
{
    [AllowAnonymous]
    [Route("companies")]
    public class CompanyController : Controller
    {
        private readonly AppDbContext _db;

        public CompanyController(AppDbContext db)
        {
            _db = db;
        }

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
                .WhereOpen()
                .OrderByDescending(j => j.CreatedAt)
                .ThenBy(j => j.Id)
                .Take(20)
                .ToListAsync();

            return View(new CompanyDetailViewModel { Company = company, Jobs = jobs });
        }
    }
}
