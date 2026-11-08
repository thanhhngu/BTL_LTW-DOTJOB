using BTL_LTW_DOTJOB.Data;
using BTL_LTW_DOTJOB.Services.Application;
using BTL_LTW_DOTJOB.Services.Auth;
using BTL_LTW_DOTJOB.Services.Job;
using BTL_LTW_DOTJOB.Services.Profile;
using BTL_LTW_DOTJOB.Services.SavedJobs;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

// Cache nhỏ dùng để kiểm tra trạng thái khóa tài khoản (IsActive) mà không truy vấn DB ở mọi request
builder.Services.AddMemoryCache();

builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IJobService, JobService>();
builder.Services.AddScoped<IApplicationService, ApplicationService>();
builder.Services.AddScoped<ISavedJobService, SavedJobService>();
builder.Services.AddScoped<IProfileService, ProfileService>();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString));

// Request gửi bằng fetch/AJAX kèm header X-Requested-With: XMLHttpRequest, hoặc gọi vào /api/...
// (API luôn trả 401/403 thay vì chuyển hướng sang trang login)
static bool IsAjaxRequest(HttpRequest request) =>
    request.Headers["X-Requested-With"] == "XMLHttpRequest" || request.Path.StartsWithSegments("/api");

//Cấu hình Cookie
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = "DotJobAuthCookie";
        options.LoginPath = "/Auth/Login";
        options.AccessDeniedPath = "/Auth/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromDays(1);

        // AJAX chưa đăng nhập => trả 401 (JS tự xử lý), thay vì chuyển hướng 302 sang trang login
        options.Events.OnRedirectToLogin = context =>
        {
            if (IsAjaxRequest(context.Request))
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            }
            else
            {
                context.Response.Redirect(context.RedirectUri);
            }
            return Task.CompletedTask;
        };

        // AJAX sai quyền (vd: Employer bấm ứng tuyển) => trả 403
        options.Events.OnRedirectToAccessDenied = context =>
        {
            if (IsAjaxRequest(context.Request))
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
            }
            else
            {
                context.Response.Redirect(context.RedirectUri);
            }
            return Task.CompletedTask;
        };

        // Tài khoản bị khóa (IsActive = false) hoặc đã bị xóa thì cookie cũ mất hiệu lực (trễ tối đa 1 phút do cache)
        options.Events.OnValidatePrincipal = async context =>
        {
            var userId = context.Principal?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                context.RejectPrincipal();
                await context.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                return;
            }

            var cache = context.HttpContext.RequestServices.GetRequiredService<IMemoryCache>();
            var cacheKey = $"user-active:{userId}";

            if (!cache.TryGetValue(cacheKey, out bool _))
            {
                var db = context.HttpContext.RequestServices.GetRequiredService<AppDbContext>();
                var isActive = await db.Users
                    .AsNoTracking()
                    .AnyAsync(user => user.Id == userId && user.IsActive);

                if (!isActive)
                {
                    context.RejectPrincipal();
                    await context.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                    return;
                }

                // Chỉ cache trường hợp đang hoạt động, để mở khóa có hiệu lực ngay
                cache.Set(cacheKey, true, TimeSpan.FromMinutes(1));
            }
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
});

var app = builder.Build();

// Seed dữ liệu mẫu (Role, tài khoản, danh mục, việc làm...). Cần chạy "dotnet ef database update" trước
using (var scope = app.Services.CreateScope())
{
    SeedData.Initialize(scope.ServiceProvider);
}



// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles(); // phục vụ file tải lên lúc chạy (wwwroot/uploads: avatar, logo). MapStaticAssets chỉ phục vụ file có sẵn khi build
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets().AllowAnonymous();

app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
