using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using bikey.Repository;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace bikey.Pages.Account
{
    [Authorize]
    public class ChangePasswordModel : PageModel
    {
        private readonly BikeyDbContext _context;

        public ChangePasswordModel(BikeyDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        [TempData]
        public string? StatusMessage { get; set; }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var user = await FindCurrentUserAsync();

            if (user is null)
            {
                ModelState.AddModelError(string.Empty, "Khong tim thay tai khoan de doi mat khau.");
                return Page();
            }

            if (!string.Equals(user.MatKhau, Input.CurrentPassword, StringComparison.Ordinal))
            {
                ModelState.AddModelError(nameof(Input.CurrentPassword), "Mat khau hien tai khong chinh xac.");
                return Page();
            }

            if (string.Equals(Input.CurrentPassword, Input.NewPassword, StringComparison.Ordinal))
            {
                ModelState.AddModelError(nameof(Input.NewPassword), "Mat khau moi phai khac mat khau hien tai.");
                return Page();
            }

            user.MatKhau = Input.NewPassword;
            await _context.SaveChangesAsync();
            await RefreshSignInAsync(user);

            StatusMessage = "Doi mat khau thanh cong.";
            ModelState.Clear();
            Input = new InputModel();
            return Page();
        }

        private async Task<Models.NguoiDung?> FindCurrentUserAsync()
        {
            if (int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
            {
                return await _context.NguoiDung.FirstOrDefaultAsync(item => item.Id == userId);
            }

            var email = User.FindFirstValue(ClaimTypes.Email);
            if (!string.IsNullOrWhiteSpace(email))
            {
                var normalizedEmail = email.Trim().ToLowerInvariant();
                return await _context.NguoiDung.FirstOrDefaultAsync(item =>
                    item.Email != null && item.Email.ToLower() == normalizedEmail);
            }

            return null;
        }

        private async Task RefreshSignInAsync(Models.NguoiDung user)
        {
            var claims = new List<Claim>
            {
                new(ClaimTypes.Name, user.Ten ?? user.Email ?? "User"),
                new(ClaimTypes.MobilePhone, user.SoDienThoai ?? string.Empty)
            };

            claims.Add(new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()));

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

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal,
                new AuthenticationProperties
                {
                    IsPersistent = true,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddDays(7)
                });
        }

        public class InputModel
        {
            [Required(ErrorMessage = "Vui lòng nhập mật khẩu hiện tại")]
            [DataType(DataType.Password)]
            [Display(Name = "Mật khẩu hiện tại")]
            public string CurrentPassword { get; set; } = string.Empty;

            [Required(ErrorMessage = "Vui lòng nhập mật khẩu mới")]
            [DataType(DataType.Password)]
            [Display(Name = "Mật khẩu mới")]
            [StringLength(100, MinimumLength = 6, ErrorMessage = "Mật khẩu mới phải có ít nhất 6 ký tự")]
            public string NewPassword { get; set; } = string.Empty;

            [Required(ErrorMessage = "Vui lòng xác nhận mật khẩu mới")]
            [DataType(DataType.Password)]
            [Compare(nameof(NewPassword), ErrorMessage = "Xác nhận mật khẩu không khớp")]
            [Display(Name = "Xác nhận mật khẩu mới")]
            public string ConfirmPassword { get; set; } = string.Empty;
        }
    }
}
