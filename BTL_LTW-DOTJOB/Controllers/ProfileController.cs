using BTL_LTW_DOTJOB.Helpers;
using BTL_LTW_DOTJOB.Services.Profile;
using BTL_LTW_DOTJOB.ViewModels.Profile;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BTL_LTW_DOTJOB.Controllers
{
    // Hồ sơ cá nhân dùng chung cho mọi vai trò (Admin, Employer, Candidate). Mỗi người chỉ sửa được hồ sơ của chính mình
    [Authorize]
    [Route("profile")]
    public class ProfileController : Controller
    {
        private readonly IProfileService _profileService;

        public ProfileController(IProfileService profileService)
        {
            _profileService = profileService;
        }

        // GET: /profile
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Challenge();
            }

            var model = await _profileService.GetProfileAsync(userId);
            if (model == null)
            {
                return Challenge();
            }

            return View(model);
        }

        // POST: /profile - Lưu thông tin cá nhân (và thông tin công ty nếu là Employer)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(ProfileViewModel model)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var role = User.FindFirstValue(ClaimTypes.Role);
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(role))
            {
                return Challenge();
            }

            // Gán vai trò từ Claim rồi mới kiểm tra, để rule "Employer bắt buộc nhập công ty" dùng đúng vai trò thật
            model.Role = role;
            ModelState.Clear();
            if (!TryValidateModel(model))
            {
                await _profileService.FillDisplayInfoAsync(model, userId);
                return View("Index", model);
            }

            var (success, message) = await _profileService.UpdateProfileAsync(userId, model);
            if (!success)
            {
                ModelState.AddModelError(string.Empty, message);
                await _profileService.FillDisplayInfoAsync(model, userId);
                return View("Index", model);
            }

            // Tên hiển thị nằm trong cookie đăng nhập => đăng nhập lại với Claim Name mới để Menu hiện đúng ngay
            await RefreshSignInAsync(model.FullName.Trim());

            TempData["SuccessMessage"] = message;
            return RedirectToAction(nameof(Index));
        }

        // POST: /profile/password - Đổi mật khẩu
        [HttpPost("password")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Challenge();
            }

            if (!ModelState.IsValid)
            {
                TempData["PasswordError"] = ModelState.Values.SelectMany(v => v.Errors).First().ErrorMessage;
                return RedirectToAction(nameof(Index));
            }

            var (success, message) = await _profileService.ChangePasswordAsync(userId, model);

            TempData[success ? "SuccessMessage" : "PasswordError"] = message;
            return RedirectToAction(nameof(Index));
        }

        private async Task RefreshSignInAsync(string fullName)
        {
            // Giữ nguyên kiểu đăng nhập hiện tại (có hay không "Ghi nhớ đăng nhập") và thời hạn cũ của cookie
            var current = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            var claims = User.Claims
                .Where(c => c.Type != ClaimTypes.Name)
                .Append(new Claim(ClaimTypes.Name, fullName));

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(identity),
                new AuthenticationProperties
                {
                    IsPersistent = current.Properties?.IsPersistent ?? false,
                    ExpiresUtc = current.Properties?.ExpiresUtc
                });

            // Cập nhật luôn tên trong Session["User"]
            var sessionUser = HttpContext.Session.GetUser();
            if (sessionUser != null)
            {
                sessionUser.FullName = fullName;
                HttpContext.Session.SetUser(sessionUser);
            }
        }
    }
}
