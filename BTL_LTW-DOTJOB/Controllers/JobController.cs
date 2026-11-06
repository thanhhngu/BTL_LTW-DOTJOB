using BTL_LTW_DOTJOB.Data;
using BTL_LTW_DOTJOB.Models;
using BTL_LTW_DOTJOB.Services.Job;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BTL_LTW_DOTJOB.Controllers
{
    [AllowAnonymous] // Chi tiết việc làm công khai, chỉ hành động ứng tuyển mới yêu cầu đăng nhập
    [Route("jobs")]
    public class JobController : Controller
    {

        private readonly IJobService _jobService;
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
