using BTL_LTW_DOTJOB.ViewModels.Auth;
using BCrypt.Net;
using BTL_LTW_DOTJOB.Models;

namespace BTL_LTW_DOTJOB.Services.Auth
{
    public class AuthService : IAuthService
    {
        public bool RegisterUser(RegisterViewModel model)
        {
            //kiem tra email da ton tai trong database chua
            
            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(model.Password);

            User newUser = new User
            {
                Role = model.Role,
                FullName = model.FullName,
                Email = model.Email,
                Phone = model.Phone,
                PasswordHash = hashedPassword,
                CreatedAt = DateTime.Now
            };

            return true;
        }
    }
}
