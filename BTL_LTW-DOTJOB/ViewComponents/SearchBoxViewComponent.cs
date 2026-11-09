using BTL_LTW_DOTJOB.Services.Job;
using BTL_LTW_DOTJOB.ViewModels.Job;
using Microsoft.AspNetCore.Mvc;

namespace BTL_LTW_DOTJOB.ViewComponents
{
    public class SearchBoxViewComponent : ViewComponent
    {
        private readonly IJobService _jobService;

        public SearchBoxViewComponent(IJobService jobService)
        {
            _jobService = jobService;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var model = new SearchJobViewModel();
            await _jobService.FillFilterOptionsAsync(model);

            return View(model);
        }
    }
}
