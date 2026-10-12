using BTL_LTW_DOTJOB.Data;
using BTL_LTW_DOTJOB.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

using X.PagedList;
using X.PagedList.Extensions;


namespace BTL_LTW_DOTJOB.Controllers
{
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
                .Where(job => job.IsActive)
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
                .Where(job => job.IsActive)
                .OrderByDescending(job => job.CreatedAt);
            var pagedJobs = jobs.ToPagedList(page, pageSize);

            return PartialView("_FeaturedJobs", pagedJobs);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

    }
}



