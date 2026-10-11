using BTL_LTW_DOTJOB.Data;
using BTL_LTW_DOTJOB.Models;
using BTL_LTW_DOTJOB.Services.Job;
using Microsoft.AspNetCore.Mvc;

namespace BTL_LTW_DOTJOB.Controllers
{
    [Route("jobs")]
    public class JobController : Controller
    {

        IJobService _jobService;
        public JobController(IJobService jobService)
        {
            _jobService = jobService;
        }

        [Route("{id}")]
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
