using BTL_LTW_DOTJOB.Services.Auth;
using BTL_LTW_DOTJOB.ViewModels.Auth;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BTL_LTW_DOTJOB.Controllers
{
    public class AuthController : Controller
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }
        [HttpGet]
        public IActionResult Login()
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View(new RegisterViewModel { Role = "Candidate" });
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var user = await _authService.AuthenticateAsync(model.Email, model.Password);

            if (user != null)
            {
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Name, user.FullName),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.Role, user.Role)
                };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity),
                    new AuthenticationProperties
                    {
                        IsPersistent = true, 
                        ExpiresUtc = DateTimeOffset.UtcNow.AddDays(7) 
                    });

                if (user.Role == "Employer")
                    return RedirectToAction("Index", "Dashboard", new { area = "Employer" });

                return RedirectToAction("Index", "Home");
            }

            ModelState.AddModelError("", "Email hoặc mật khẩu không chính xác.");
            return View(model);
        }

        //[HttpGet]
        //public async Task<IActionResult> Logout()
        //{
        //    await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        //    return RedirectToAction("Login", "Auth");
        //}
    }
}
