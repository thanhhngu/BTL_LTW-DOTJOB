using BTL_LTW_DOTJOB.Services.Profile;
using BTL_LTW_DOTJOB.ViewModels.Profile;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BTL_LTW_DOTJOB.Controllers
{
    [Authorize]
    [Route("profile")]
    public class ProfileController : Controller
    {
        private readonly IProfileService _profileService;

        public ProfileController(IProfileService profileService)
        {
            _profileService = profileService;
        }

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

            await RefreshSignInAsync(model.FullName.Trim());

            TempData["SuccessMessage"] = message;
            return RedirectToAction(nameof(Index));
        }

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
            var claims = User.Claims
                .Where(c => c.Type != ClaimTypes.Name)
                .Append(new Claim(ClaimTypes.Name, fullName));

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(identity),
                new AuthenticationProperties
                {
                    IsPersistent = true,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddDays(1)
                });
        }
    }
}
