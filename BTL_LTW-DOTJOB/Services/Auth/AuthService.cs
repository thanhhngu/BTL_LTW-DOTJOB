using BTL_LTW_DOTJOB.ViewModels.Auth;
using BCrypt.Net;
using BTL_LTW_DOTJOB.Models;
using BTL_LTW_DOTJOB.Data;
using Microsoft.EntityFrameworkCore;


namespace BTL_LTW_DOTJOB.Services.Auth
{
    public class AuthService : IAuthService
    {
        private readonly AppDbContext _db;

        // Chỉ cho phép tự đăng ký 2 vai trò này. Admin chỉ được tạo từ SeedData
        private static readonly string[] RegisterableRoles = { "Candidate", "Employer" };

        public AuthService(AppDbContext db)
        {
            _db = db;
        }
        public async Task<bool> UserExistsAsync(string email)
        {
            return await _db.Users.AnyAsync(u => u.Email == email);
        }
        //register user
        public async Task<bool> RegisterUserAsync(RegisterViewModel model)
        {
            // Chặn vai trò ngoài danh sách cho phép (vd: Admin) dù ViewModel đã kiểm tra
            if (!RegisterableRoles.Contains(model.RoleName))
            {
                return false;
            }

            if (await UserExistsAsync(model.Email))
            {
                return false;
            }

            var role = await _db.Roles.FirstOrDefaultAsync(r => r.Name == model.RoleName);
            if (role == null)
            {
                // Chưa có Role trong DB (chưa chạy SeedData)
                return false;
            }

            //hash password
            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(model.Password);
            //create user object

            var newUser = new User
            {
                RoleId = role.Id,
                FullName = model.FullName,
                Email = model.Email,
                Phone = model.Phone,
                PasswordHash = hashedPassword,
                CreatedAt = DateTime.Now,
                IsActive = true
            };
            if (model.RoleName == "Employer")
            {
                newUser.Company = new Company
                {
                    CompanyName = model.CompanyName ?? "Chưa cập nhật tên công ty",
                    CompanyAddress = model.CompanyAddress,
                    WorkLocation = model.WorkLocation
                };
            }

            try
            {
                _db.Users.Add(newUser);
                await _db.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {

                Console.WriteLine(ex.Message);
                return false;
            }
        }
        public async Task<User?> AuthenticateAsync(string username, string password)
        {
            // Include Role để lấy tên quyền (Admin / Employer / Candidate) đưa vào Claim
            var user = await _db.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Email == username);

            if (user == null)
            {
                return null;
            }

            try
            {
                return BCrypt.Net.BCrypt.Verify(password, user.PasswordHash) ? user : null;
            }
            catch (Exception ex)
            {
                // PasswordHash không đúng định dạng BCrypt (vd: dữ liệu cũ hash bằng thuật toán khác)
                Console.WriteLine(ex.Message);
                return null;
            }
        }
    }
}
