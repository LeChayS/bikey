using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace bikey.Pages.Account
{
    [Authorize]
    public class ProfileModel : PageModel
    {
        [BindProperty]
        public InputModel Input { get; set; } = new();

        [TempData]
        public string? StatusMessage { get; set; }

        public void OnGet()
        {
            Input = BuildInputFromClaims();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var claims = User.Claims
                .Where(claim =>
                    claim.Type != ClaimTypes.Name &&
                    claim.Type != ClaimTypes.MobilePhone &&
                    claim.Type != ClaimTypes.Email &&
                    claim.Type != ClaimTypes.StreetAddress)
                .ToList();

            claims.Add(new Claim(ClaimTypes.Name, Input.HoTen));
            claims.Add(new Claim(ClaimTypes.MobilePhone, Input.SoDienThoai));
            claims.Add(new Claim(ClaimTypes.Email, Input.Email));
            claims.Add(new Claim(ClaimTypes.StreetAddress, Input.DiaChi));

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

            StatusMessage = "Cập nhật thông tin thành công.";
            return RedirectToPage();
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

        public class InputModel
        {
            [Required(ErrorMessage = "Vui lòng nhập họ và tên")]
            [Display(Name = "Họ và tên")]
            public string HoTen { get; set; } = string.Empty;

            [Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
            [Display(Name = "Số điện thoại")]
            public string SoDienThoai { get; set; } = string.Empty;

            [Required(ErrorMessage = "Vui lòng nhập email")]
            [EmailAddress(ErrorMessage = "Email không hợp lệ")]
            public string Email { get; set; } = string.Empty;

            [Required(ErrorMessage = "Vui lòng nhập địa chỉ")]
            [Display(Name = "Địa chỉ")]
            public string DiaChi { get; set; } = string.Empty;
        }
    }
}
