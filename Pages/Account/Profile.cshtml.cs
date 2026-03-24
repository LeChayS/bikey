using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using bikey.Repository;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace bikey.Pages.Account
{
    [Authorize]
    public class ProfileModel : PageModel
    {
        private readonly BikeyDbContext _context;

        public ProfileModel(BikeyDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        [TempData]
        public string? StatusMessage { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await FindCurrentUserAsync();

            if (user is not null)
            {
                Input = BuildInputFromUser(user);
                return Page();
            }

            Input = BuildInputFromClaims();
            return Page();
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
                ModelState.AddModelError(string.Empty, "Không tìm thấy tài khoản để cập nhật thông tin.");
                return Page();
            }

            var normalizedEmail = Input.Email.Trim().ToLowerInvariant();

            var duplicateEmail = await _context.NguoiDung
                .AnyAsync(item => item.Id != user.Id && item.Email != null && item.Email.ToLower() == normalizedEmail);

            if (duplicateEmail)
            {
                ModelState.AddModelError(nameof(Input.Email), "Email này đã được sử dụng.");
                return Page();
            }

            var duplicatePhone = await _context.NguoiDung
                .AnyAsync(item => item.Id != user.Id && item.SoDienThoai == Input.SoDienThoai);

            if (duplicatePhone)
            {
                ModelState.AddModelError(nameof(Input.SoDienThoai), "Số điện thoại này đã được sử dụng.");
                return Page();
            }

            user.Ten = Input.HoTen.Trim();
            user.SoDienThoai = Input.SoDienThoai.Trim();
            user.Email = normalizedEmail;
            user.DiaChi = Input.DiaChi.Trim();

            await _context.SaveChangesAsync();
            await RefreshSignInAsync(user);

            StatusMessage = "Cập nhật thông tin thành công.";
            return RedirectToPage();
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

        private InputModel BuildInputFromClaims()
        {
            return new InputModel
            {
                HoTen = User.FindFirstValue(ClaimTypes.Name) ?? string.Empty,
                SoDienThoai = User.FindFirstValue(ClaimTypes.MobilePhone) ?? string.Empty,
                Email = User.FindFirstValue(ClaimTypes.Email) ?? "chuacapnhat@bikey.local",
                DiaChi = User.FindFirstValue(ClaimTypes.StreetAddress) ?? "TP.HCM"
            };
        }

        private static InputModel BuildInputFromUser(Models.NguoiDung user)
        {
            return new InputModel
            {
                HoTen = user.Ten ?? string.Empty,
                SoDienThoai = user.SoDienThoai ?? string.Empty,
                Email = user.Email ?? string.Empty,
                DiaChi = user.DiaChi ?? string.Empty
            };
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
            [Required(ErrorMessage = "Vui lòng nhập họ và tên")]
            [Display(Name = "Họ và tên")]
            public string HoTen { get; set; } = string.Empty;

            [Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
            [Display(Name = "Số điện thoại")]
            [RegularExpression(@"^\d{10}$", ErrorMessage = "Số điện thoại phải có đúng 10 chữ số.")]
            public string SoDienThoai { get; set; } = string.Empty;

            [Required(ErrorMessage = "Vui lòng nhập email")]
            [EmailAddress(ErrorMessage = "Email không hợp lệ")]
            public string Email { get; set; } = string.Empty;

            // [Required(ErrorMessage = "Vui lòng nhập địa chỉ")]
            [Display(Name = "Địa chỉ")]
            public string DiaChi { get; set; } = string.Empty;
        }
    }
}
