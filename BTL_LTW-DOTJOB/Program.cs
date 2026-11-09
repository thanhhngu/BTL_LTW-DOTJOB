using BTL_LTW_DOTJOB.Data;
using BTL_LTW_DOTJOB.Services.Application;
using BTL_LTW_DOTJOB.Services.Auth;
using BTL_LTW_DOTJOB.Services.Job;
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

// Session cần một nơi lưu dữ liệu; dùng bộ nhớ RAM của server (đủ cho đồ án)
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Không thao tác 30 phút thì Session hết hạn
    options.Cookie.Name = "DotJobSession";
    options.Cookie.HttpOnly = true;   // JavaScript không đọc được cookie này (chống XSS)
    options.Cookie.IsEssential = true; // Luôn gửi cookie, không phụ thuộc cookie consent
});
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IJobService, JobService>();
builder.Services.AddScoped<IApplicationService, ApplicationService>();

//Service thống kê cho Dashboard của Admin
builder.Services.AddScoped<BTL_LTW_DOTJOB.Services.Admin.IDashboardService, BTL_LTW_DOTJOB.Services.Admin.DashboardService>();
//Service quản lý danh mục ngành nghề của Admin
builder.Services.AddScoped<BTL_LTW_DOTJOB.Services.Admin.ICategoryService, BTL_LTW_DOTJOB.Services.Admin.CategoryService>();
// Service quản lý người dùng (khóa / mở khóa) của Admin
builder.Services.AddScoped<BTL_LTW_DOTJOB.Services.Admin.IUserManagementService, BTL_LTW_DOTJOB.Services.Admin.UserManagementService>();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString));

// Request gửi bằng fetch/AJAX kèm header X-Requested-With: XMLHttpRequest
static bool IsAjaxRequest(HttpRequest request) =>
    request.Headers["X-Requested-With"] == "XMLHttpRequest";

//Cấu hình Cookie
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = "DotJobAuthCookie";
        options.LoginPath = "/Auth/Login";
        options.AccessDeniedPath = "/Auth/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromDays(1);

        options.Cookie.HttpOnly = true;                  // JavaScript không đọc được cookie đăng nhập
        options.Cookie.SameSite = SameSiteMode.Lax;      // Hạn chế tấn công CSRF từ site khác
        options.SlidingExpiration = true;                // Có hoạt động thì tự gia hạn thời gian đăng nhập
        
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

    //  policy chỉ cho Admin vào 
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
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
app.UseRouting();
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets().AllowAnonymous();

// route cho Area (Admin, Employer)
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
