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
        public AuthService(AppDbContext db)
        {
            _db = db;
        }
        //register user
        public async Task<bool> RegisterUserAsync(RegisterViewModel model)
        {
            //check if user already exists
            //if (await UserExistsAsync(model.Username))
            //{
            //    return false;
            //}

            //hash password
            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(model.Password);
            //create user object

            var newUser = new User
            {
                RoleId = model.RoleId,
                FullName = model.FullName,
                Email = model.Email,
                Phone = model.Phone,
                PasswordHash = hashedPassword,
                CreatedAt = DateTime.Now,
                IsActive = true
            };
            if (model.RoleId == "E")
            {
                newUser.Company = new Company
                {
                    CompanyName = model.CompanyName,
                    CompanyAddress = model.CompanyAddress,
                    WorkLocation = model.WorkLocation,
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
        public async Task<User> AuthenticateAsync(string username, string password)
        {
            
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == username);
            if (user != null && BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            {
                return user;
            }
            
            return null;
            
        }
    }
}
