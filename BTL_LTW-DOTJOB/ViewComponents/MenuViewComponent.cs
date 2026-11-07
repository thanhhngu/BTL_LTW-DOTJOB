using BTL_LTW_DOTJOB.Services.Profile;
using BTL_LTW_DOTJOB.ViewModels.Profile;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BTL_LTW_DOTJOB.ViewComponents
{
    public class MenuViewComponent : ViewComponent
    {
        private readonly IProfileService _profileService;

        public MenuViewComponent(IProfileService profileService)
        {
            _profileService = profileService;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            // Khách chưa đăng nhập: không có hồ sơ, View vẫn hiện nút Đăng nhập / Đăng ký
            ProfileViewModel? model = null;

            var userId = UserClaimsPrincipal.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!string.IsNullOrEmpty(userId))
            {
                model = await _profileService.GetProfileAsync(userId);
            }

            return View(model);
        }
    }
}