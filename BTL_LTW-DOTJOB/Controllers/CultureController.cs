using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;

namespace BTL_LTW_DOTJOB.Controllers
{
    // Đổi ngôn ngữ giao diện: lưu lựa chọn vào cookie rồi quay lại trang đang xem
    [AllowAnonymous]
    [Route("culture")]
    public class CultureController : Controller
    {
        private static readonly string[] SupportedCultures = { "vi-VN", "en-US" };

        // GET: /culture/set?culture=en-US&returnUrl=/Employer/Jobs
        [HttpGet("set")]
        public IActionResult Set(string culture, string? returnUrl = null)
        {
            // Chỉ nhận ngôn ngữ đã hỗ trợ, giá trị lạ thì về tiếng Việt
            if (!SupportedCultures.Contains(culture))
            {
                culture = "vi-VN";
            }

            Response.Cookies.Append(
                CookieRequestCultureProvider.DefaultCookieName,
                CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
                new CookieOptions
                {
                    Expires = DateTimeOffset.UtcNow.AddYears(1),
                    IsEssential = true,
                    HttpOnly = true,
                    SameSite = SameSiteMode.Lax,
                    Secure = Request.IsHttps
                });

            // Chỉ quay lại địa chỉ nội bộ (chống chuyển hướng ra site khác)
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return LocalRedirect(returnUrl);
            }

            return RedirectToAction("Index", "Home");
        }
    }
}
