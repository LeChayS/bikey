using System.Security.Claims;
using bikey.Repository;
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

        public AccountController(BikeyDbContext context)
        {
            _context = context;
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "TrangChu");
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

            var matchedUser = await _context.NguoiDung
                .AsNoTracking()
                .FirstOrDefaultAsync(user =>
                    user.IsActive &&
                    user.Email == model.Email &&
                    user.MatKhau == model.MatKhau);

            if (matchedUser is null)
            {
                ModelState.AddModelError(string.Empty, "Email hoặc mật khẩu không chính xác.");
                return View(model);
            }

            await SignInUserAsync(matchedUser);

            if (!string.IsNullOrWhiteSpace(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
            {
                return Redirect(model.ReturnUrl);
            }

            return RedirectToAction("Index", "TrangChu");
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult Register()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "TrangChu");
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

            var emailExists = await _context.NguoiDung
                .AnyAsync(user => user.Email == model.Email);

            if (emailExists)
            {
                ModelState.AddModelError(nameof(model.Email), "Email này đã được sử dụng.");
                return View(model);
            }

            var phoneExists = await _context.NguoiDung
                .AnyAsync(user => user.SoDienThoai == model.SoDienThoai);

            if (phoneExists)
            {
                ModelState.AddModelError(nameof(model.SoDienThoai), "Số điện thoại này đã được sử dụng.");
                return View(model);
            }

            var newUser = new Models.NguoiDung
            {
                Ten = model.HoTen,
                Email = model.Email,
                SoDienThoai = model.SoDienThoai,
                MatKhau = model.MatKhau,
                VaiTro = "User",
                IsActive = true,
                NgayTao = DateTime.Now
            };

            _context.NguoiDung.Add(newUser);
            await _context.SaveChangesAsync();

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
        public IActionResult ForgotPassword(ForgotPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            ViewBag.StatusMessage = "Yêu cầu đặt lại mật khẩu đã được ghi nhận. Vui lòng liên hệ quản trị viên hoặc triển khai luồng gửi email để hoàn tất.";
            ModelState.Clear();
            return View(new ForgotPasswordViewModel());
        }

        private async Task SignInUserAsync(Models.NguoiDung user)
        {
            var claims = new List<Claim>
            {
                new(ClaimTypes.Name, user.Ten ?? user.Email ?? "Khach hang"),
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
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "TrangChu");
        }
    }
}
