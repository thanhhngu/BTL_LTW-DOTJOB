using BTL_LTW_DOTJOB.Data;
using BTL_LTW_DOTJOB.ViewModels.Admin;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace BTL_LTW_DOTJOB.Services.Admin
{
    public class UserManagementService : IUserManagementService
    {
        private readonly AppDbContext _db;
        private readonly IMemoryCache _cache;

        public UserManagementService(AppDbContext db, IMemoryCache cache)
        {
            _db = db;
            _cache = cache;
        }

        public async Task<UserListViewModel> GetPagedAsync(string? keyword, string? role, string? status, int page)
        {
            var query = _db.Users.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                var kw = keyword.Trim();
                query = query.Where(u => u.FullName.Contains(kw) || u.Email.Contains(kw));
            }

            if (!string.IsNullOrWhiteSpace(role))
            {
                query = query.Where(u => u.Role!.Name == role);
            }

            if (status == "active") query = query.Where(u => u.IsActive);
            else if (status == "locked") query = query.Where(u => !u.IsActive);

            var model = new UserListViewModel
            {
                Keyword = keyword,
                Role = role,
                Status = status,
                Page = page < 1 ? 1 : page
            };

            model.TotalItems = await query.CountAsync();

            model.Users = await query
                .OrderByDescending(u => u.CreatedAt)
                .Skip((model.Page - 1) * model.PageSize)
                .Take(model.PageSize)
                .Select(u => new UserListItemViewModel
                {
                    Id = u.Id,
                    FullName = u.FullName,
                    Email = u.Email,
                    Phone = u.Phone,
                    RoleName = u.Role!.Name,
                    IsActive = u.IsActive,
                    CreatedAt = u.CreatedAt
                })
                .ToListAsync();

            return model;
        }

        public async Task<(bool Success, string Message)> ToggleLockAsync(string userId, string currentAdminId)
        {
            if (userId == currentAdminId)
            {
                return (false, "Bạn không thể khóa chính tài khoản của mình.");
            }

            var user = await _db.Users.Include(u => u.Role).FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
            {
                return (false, "Tài khoản không tồn tại.");
            }

            if (user.Role?.Name == "Admin")
            {
                return (false, "Không thể khóa tài khoản Admin.");
            }

            user.IsActive = !user.IsActive;
            await _db.SaveChangesAsync();

            _cache.Remove($"user-active:{userId}");

            return (true, user.IsActive
                ? $"Đã mở khóa tài khoản {user.Email}."
                : $"Đã khóa tài khoản {user.Email}.");
        }
    }
}