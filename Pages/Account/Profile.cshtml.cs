using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using bikey.Repository;
using bikey.Services;
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

        private readonly IUserService _userService;

        public ProfileModel(BikeyDbContext context, IUserService userService)
        {
            _context = context;
            _userService = userService;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        [TempData]
        public string? StatusMessage { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userService.FindCurrentUserAsync(User);

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

            var user = await _userService.FindCurrentUserAsync(User);

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
            await _userService.RefreshSignInAsync(user, HttpContext);

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
