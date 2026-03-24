using bikey.Models;
using bikey.Repository;
using bikey.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace bikey.Controllers
{
    [Authorize]
    public class NguoiDungController : Controller
    {
        private readonly BikeyDbContext _context;

        public NguoiDungController(BikeyDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userId = int.TryParse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value, out var parsedUserId)
                ? parsedUserId
                : 0;

            var permission = userId > 0
                ? await _context.PhanQuyen.AsNoTracking().FirstOrDefaultAsync(item => item.UserId == userId)
                : null;

            var canManageUsers = permission?.CanViewUser == true || permission?.CanEditUser == true ||
                                permission?.CanCreateUser == true || permission?.CanDeleteUser == true;

            if (!canManageUsers)
            {
                return Redirect("/AccessDenied");
            }

            return View(await BuildViewModelAsync());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateNguoiDungInput input)
        {
            var userId = int.TryParse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value, out var parsedUserId)
                ? parsedUserId
                : 0;

            var userPermission = userId > 0
                ? await _context.PhanQuyen.AsNoTracking().FirstOrDefaultAsync(item => item.UserId == userId)
                : null;

            if (userPermission?.CanCreateUser != true)
            {
                return Redirect("/AccessDenied");
            }

            if (!ModelState.IsValid)
            {
                TempData["UserManagementError"] = "Thông tin không hợp lệ.";
                return View("Index", await BuildViewModelAsync());
            }

            var normalizedEmail = NormalizeEmail(input.Email);

            if (await _context.NguoiDung.AnyAsync(item => item.Email != null && item.Email.ToLower() == normalizedEmail))
            {
                TempData["UserManagementError"] = "Email đã tồn tại.";
                return View("Index", await BuildViewModelAsync());
            }

            if (await _context.NguoiDung.AnyAsync(item => item.SoDienThoai == input.SoDienThoai))
            {
                TempData["UserManagementError"] = "Số điện thoại đã tồn tại.";
                return View("Index", await BuildViewModelAsync());
            }

            var user = new Models.NguoiDung
            {
                Ten = input.HoTen.Trim(),
                Email = normalizedEmail,
                SoDienThoai = input.SoDienThoai.Trim(),
                MatKhau = input.MatKhau,
                VaiTro = input.VaiTro,
                IsActive = true,
                NgayTao = DateTime.Now
            };

            _context.NguoiDung.Add(user);
            await _context.SaveChangesAsync();

            var permission = CreatePermissionEntity(user.Id, GetDefaultPermissionsByRole(user.VaiTro));
            _context.PhanQuyen.Add(permission);
            await _context.SaveChangesAsync();

            TempData["UserManagementSuccess"] = "Thêm người dùng mới thành công.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditNguoiDungInput input)
        {
            var userId = int.TryParse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value, out var parsedUserId)
                ? parsedUserId
                : 0;

            var userPermission = userId > 0
                ? await _context.PhanQuyen.AsNoTracking().FirstOrDefaultAsync(item => item.UserId == userId)
                : null;

            if (userPermission?.CanEditUser != true)
            {
                return Redirect("/AccessDenied");
            }

            if (!ModelState.IsValid)
            {
                TempData["UserManagementError"] = "Thông tin không hợp lệ.";
                return View("Index", await BuildViewModelAsync());
            }

            var user = await _context.NguoiDung.FirstOrDefaultAsync(item => item.Id == input.Id);
            if (user is null)
            {
                return NotFound();
            }

            var normalizedEmail = NormalizeEmail(input.Email);

            if (await _context.NguoiDung.AnyAsync(item => item.Id != input.Id && item.Email != null && item.Email.ToLower() == normalizedEmail))
            {
                TempData["UserManagementError"] = "Email đã tồn tại.";
                return RedirectToAction(nameof(Index));
            }

            if (await _context.NguoiDung.AnyAsync(item => item.Id != input.Id && item.SoDienThoai == input.SoDienThoai))
            {
                TempData["UserManagementError"] = "Số điện thoại đã tồn tại.";
                return RedirectToAction(nameof(Index));
            }

            user.Ten = input.HoTen.Trim();
            user.Email = normalizedEmail;
            user.SoDienThoai = input.SoDienThoai.Trim();
            user.DiaChi = string.IsNullOrWhiteSpace(input.DiaChi) ? null : input.DiaChi.Trim();
            user.VaiTro = input.VaiTro;
            user.IsActive = input.IsActive;

            if (!string.IsNullOrWhiteSpace(input.MatKhauMoi))
            {
                user.MatKhau = input.MatKhauMoi;
            }

            var permission = await EnsurePermissionEntityAsync(user.Id, user.VaiTro);
            await _context.SaveChangesAsync();

            TempData["UserManagementSuccess"] = "Cập nhật thông tin người dùng thành công.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var currentUserId = int.TryParse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value, out var parsedUserId)
                ? parsedUserId
                : 0;

            var userPermission = currentUserId > 0
                ? await _context.PhanQuyen.AsNoTracking().FirstOrDefaultAsync(item => item.UserId == currentUserId)
                : null;

            if (userPermission?.CanDeleteUser != true)
            {
                return Redirect("/AccessDenied");
            }

            var user = await _context.NguoiDung.FirstOrDefaultAsync(item => item.Id == id);
            if (user is null)
            {
                return NotFound();
            }

            if (user.Id == currentUserId)
            {
                TempData["UserManagementError"] = "Không thể xóa tài khoản đang đăng nhập.";
                return RedirectToAction(nameof(Index));
            }

            var permission = await _context.PhanQuyen.FirstOrDefaultAsync(item => item.UserId == user.Id);
            if (permission is not null)
            {
                _context.PhanQuyen.Remove(permission);
            }

            _context.NguoiDung.Remove(user);
            await _context.SaveChangesAsync();

            TempData["UserManagementSuccess"] = "Xóa người dùng thành công.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Permission(int id)
        {
            var currentUserId = int.TryParse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value, out var parsedUserId)
                ? parsedUserId
                : 0;

            var currentUserPermission = currentUserId > 0
                ? await _context.PhanQuyen.AsNoTracking().FirstOrDefaultAsync(item => item.UserId == currentUserId)
                : null;

            if (currentUserPermission?.CanEditUser != true)
            {
                return Redirect("/AccessDenied");
            }

            var user = await _context.NguoiDung
                .AsNoTracking()
                .FirstOrDefaultAsync(item => item.Id == id);

            if (user is null)
            {
                return NotFound();
            }

            if (user.Id == 1)
            {
                TempData["UserManagementError"] = "Không thể thay đổi quyền cho tài khoản admin mặc định.";
                return RedirectToAction(nameof(Index));
            }

            var permission = await _context.PhanQuyen
                .AsNoTracking()
                .FirstOrDefaultAsync(item => item.UserId == id);

            var model = new UserPermissionEditorViewModel
            {
                UserId = user.Id,
                UserName = user.Ten ?? string.Empty,
                UserEmail = user.Email ?? string.Empty,
                UserRole = user.VaiTro,
                Permissions = permission is not null ? MapPermissionSet(permission) : GetDefaultPermissionsByRole(user.VaiTro)
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Permission(UserPermissionEditorViewModel model)
        {
            var currentUserId = int.TryParse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value, out var parsedUserId)
                ? parsedUserId
                : 0;

            var currentUserPermission = currentUserId > 0
                ? await _context.PhanQuyen.AsNoTracking().FirstOrDefaultAsync(item => item.UserId == currentUserId)
                : null;

            if (currentUserPermission?.CanEditUser != true)
            {
                return Redirect("/AccessDenied");
            }

            var user = await _context.NguoiDung.FirstOrDefaultAsync(item => item.Id == model.UserId);
            if (user is null)
            {
                return NotFound();
            }

            if (user.Id == 1)
            {
                TempData["UserManagementError"] = "Không thể thay đổi quyền cho tài khoản admin mặc định.";
                return RedirectToAction(nameof(Index));
            }

            var permission = await EnsurePermissionEntityAsync(user.Id, user.VaiTro);
            ApplyPermissionSet(permission, model.Permissions);
            EnforceViewPermissionLogic(permission);
            await _context.SaveChangesAsync();

            TempData["UserManagementSuccess"] = $"Cập nhật quyền cho tài khoản {user.Ten} thành công.";
            return RedirectToAction(nameof(Index));
        }

        private async Task<NguoiDungManagementViewModel> BuildViewModelAsync()
        {
            ViewBag.Roles = BuildRoleSelectList();

            var users = await _context.NguoiDung
                .AsNoTracking()
                .OrderByDescending(item => item.NgayTao)
                .ToListAsync();

            var permissions = await _context.PhanQuyen
                .AsNoTracking()
                .ToListAsync();

            return new NguoiDungManagementViewModel
            {
                Users = users
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

        private static string NormalizeEmail(string email)
        {
            return email.Trim().ToLowerInvariant();
        }

        private async Task<PhanQuyen> EnsurePermissionEntityAsync(int userId, string role)
        {
            var permission = await _context.PhanQuyen.FirstOrDefaultAsync(item => item.UserId == userId);
            if (permission is not null)
            {
                return permission;
            }

            permission = CreatePermissionEntity(userId, GetDefaultPermissionsByRole(role));
            _context.PhanQuyen.Add(permission);
            return permission;
        }

        private static PhanQuyen CreatePermissionEntity(int userId, PermissionSetInput input)
        {
            var permission = new PhanQuyen
            {
                UserId = userId
            };

            ApplyPermissionSet(permission, input);
            return permission;
        }

        private static PermissionSetInput MapPermissionSet(PhanQuyen permission)
        {
            return new PermissionSetInput
            {
                CanViewXe = permission.CanViewXe,
                CanCreateXe = permission.CanCreateXe,
                CanEditXe = permission.CanEditXe,
                CanDeleteXe = permission.CanDeleteXe,
                CanViewLoaiXe = permission.CanViewLoaiXe,
                CanCreateLoaiXe = permission.CanCreateLoaiXe,
                CanEditLoaiXe = permission.CanEditLoaiXe,
                CanDeleteLoaiXe = permission.CanDeleteLoaiXe,
                CanViewHopDong = permission.CanViewHopDong,
                CanProcessBooking = permission.CanProcessBooking,
                CanReturnVehicle = permission.CanReturnVehicle,
                CanPrintHopDong = permission.CanPrintHopDong,
                CanViewHoaDon = permission.CanViewHoaDon,
                CanPrintHoaDon = permission.CanPrintHoaDon,
                CanViewUser = permission.CanViewUser,
                CanCreateUser = permission.CanCreateUser,
                CanEditUser = permission.CanEditUser,
                CanDeleteUser = permission.CanDeleteUser,
                CanViewBaoCao = permission.CanViewBaoCao,
                CanViewThongKe = permission.CanViewThongKe,
                CanExportBaoCao = permission.CanExportBaoCao,
                CanDatCho = permission.CanDatCho,
                CanViewDatCho = permission.CanViewDatCho
            };
        }

        private static void ApplyPermissionSet(PhanQuyen target, PermissionSetInput input)
        {
            target.CanViewXe = input.CanViewXe;
            target.CanCreateXe = input.CanCreateXe;
            target.CanEditXe = input.CanEditXe;
            target.CanDeleteXe = input.CanDeleteXe;

            target.CanViewLoaiXe = input.CanViewLoaiXe;
            target.CanCreateLoaiXe = input.CanCreateLoaiXe;
            target.CanEditLoaiXe = input.CanEditLoaiXe;
            target.CanDeleteLoaiXe = input.CanDeleteLoaiXe;

            target.CanViewHopDong = input.CanViewHopDong;
            target.CanProcessBooking = input.CanProcessBooking;
            target.CanReturnVehicle = input.CanReturnVehicle;
            target.CanPrintHopDong = input.CanPrintHopDong;

            target.CanViewHoaDon = input.CanViewHoaDon;
            target.CanPrintHoaDon = input.CanPrintHoaDon;

            target.CanViewUser = input.CanViewUser;
            target.CanCreateUser = input.CanCreateUser;
            target.CanEditUser = input.CanEditUser;
            target.CanDeleteUser = input.CanDeleteUser;

            target.CanViewBaoCao = input.CanViewBaoCao;
            target.CanViewThongKe = input.CanViewThongKe;
            target.CanExportBaoCao = input.CanExportBaoCao;

            target.CanDatCho = input.CanDatCho;
            target.CanViewDatCho = input.CanViewDatCho;
        }

        private static void EnforceViewPermissionLogic(PhanQuyen permission)
        {
            if (!permission.CanViewXe)
            {
                permission.CanCreateXe = false;
                permission.CanEditXe = false;
                permission.CanDeleteXe = false;
            }

            if (!permission.CanViewLoaiXe)
            {
                permission.CanCreateLoaiXe = false;
                permission.CanEditLoaiXe = false;
                permission.CanDeleteLoaiXe = false;
            }

            if (!permission.CanViewHopDong)
            {
                permission.CanProcessBooking = false;
                permission.CanReturnVehicle = false;
                permission.CanPrintHopDong = false;
            }

            if (!permission.CanViewHoaDon)
            {
                permission.CanPrintHoaDon = false;
            }

            if (!permission.CanViewUser)
            {
                permission.CanCreateUser = false;
                permission.CanEditUser = false;
                permission.CanDeleteUser = false;
            }

        }

        private static PermissionSetInput GetDefaultPermissionsByRole(string role)
        {
            if (string.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase))
            {
                return new PermissionSetInput
                {
                    CanViewXe = true,
                    CanCreateXe = true,
                    CanEditXe = true,
                    CanDeleteXe = true,
                    CanViewLoaiXe = true,
                    CanCreateLoaiXe = true,
                    CanEditLoaiXe = true,
                    CanDeleteLoaiXe = true,
                    CanViewHopDong = true,
                    CanProcessBooking = true,
                    CanReturnVehicle = true,
                    CanPrintHopDong = true,
                    CanViewHoaDon = true,
                    CanPrintHoaDon = true,
                    CanViewUser = true,
                    CanCreateUser = true,
                    CanEditUser = true,
                    CanDeleteUser = true,
                    CanViewBaoCao = true,
                    CanViewThongKe = true,
                    CanExportBaoCao = true,
                    CanDatCho = true,
                    CanViewDatCho = true
                };
            }

            if (string.Equals(role, "Staff", StringComparison.OrdinalIgnoreCase))
            {
                return new PermissionSetInput
                {
                    CanViewXe = true,
                    CanCreateXe = true,
                    CanEditXe = true,
                    CanDeleteXe = false,
                    CanViewLoaiXe = true,
                    CanCreateLoaiXe = true,
                    CanEditLoaiXe = true,
                    CanDeleteLoaiXe = false,
                    CanViewHopDong = true,
                    CanProcessBooking = true,
                    CanReturnVehicle = true,
                    CanPrintHopDong = true,
                    CanViewHoaDon = true,
                    CanPrintHoaDon = true,
                    CanViewUser = false,
                    CanCreateUser = false,
                    CanEditUser = false,
                    CanDeleteUser = false,
                    CanViewBaoCao = true,
                    CanViewThongKe = true,
                    CanExportBaoCao = true,
                    CanDatCho = true,
                    CanViewDatCho = true
                };
            }
            return new PermissionSetInput();
        }
    }
}
