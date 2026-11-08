using BTL_LTW_DOTJOB.Data;
using BTL_LTW_DOTJOB.Models;
using BTL_LTW_DOTJOB.ViewModels.Profile;
using Ganss.Xss;
using Microsoft.EntityFrameworkCore;

namespace BTL_LTW_DOTJOB.Services.Profile
{
    public class ProfileService : IProfileService
    {
        private static readonly string[] AllowedImageExt = { ".jpg", ".jpeg", ".png", ".webp" };
        private const long MaxImageSize = 2 * 1024 * 1024;

        private readonly AppDbContext _db;
        private readonly IWebHostEnvironment _env;

        // Làm sạch HTML từ editor (xóa <script>, onerror=...) trước khi lưu phần giới thiệu công ty
        private readonly HtmlSanitizer _sanitizer = new();

        public ProfileService(AppDbContext db, IWebHostEnvironment env)
        {
            _db = db;
            _env = env;
        }

        public async Task<ProfileViewModel?> GetProfileAsync(string userId)
        {
            var user = await _db.Users
                .AsNoTracking()
                .Include(u => u.Role)
                .Include(u => u.Company)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null) return null;

            return new ProfileViewModel
            {
                Email = user.Email,
                Role = user.Role?.Name ?? string.Empty,
                AvatarPath = user.Avatar,
                FullName = user.FullName,
                Phone = user.Phone,
                CompanyName = user.Company?.CompanyName,
                WorkLocation = user.Company?.WorkLocation,
                CompanyAddress = user.Company?.CompanyAddress,
                Website = user.Company?.Website,
                Description = user.Company?.Description,
                LogoPath = user.Company?.Logo
            };
        }

        public async Task FillDisplayInfoAsync(ProfileViewModel model, string userId)
        {
            var user = await _db.Users
                .AsNoTracking()
                .Include(u => u.Role)
                .Include(u => u.Company)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null) return;

            model.Email = user.Email;
            model.Role = user.Role?.Name ?? string.Empty;
            model.AvatarPath = user.Avatar;
            model.LogoPath = user.Company?.Logo;
        }

        public async Task<(bool Success, string Message)> UpdateProfileAsync(string userId, ProfileViewModel model)
        {
            var user = await _db.Users
                .Include(u => u.Role)
                .Include(u => u.Company)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
                return (false, "Không tìm thấy tài khoản.");

            // Vai trò lấy từ DB, không lấy từ form => Candidate/Admin không thể sửa dữ liệu công ty bằng cách giả request
            var isEmployer = user.Role?.Name == "Employer";

            // 1. Kiểm tra file ảnh trước khi ghi bất cứ thứ gì
            var avatarError = ValidateImage(model.AvatarFile);
            if (avatarError != null) return (false, $"Ảnh đại diện: {avatarError}");

            // Logo công ty chỉ nhận JPG/PNG, ảnh đại diện nhận thêm WEBP
            var logoError = isEmployer ? ValidateImage(model.LogoFile, allowWebp: false) : null;
            if (logoError != null) return (false, $"Logo công ty: {logoError}");

            // 2. Thông tin cá nhân (Email và vai trò không cho sửa ở đây)
            user.FullName = model.FullName.Trim();
            user.Phone = model.Phone.Trim();

            var oldAvatar = user.Avatar;
            if (model.AvatarFile != null && model.AvatarFile.Length > 0)
            {
                user.Avatar = await SaveImageAsync(model.AvatarFile, "avatars");
            }

            // 3. Thông tin công ty (chỉ Employer)
            string? oldLogo = null;
            if (isEmployer)
            {
                var company = user.Company;
                if (company == null)
                {
                    company = new Company { UserId = user.Id };
                    _db.Companies.Add(company);
                    user.Company = company;
                }

                company.CompanyName = model.CompanyName!.Trim();
                company.WorkLocation = model.WorkLocation!.Trim();
                company.CompanyAddress = model.CompanyAddress!.Trim();
                company.Website = string.IsNullOrWhiteSpace(model.Website) ? null : model.Website.Trim();
                company.Description = string.IsNullOrWhiteSpace(model.Description) ? null : _sanitizer.Sanitize(model.Description.Trim());

                if (model.LogoFile != null && model.LogoFile.Length > 0)
                {
                    oldLogo = company.Logo;
                    company.Logo = await SaveImageAsync(model.LogoFile, "logos");
                }
            }

            try
            {
                await _db.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                Console.WriteLine(ex.Message);
                return (false, "Không thể lưu thay đổi. Vui lòng thử lại.");
            }

            // 4. Lưu DB thành công mới xóa ảnh cũ để không mất ảnh khi lỗi
            if (oldAvatar != user.Avatar) DeleteImage(oldAvatar);
            if (oldLogo != null) DeleteImage(oldLogo);

            return (true, "Cập nhật hồ sơ thành công!");
        }

        public async Task<(bool Success, string Message)> ChangePasswordAsync(string userId, ChangePasswordViewModel model)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
                return (false, "Không tìm thấy tài khoản.");

            bool currentOk;
            try
            {
                currentOk = BCrypt.Net.BCrypt.Verify(model.CurrentPassword, user.PasswordHash);
            }
            catch (Exception ex)
            {
                // PasswordHash không đúng định dạng BCrypt
                Console.WriteLine(ex.Message);
                currentOk = false;
            }

            if (!currentOk)
                return (false, "Mật khẩu hiện tại không chính xác.");

            if (model.CurrentPassword == model.NewPassword)
                return (false, "Mật khẩu mới phải khác mật khẩu hiện tại.");

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.NewPassword);
            await _db.SaveChangesAsync();

            return (true, "Đổi mật khẩu thành công!");
        }

        // ===== Xử lý ảnh =====

        // Trả về thông báo lỗi, null nếu hợp lệ hoặc không có file
        private static string? ValidateImage(IFormFile? file, bool allowWebp = true)
        {
            if (file == null || file.Length == 0) return null;

            if (file.Length > MaxImageSize)
                return "không được vượt quá 2MB.";

            var allowedExt = allowWebp ? AllowedImageExt : AllowedImageExt.Where(e => e != ".webp").ToArray();
            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!allowedExt.Contains(ext))
                return allowWebp ? "chỉ chấp nhận JPG, PNG, WEBP." : "chỉ chấp nhận JPG, PNG.";

            // Kiểm tra thêm nội dung thật của file: đổi đuôi .exe thành .png vẫn bị chặn
            if (!HasImageSignature(file, allowWebp))
                return "nội dung file không phải ảnh hợp lệ.";

            return null;
        }

        // Đọc vài byte đầu của file để nhận biết JPG / PNG / WEBP thật
        private static bool HasImageSignature(IFormFile file, bool allowWebp)
        {
            var header = new byte[12];
            using var stream = file.OpenReadStream();
            var read = stream.Read(header, 0, header.Length);

            var isJpg = read >= 3 && header[0] == 0xFF && header[1] == 0xD8 && header[2] == 0xFF;
            var isPng = read >= 4 && header[0] == 0x89 && header[1] == 0x50 && header[2] == 0x4E && header[3] == 0x47;
            var isWebp = allowWebp && read >= 12
                && header[0] == 'R' && header[1] == 'I' && header[2] == 'F' && header[3] == 'F'
                && header[8] == 'W' && header[9] == 'E' && header[10] == 'B' && header[11] == 'P';

            return isJpg || isPng || isWebp;
        }

        // Lưu vào wwwroot/uploads/{folder}, trả về đường dẫn web (vd: /uploads/avatars/xxx.png)
        private async Task<string> SaveImageAsync(IFormFile file, string folder)
        {
            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            var dir = Path.Combine(_env.WebRootPath, "uploads", folder);
            Directory.CreateDirectory(dir);

            var fileName = $"{Guid.NewGuid()}{ext}"; // tên ngẫu nhiên, không dùng tên file người dùng gửi
            using (var stream = new FileStream(Path.Combine(dir, fileName), FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return $"/uploads/{folder}/{fileName}";
        }

        // Chỉ xóa file do chính chức năng này tạo ra (nằm trong wwwroot/uploads/avatars hoặc logos)
        private void DeleteImage(string? webPath)
        {
            if (string.IsNullOrEmpty(webPath)) return;
            if (!webPath.StartsWith("/uploads/avatars/") && !webPath.StartsWith("/uploads/logos/")) return;

            var root = Path.GetFullPath(Path.Combine(_env.WebRootPath, "uploads"));
            var full = Path.GetFullPath(Path.Combine(_env.WebRootPath, webPath.TrimStart('/')));
            if (!full.StartsWith(root) || !File.Exists(full)) return;

            try { File.Delete(full); }
            catch (IOException ex) { Console.WriteLine(ex.Message); }
        }
    }
}
