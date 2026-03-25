using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;

namespace bikey.Services
{
    public interface IUserService
    {
        Task<Models.NguoiDung?> FindCurrentUserAsync(ClaimsPrincipal user);
        int? GetUserIdFromClaims(ClaimsPrincipal user);
        Task RefreshSignInAsync(Models.NguoiDung user, HttpContext context);
        Task<bool> HasPermissionAsync(int userId, Func<Models.PhanQuyen, bool> check);
        Task<bool> HasManageUsersPermissionAsync(int userId);
        Task<bool> HasCreateUserPermissionAsync(int userId);
        Task<bool> HasCreateXePermissionAsync(int userId);
        Task<Models.PhanQuyen?> GetPermissionAsync(int userId);
    }

    public class UserService : IUserService
    {
        private readonly Repository.BikeyDbContext _context;

        public UserService(Repository.BikeyDbContext context)
        {
            _context = context;
        }

        public int? GetUserIdFromClaims(ClaimsPrincipal user)
        {
            return int.TryParse(user.FindFirstValue(ClaimTypes.NameIdentifier), out var userId)
                ? userId
                : null;
        }

        public async Task<Models.NguoiDung?> FindCurrentUserAsync(ClaimsPrincipal user)
        {
            if (GetUserIdFromClaims(user) is int userId)
            {
                return await _context.NguoiDung.FirstOrDefaultAsync(u => u.Id == userId);
            }

            var email = user.FindFirstValue(ClaimTypes.Email);
            if (!string.IsNullOrWhiteSpace(email))
            {
                var normalizedEmail = email.Trim().ToLowerInvariant();
                return await _context.NguoiDung.FirstOrDefaultAsync(u =>
                    u.Email != null && u.Email.ToLower() == normalizedEmail);
            }

            return null;
        }

        public async Task<Models.PhanQuyen?> GetPermissionAsync(int userId)
        {
            return await _context.PhanQuyen.AsNoTracking().FirstOrDefaultAsync(p => p.UserId == userId);
        }

        public async Task<bool> HasPermissionAsync(int userId, Func<Models.PhanQuyen, bool> check)
        {
            var permission = await GetPermissionAsync(userId);
            return permission != null && check(permission);
        }

        public async Task<bool> HasManageUsersPermissionAsync(int userId)
        {
            var permission = await GetPermissionAsync(userId);
            return permission != null &&
                (permission.CanViewUser || permission.CanEditUser || permission.CanCreateUser || permission.CanDeleteUser);
        }

        public async Task<bool> HasCreateUserPermissionAsync(int userId)
        {
            var permission = await GetPermissionAsync(userId);
            return permission?.CanCreateUser == true;
        }

        public async Task<bool> HasCreateXePermissionAsync(int userId)
        {
            var permission = await GetPermissionAsync(userId);
            return permission?.CanCreateXe == true;
        }

        public async Task RefreshSignInAsync(Models.NguoiDung user, HttpContext context)
        {
            var claims = new List<Claim>
            {
                new(ClaimTypes.Name, user.Ten ?? user.Email ?? "User"),
                new(ClaimTypes.MobilePhone, user.SoDienThoai ?? string.Empty),
                new(ClaimTypes.NameIdentifier, user.Id.ToString())
            };

            if (!string.IsNullOrWhiteSpace(user.Email))
            {
                claims.Add(new Claim(ClaimTypes.Email, user.Email));
            }

            if (!string.IsNullOrWhiteSpace(user.DiaChi))
            {
                claims.Add(new Claim(ClaimTypes.StreetAddress, user.DiaChi));
            }

            if (!string.IsNullOrWhiteSpace(user.VaiTro))
            {
                claims.Add(new Claim(ClaimTypes.Role, user.VaiTro));
            }

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await context.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal,
                new AuthenticationProperties
                {
                    IsPersistent = true,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddDays(7)
                });
        }
    }
}
