using BTL_LTW_DOTJOB.Data;
using BTL_LTW_DOTJOB.Models;
using BTL_LTW_DOTJOB.Services.Job;
using BTL_LTW_DOTJOB.ViewModels.Job;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BTL_LTW_DOTJOB.Controllers
{
    [AllowAnonymous]
    [Route("jobs")]
    public class JobController : Controller
    {

        private readonly IJobService _jobService;
        public JobController(IJobService jobService)
        {
            _jobService = jobService;
        }

        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] SearchJobViewModel filter)
        {
            var model = await _jobService.SearchJobsAsync(filter);
            return View(model);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Detail(string id)
        {
            JobPosting? job = await _jobService.GetJobByIdAsync(id);
            if (job == null)
            {
                return NotFound();
            }
            return View(job);
        }
    }
}
