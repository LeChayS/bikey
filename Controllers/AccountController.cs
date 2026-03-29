using System.Security.Claims;
using bikey.Common;
using bikey.Repository;
using bikey.Services;
using bikey.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;

namespace bikey.Controllers
{
    public class AccountController : Controller
    {
        private readonly BikeyDbContext _context;
        private readonly INguoiDungService _nguoiDungService;
        private readonly IOnlineUserService _onlineUserService;

        public AccountController(BikeyDbContext context, INguoiDungService nguoiDungService, IOnlineUserService onlineUserService)
        {
            _context = context;
            _nguoiDungService = nguoiDungService;
            _onlineUserService = onlineUserService;
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToDefaultPageByRole();
            }

            return View(new LoginViewModel { ReturnUrl = returnUrl });
        }

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var normalizedEmail = StringHelpers.NormalizeEmail(model.Email);

            var matchedUser = await _context.NguoiDung
                .AsNoTracking()
                .FirstOrDefaultAsync(user =>
                    user.IsActive &&
                    user.Email != null &&
                    user.Email.ToLower() == normalizedEmail &&
                    user.MatKhau == model.MatKhau);

            if (matchedUser is null)
            {
                ModelState.AddModelError(string.Empty, "Email hoặc mật khẩu không chính xác.");
                return View(model);
            }

            await SignInUserAsync(matchedUser);

            if (IsAdminOrStaff(matchedUser.VaiTro))
            {
                return RedirectToAction("Index", "Admin");
            }

            if (!string.IsNullOrWhiteSpace(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
            {
                return Redirect(model.ReturnUrl);
            }

            return RedirectToDefaultPageByRole(matchedUser.VaiTro);
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult Register()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToDefaultPageByRole();
            }

            return View(new RegisterViewModel());
        }

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var normalizedEmail = StringHelpers.NormalizeEmail(model.Email);

            if (await _nguoiDungService.EmailExistsAsync(model.Email))
            {
                ModelState.AddModelError(nameof(model.Email), "Email này đã được sử dụng.");
                return View(model);
            }

            if (await _nguoiDungService.PhoneExistsAsync(model.SoDienThoai))
            {
                ModelState.AddModelError(nameof(model.SoDienThoai), "Số điện thoại này đã được sử dụng.");
                return View(model);
            }

            var newUser = await _nguoiDungService.CreateAsync(model.HoTen, normalizedEmail, model.SoDienThoai, model.MatKhau, "User");

            await SignInUserAsync(newUser);

            return RedirectToAction("Index", "TrangChu");
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View(new ForgotPasswordViewModel());
        }

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var normalizedEmail = StringHelpers.NormalizeEmail(model.Email);
            var user = await _context.NguoiDung
                .FirstOrDefaultAsync(item => item.Email != null && item.Email.ToLower() == normalizedEmail);

            if (user is null)
            {
                ModelState.AddModelError(nameof(model.Email), "Không tìm thấy tài khoản với email này.");
                return View(model);
            }

            user.MatKhau = model.MatKhauMoi;
            await _context.SaveChangesAsync();

            ViewBag.StatusMessage = "Mật khẩu đã được cập nhật thành công. Bạn có thể đăng nhập lại ngay bây giờ.";
            ModelState.Clear();
            return View(new ForgotPasswordViewModel());
        }

        private async Task SignInUserAsync(Models.NguoiDung user)
        {
            var claims = new List<Claim>
            {
                new(ClaimTypes.Name, user.Ten ?? user.Email ?? "User"),
                new(ClaimTypes.MobilePhone, user.SoDienThoai ?? string.Empty)
            };

            if (user.Id > 0)
            {
                claims.Add(new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()));
            }

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

            // Track user as online
            _onlineUserService.UserLoggedIn(user.Id);
        }

        private IActionResult RedirectToDefaultPageByRole(string? role = null)
        {
            var currentRole = role ?? User.FindFirstValue(ClaimTypes.Role);

            if (IsAdminOrStaff(currentRole))
            {
                return RedirectToAction("Index", "Admin");
            }

            return RedirectToAction("Index", "TrangChu");
        }

        private static bool IsAdminOrStaff(string? role)
        {
            return string.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(role, "Staff", StringComparison.OrdinalIgnoreCase);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            // Get current user ID before signing out
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out var userId))
            {
                _onlineUserService.UserLoggedOut(userId);
            }

            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "TrangChu");
        }
    }
}
