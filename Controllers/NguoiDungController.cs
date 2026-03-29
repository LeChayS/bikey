using bikey.Common;
using bikey.Models;
using bikey.Services;
using bikey.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace bikey.Controllers
{
    [Authorize]
    public class NguoiDungController : BaseController
    {
        private readonly INguoiDungService _nguoiDungService;
        private readonly IOnlineUserService _onlineUserService;
        private readonly IDataChangeCheckService _dataChangeCheckService;

        public NguoiDungController(IUserService userService, INguoiDungService nguoiDungService, IOnlineUserService onlineUserService, IDataChangeCheckService dataChangeCheckService)
            : base(userService)
        {
            _nguoiDungService = nguoiDungService;
            _onlineUserService = onlineUserService;
            _dataChangeCheckService = dataChangeCheckService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var result = await RequirePermissionAsync(p => p.CanViewUser);
            if (result != null) return result;

            return View(await BuildViewModelAsync());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateNguoiDungInput input)
        {
            var result = await RequirePermissionAsync(p => p.CanCreateUser);
            if (result != null) return result;

            if (!ModelState.IsValid)
            {
                TempData["UserManagementError"] = "Thông tin không hợp lệ.";
                return View("Index", await BuildViewModelAsync());
            }

            var normalizedEmail = StringHelpers.NormalizeEmail(input.Email);

            if (await _nguoiDungService.EmailExistsAsync(input.Email))
            {
                TempData["UserManagementError"] = "Email đã tồn tại.";
                return View("Index", await BuildViewModelAsync());
            }

            if (await _nguoiDungService.PhoneExistsAsync(input.SoDienThoai))
            {
                TempData["UserManagementError"] = "Số điện thoại đã tồn tại.";
                return View("Index", await BuildViewModelAsync());
            }

            var user = await _nguoiDungService.CreateAsync(input.HoTen, normalizedEmail, input.SoDienThoai, input.MatKhau, input.VaiTro);

            await _nguoiDungService.CreateDefaultPermissionsAsync(user.Id, user.VaiTro);

            TempData["UserManagementSuccess"] = "Thêm người dùng mới thành công.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditNguoiDungInput input)
        {
            var result = await RequirePermissionAsync(p => p.CanEditUser);
            if (result != null) return result;

            var currentUserId = GetCurrentUserId();
            if (input.Id == 1 && currentUserId != 1)
            {
                return Redirect("/AccessDenied");
            }

            if (!ModelState.IsValid)
            {
                TempData["UserManagementError"] = "Thông tin không hợp lệ.";
                return View("Index", await BuildViewModelAsync());
            }

            var user = await _nguoiDungService.GetByIdAsync(input.Id);
            if (user is null)
            {
                return NotFound();
            }

            if (await _nguoiDungService.EmailExistsAsync(input.Email, input.Id))
            {
                TempData["UserManagementError"] = "Email đã tồn tại.";
                return RedirectToAction(nameof(Index));
            }

            if (await _nguoiDungService.PhoneExistsAsync(input.SoDienThoai, input.Id))
            {
                TempData["UserManagementError"] = "Số điện thoại đã tồn tại.";
                return RedirectToAction(nameof(Index));
            }

            var normalizedEmail = StringHelpers.NormalizeEmail(input.Email);
            await _nguoiDungService.UpdateAsync(input.Id, input.HoTen, normalizedEmail, input.SoDienThoai, input.DiaChi, input.VaiTro, input.IsActive, input.MatKhauMoi);

            TempData["UserManagementSuccess"] = "Cập nhật thông tin người dùng thành công.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await RequirePermissionAsync(p => p.CanDeleteUser);
            if (result != null) return result;

            if (id == 1)
            {
                return Redirect("/AccessDenied");
            }

            var currentUserId = GetCurrentUserId();

            var user = await _nguoiDungService.GetByIdAsync(id);
            if (user is null)
            {
                return NotFound();
            }

            if (user.Id == currentUserId)
            {
                TempData["UserManagementError"] = "Không thể xóa tài khoản đang đăng nhập.";
                return RedirectToAction(nameof(Index));
            }

            if (_onlineUserService.IsUserOnline(user.Id))
            {
                TempData["UserManagementError"] = "Không thể xóa tài khoản đang hoạt động. Vui lòng yêu cầu người dùng đăng xuất trước.";
                return RedirectToAction(nameof(Index));
            }

            await _nguoiDungService.DeleteAsync(id);

            TempData["UserManagementSuccess"] = "Xóa người dùng thành công.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Permission(int id)
        {
            var result = await RequirePermissionAsync(p => p.CanEditUser);
            if (result != null) return result;

            var user = await _nguoiDungService.GetByIdAsync(id);

            if (user is null)
            {
                return NotFound();
            }

            var currentUserId = GetCurrentUserId();
            if (user.Id == currentUserId)
            {
                return Redirect("/AccessDenied");
            }

            if (user.Id == 1 && currentUserId != 1)
            {
                return Redirect("/AccessDenied");
            }

            var permission = await _userService.GetPermissionAsync(id);

            var model = new UserPermissionEditorViewModel
            {
                UserId = user.Id,
                UserName = user.Ten ?? string.Empty,
                UserEmail = user.Email ?? string.Empty,
                UserRole = user.VaiTro,
                Permissions = permission is not null ? _nguoiDungService.MapPermissionSet(permission) : _nguoiDungService.GetDefaultPermissionsByRole(user.VaiTro)
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Permission(UserPermissionEditorViewModel model)
        {
            var result = await RequirePermissionAsync(p => p.CanEditUser);
            if (result != null) return result;

            var user = await _nguoiDungService.GetByIdAsync(model.UserId);
            if (user is null)
            {
                return NotFound();
            }

            var currentUserId = GetCurrentUserId();
            if (user.Id == currentUserId)
            {
                return Redirect("/AccessDenied");
            }

            if (user.Id == 1 && currentUserId != 1)
            {
                return Redirect("/AccessDenied");
            }

            await _nguoiDungService.UpdatePermissionsAsync(user.Id, user.VaiTro, model.Permissions);

            TempData["UserManagementSuccess"] = $"Cập nhật quyền cho tài khoản {user.Ten} thành công.";
            return RedirectToAction(nameof(Index));
        }

        private async Task<NguoiDungManagementViewModel> BuildViewModelAsync()
        {
            ViewBag.Roles = BuildRoleSelectList();

            var users = await _nguoiDungService.GetAllAsync();
            var onlineUserIds = _onlineUserService.GetOnlineUserIds();

            return new NguoiDungManagementViewModel
            {
                Users = users,
                OnlineUserIds = onlineUserIds.ToList()
            };
        }

        private static IReadOnlyList<SelectListItem> BuildRoleSelectList()
        {
            return
            [
                new SelectListItem("Quản trị viên", "Admin"),
                new SelectListItem("Nhân viên", "Staff"),
                new SelectListItem("Khách hàng", "User")
            ];
        }

        /// <summary>
        /// API endpoint for auto-refresh feature - returns checksum of current user list
        /// </summary>
        [HttpPost]
        [Route("NguoiDung/GetDataChecksum")]
        public async Task<IActionResult> GetDataChecksum([FromBody] DataChecksumRequest request)
        {
            var result = await RequirePermissionAsync(p => p.CanViewUser);
            if (result != null) return result;

            try
            {
                var users = await _nguoiDungService.GetAllAsync();
                var checksum = _dataChangeCheckService.GenerateChecksum(users);
                return Json(new { success = true, checksum = checksum });
            }
            catch (Exception)
            {
                return Json(new { success = false, checksum = Guid.NewGuid().ToString() });
            }
        }
    }
}
