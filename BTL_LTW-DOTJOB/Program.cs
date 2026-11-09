using BTL_LTW_DOTJOB.Data;
using BTL_LTW_DOTJOB.Services.Application;
using BTL_LTW_DOTJOB.Services.Auth;
using BTL_LTW_DOTJOB.Services.Job;
using BTL_LTW_DOTJOB.Services.Profile;
using BTL_LTW_DOTJOB.Services.Reports;
using BTL_LTW_DOTJOB.Services.SavedJobs;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System.Globalization;
using System.Security.Claims;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// Mọi request POST/PUT/PATCH/DELETE đều tự kiểm tra anti-forgery token, kể cả action mới quên gắn [ValidateAntiForgeryToken].
// Request AJAX/API phải gửi token trong header "RequestVerificationToken" (xem meta csrf-token ở _Layout)
builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
});

// Đa ngôn ngữ (quốc tế hóa): mặc định tiếng Việt, đổi sang tiếng Anh bằng cookie (xem CultureController).
// Chuỗi giao diện nằm trong Resources/SharedResource.resx (vi) và SharedResource.en.resx (en).
// Culture cũng quyết định cách hiển thị ngày giờ, số, tiền (vd: 1.000.000 hay 1,000,000)
builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");
builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    var supportedCultures = new[] { new CultureInfo("vi-VN"), new CultureInfo("en-US") };
    options.DefaultRequestCulture = new RequestCulture("vi-VN");
    options.SupportedCultures = supportedCultures;
    options.SupportedUICultures = supportedCultures;

    // Chỉ theo lựa chọn của người dùng (cookie), bỏ qua ngôn ngữ trình duyệt để mặc định luôn là tiếng Việt,
    // và không phụ thuộc cài đặt Region của máy chạy ứng dụng
    options.RequestCultureProviders = new List<IRequestCultureProvider> { new CookieRequestCultureProvider() };
});

// Cache nhỏ dùng để kiểm tra trạng thái khóa tài khoản (IsActive) mà không truy vấn DB ở mọi request
builder.Services.AddMemoryCache();

// Giới hạn số lần gửi form đăng nhập: tối đa 10 lần/phút cho mỗi địa chỉ IP (chống dò mật khẩu).
// Áp dụng bằng [EnableRateLimiting("login")] trên action Login (POST)
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddPolicy("login", httpContext => RateLimitPartition.GetFixedWindowLimiter(
        httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
        _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = 10,
            Window = TimeSpan.FromMinutes(1),
            QueueLimit = 0
        }));
    options.OnRejected = async (context, cancellationToken) =>
    {
        context.HttpContext.Response.ContentType = "text/plain; charset=utf-8";
        await context.HttpContext.Response.WriteAsync(
            "Bạn đã thử đăng nhập quá nhiều lần. Vui lòng đợi 1 phút rồi thử lại.", cancellationToken);
    };
});

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
builder.Services.AddScoped<ISavedJobService, SavedJobService>();
builder.Services.AddScoped<IProfileService, ProfileService>();
builder.Services.AddScoped<IReportService, ReportService>();

//Service thống kê cho Dashboard của Admin
builder.Services.AddScoped<BTL_LTW_DOTJOB.Services.Admin.IDashboardService, BTL_LTW_DOTJOB.Services.Admin.DashboardService>();
//Service quản lý danh mục ngành nghề của Admin
builder.Services.AddScoped<BTL_LTW_DOTJOB.Services.Admin.ICategoryService, BTL_LTW_DOTJOB.Services.Admin.CategoryService>();
// Service quản lý người dùng (khóa / mở khóa) của Admin
builder.Services.AddScoped<BTL_LTW_DOTJOB.Services.Admin.IUserManagementService, BTL_LTW_DOTJOB.Services.Admin.UserManagementService>();

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

        // Cookie đăng nhập: JavaScript không đọc được, không gửi kèm request từ site khác,
        // và chỉ đi qua HTTPS khi chạy thật (Development vẫn cho http để dùng profile "http")
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.Cookie.SecurePolicy = builder.Environment.IsDevelopment()
            ? CookieSecurePolicy.SameAsRequest
            : CookieSecurePolicy.Always;
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

// Mã lỗi không kèm nội dung (404, 403, 405...) hiện trang báo lỗi thân thiện thay vì trang trắng
app.UseStatusCodePagesWithReExecute("/Home/Status", "?code={0}");

// Header bảo mật cơ bản cho mọi phản hồi (kể cả file CV tải về)
app.Use(async (context, next) =>
{
    var headers = context.Response.Headers;
    headers["X-Content-Type-Options"] = "nosniff";                          // trình duyệt không tự đoán lại loại file
    headers["X-Frame-Options"] = "SAMEORIGIN";                              // chặn nhúng trang vào iframe của site khác
    headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
    await next();
});

app.UseHttpsRedirection();
app.UseStaticFiles(); // phục vụ file tải lên lúc chạy (wwwroot/uploads: avatar, logo). MapStaticAssets chỉ phục vụ file có sẵn khi build
app.UseRequestLocalization();
app.UseRouting();
app.UseRateLimiter();
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
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
