using System.ComponentModel.DataAnnotations;
using bikey.Repository;
using bikey.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace bikey.Pages.Account
{
    [Authorize]
    public class ChangePasswordModel : PageModel
    {
        private readonly BikeyDbContext _context;

        private readonly IUserService _userService;

        public ChangePasswordModel(BikeyDbContext context, IUserService userService)
        {
            _context = context;
            _userService = userService;
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

            var user = await _userService.FindCurrentUserAsync(User);

            if (user is null)
            {
                ModelState.AddModelError(string.Empty, "Không tìm thấy tài khoản để đổi mật khẩu.");
                return Page();
            }

            if (!string.Equals(user.MatKhau, Input.CurrentPassword, StringComparison.Ordinal))
            {
                ModelState.AddModelError(nameof(Input.CurrentPassword), "Mật khẩu hiện tại không chính xác.");
                return Page();
            }

            if (string.Equals(Input.CurrentPassword, Input.NewPassword, StringComparison.Ordinal))
            {
                ModelState.AddModelError(nameof(Input.NewPassword), "Mật khẩu mới phải khác mật khẩu hiện tại.");
                return Page();
            }

            user.MatKhau = Input.NewPassword;
            await _context.SaveChangesAsync();
            await _userService.RefreshSignInAsync(user, HttpContext);

            StatusMessage = "Đổi mật khẩu thành công.";
            ModelState.Clear();
            Input = new InputModel();
            return Page();
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
