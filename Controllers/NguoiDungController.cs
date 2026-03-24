using bikey.Models;
using bikey.Repository;
using bikey.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace bikey.Controllers
{
    [Authorize(Roles = "Admin")]
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
            return View(await BuildViewModelAsync());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateNguoiDungInput input)
        {
            if (!ModelState.IsValid)
            {
                TempData["UserManagementError"] = "Thong tin tao nguoi dung khong hop le.";
                return View("Index", await BuildViewModelAsync());
            }

            var normalizedEmail = NormalizeEmail(input.Email);

            if (await _context.NguoiDung.AnyAsync(item => item.Email != null && item.Email.ToLower() == normalizedEmail))
            {
                TempData["UserManagementError"] = "Email nay da ton tai.";
                return View("Index", await BuildViewModelAsync());
            }

            if (await _context.NguoiDung.AnyAsync(item => item.SoDienThoai == input.SoDienThoai))
            {
                TempData["UserManagementError"] = "So dien thoai nay da ton tai.";
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

            TempData["UserManagementSuccess"] = "Da them nguoi dung moi.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditNguoiDungInput input)
        {
            if (!ModelState.IsValid)
            {
                TempData["UserManagementError"] = "Thong tin cap nhat khong hop le.";
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
                TempData["UserManagementError"] = "Email nay da ton tai.";
                return RedirectToAction(nameof(Index));
            }

            if (await _context.NguoiDung.AnyAsync(item => item.Id != input.Id && item.SoDienThoai == input.SoDienThoai))
            {
                TempData["UserManagementError"] = "So dien thoai nay da ton tai.";
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

            ApplyRestrictedPermissions(user.VaiTro, permission);
            await _context.SaveChangesAsync();

            TempData["UserManagementSuccess"] = "Da cap nhat thong tin nguoi dung.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var user = await _context.NguoiDung.FirstOrDefaultAsync(item => item.Id == id);
            if (user is null)
            {
                return NotFound();
            }

            var currentUserId = int.TryParse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value, out var parsedUserId)
                ? parsedUserId
                : 0;

            if (user.Id == currentUserId)
            {
                TempData["UserManagementError"] = "Ban khong the xoa chinh tai khoan dang dang nhap.";
                return RedirectToAction(nameof(Index));
            }

            var permission = await _context.PhanQuyen.FirstOrDefaultAsync(item => item.UserId == user.Id);
            if (permission is not null)
            {
                _context.PhanQuyen.Remove(permission);
            }

            _context.NguoiDung.Remove(user);
            await _context.SaveChangesAsync();

            TempData["UserManagementSuccess"] = "Da xoa nguoi dung.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Permission(int id)
        {
            var user = await _context.NguoiDung
                .AsNoTracking()
                .FirstOrDefaultAsync(item => item.Id == id);

            if (user is null)
            {
                return NotFound();
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
            var user = await _context.NguoiDung.FirstOrDefaultAsync(item => item.Id == model.UserId);
            if (user is null)
            {
                return NotFound();
            }

            var permission = await EnsurePermissionEntityAsync(user.Id, user.VaiTro);
            ApplyPermissionSet(permission, model.Permissions);
            ApplyRestrictedPermissions(user.VaiTro, permission);

            await _context.SaveChangesAsync();

            TempData["UserManagementSuccess"] = $"Da cap nhat phan quyen cho tai khoan {user.Ten}.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveRolePermissions(RolePermissionMatrixViewModel rolePermissions)
        {
            await ApplyPermissionsToRoleAsync("Admin", rolePermissions.Admin);
            await ApplyPermissionsToRoleAsync("Staff", rolePermissions.Staff);
            await ApplyPermissionsToCustomerRolesAsync(rolePermissions.KhachHang);

            await _context.SaveChangesAsync();

            TempData["UserManagementSuccess"] = "Da cap nhat phan quyen theo loai tai khoan.";
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
                Users = users,
                RolePermissions = new RolePermissionMatrixViewModel
                {
                    Admin = BuildRolePermissionSet(users, permissions, "Admin"),
                    Staff = BuildRolePermissionSet(users, permissions, "Staff"),
                    KhachHang = BuildRolePermissionSet(users, permissions, "Khách hàng", "User")
                }
            };
        }

        private static IReadOnlyList<SelectListItem> BuildRoleSelectList()
        {
            return
            [
                new SelectListItem("Quản trị viên", "Admin"),
                new SelectListItem("Nhân viên", "Staff"),
                new SelectListItem("Khách hàng", "Khách hàng")
            ];
        }

        private static string NormalizeEmail(string email)
        {
            return email.Trim().ToLowerInvariant();
        }

        private PermissionSetInput BuildRolePermissionSet(
            IReadOnlyList<NguoiDung> users,
            IReadOnlyList<PhanQuyen> permissions,
            params string[] roles)
        {
            var matchedUserIds = users
                .Where(user => roles.Any(role => string.Equals(user.VaiTro, role, StringComparison.OrdinalIgnoreCase)))
                .Select(user => user.Id)
                .ToHashSet();

            var permission = permissions.FirstOrDefault(item => matchedUserIds.Contains(item.UserId));
            if (permission is not null)
            {
                return MapPermissionSet(permission);
            }

            return GetDefaultPermissionsByRole(roles.FirstOrDefault() ?? "Khách hàng");
        }

        private async Task ApplyPermissionsToRoleAsync(string role, PermissionSetInput input)
        {
            var users = await _context.NguoiDung
                .Where(item => item.VaiTro == role)
                .ToListAsync();

            foreach (var user in users)
            {
                var permission = await EnsurePermissionEntityAsync(user.Id, user.VaiTro);
                ApplyPermissionSet(permission, input);
                ApplyRestrictedPermissions(user.VaiTro, permission);
            }
        }

        private async Task ApplyPermissionsToCustomerRolesAsync(PermissionSetInput input)
        {
            var users = await _context.NguoiDung
                .Where(item => item.VaiTro == "Khách hàng" || item.VaiTro == "User")
                .ToListAsync();

            foreach (var user in users)
            {
                var permission = await EnsurePermissionEntityAsync(user.Id, user.VaiTro);
                ApplyPermissionSet(permission, input);
                ApplyRestrictedPermissions(user.VaiTro, permission);
            }
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
                CanManageXe = permission.CanManageXe,
                CanViewXe = permission.CanViewXe,
                CanCreateXe = permission.CanCreateXe,
                CanEditXe = permission.CanEditXe,
                CanDeleteXe = permission.CanDeleteXe,
                CanManageLoaiXe = permission.CanManageLoaiXe,
                CanViewLoaiXe = permission.CanViewLoaiXe,
                CanCreateLoaiXe = permission.CanCreateLoaiXe,
                CanEditLoaiXe = permission.CanEditLoaiXe,
                CanDeleteLoaiXe = permission.CanDeleteLoaiXe,
                CanManageHopDong = permission.CanManageHopDong,
                CanViewHopDong = permission.CanViewHopDong,
                CanCreateHopDong = permission.CanCreateHopDong,
                CanEditHopDong = permission.CanEditHopDong,
                CanDeleteHopDong = permission.CanDeleteHopDong,
                CanPrintHopDong = permission.CanPrintHopDong,
                CanManageHoaDon = permission.CanManageHoaDon,
                CanViewHoaDon = permission.CanViewHoaDon,
                CanCreateHoaDon = permission.CanCreateHoaDon,
                CanEditHoaDon = permission.CanEditHoaDon,
                CanDeleteHoaDon = permission.CanDeleteHoaDon,
                CanPrintHoaDon = permission.CanPrintHoaDon,
                CanManageUser = permission.CanManageUser,
                CanViewUser = permission.CanViewUser,
                CanCreateUser = permission.CanCreateUser,
                CanEditUser = permission.CanEditUser,
                CanDeleteUser = permission.CanDeleteUser,
                CanViewBaoCao = permission.CanViewBaoCao,
                CanViewThongKe = permission.CanViewThongKe,
                CanExportBaoCao = permission.CanExportBaoCao,
                CanManageCart = permission.CanManageCart,
                CanViewCart = permission.CanViewCart,
                CanCheckout = permission.CanCheckout,
                CanDatCho = permission.CanDatCho,
                CanViewDatCho = permission.CanViewDatCho,
                CanManageHinhAnhXe = permission.CanManageHinhAnhXe,
                CanViewHinhAnhXe = permission.CanViewHinhAnhXe,
                CanUploadHinhAnhXe = permission.CanUploadHinhAnhXe,
                CanEditHinhAnhXe = permission.CanEditHinhAnhXe,
                CanDeleteHinhAnhXe = permission.CanDeleteHinhAnhXe
            };
        }

        private static void ApplyPermissionSet(PhanQuyen target, PermissionSetInput input)
        {
            target.CanManageXe = input.CanManageXe;
            target.CanViewXe = input.CanViewXe;
            target.CanCreateXe = input.CanCreateXe;
            target.CanEditXe = input.CanEditXe;
            target.CanDeleteXe = input.CanDeleteXe;

            target.CanManageLoaiXe = input.CanManageLoaiXe;
            target.CanViewLoaiXe = input.CanViewLoaiXe;
            target.CanCreateLoaiXe = input.CanCreateLoaiXe;
            target.CanEditLoaiXe = input.CanEditLoaiXe;
            target.CanDeleteLoaiXe = input.CanDeleteLoaiXe;

            target.CanManageHopDong = input.CanManageHopDong;
            target.CanViewHopDong = input.CanViewHopDong;
            target.CanCreateHopDong = input.CanCreateHopDong;
            target.CanEditHopDong = input.CanEditHopDong;
            target.CanDeleteHopDong = input.CanDeleteHopDong;
            target.CanPrintHopDong = input.CanPrintHopDong;

            target.CanManageHoaDon = input.CanManageHoaDon;
            target.CanViewHoaDon = input.CanViewHoaDon;
            target.CanCreateHoaDon = input.CanCreateHoaDon;
            target.CanEditHoaDon = input.CanEditHoaDon;
            target.CanDeleteHoaDon = input.CanDeleteHoaDon;
            target.CanPrintHoaDon = input.CanPrintHoaDon;

            target.CanManageUser = input.CanManageUser;
            target.CanViewUser = input.CanViewUser;
            target.CanCreateUser = input.CanCreateUser;
            target.CanEditUser = input.CanEditUser;
            target.CanDeleteUser = input.CanDeleteUser;

            target.CanViewBaoCao = input.CanViewBaoCao;
            target.CanViewThongKe = input.CanViewThongKe;
            target.CanExportBaoCao = input.CanExportBaoCao;

            target.CanManageCart = input.CanManageCart;
            target.CanViewCart = input.CanViewCart;
            target.CanCheckout = input.CanCheckout;
            target.CanDatCho = input.CanDatCho;
            target.CanViewDatCho = input.CanViewDatCho;

            target.CanManageHinhAnhXe = input.CanManageHinhAnhXe;
            target.CanViewHinhAnhXe = input.CanViewHinhAnhXe;
            target.CanUploadHinhAnhXe = input.CanUploadHinhAnhXe;
            target.CanEditHinhAnhXe = input.CanEditHinhAnhXe;
            target.CanDeleteHinhAnhXe = input.CanDeleteHinhAnhXe;
        }

        private static PermissionSetInput GetDefaultPermissionsByRole(string role)
        {
            if (string.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase))
            {
                return new PermissionSetInput
                {
                    CanManageXe = true,
                    CanViewXe = true,
                    CanCreateXe = true,
                    CanEditXe = true,
                    CanDeleteXe = true,
                    CanManageLoaiXe = true,
                    CanViewLoaiXe = true,
                    CanCreateLoaiXe = true,
                    CanEditLoaiXe = true,
                    CanDeleteLoaiXe = true,
                    CanManageHopDong = true,
                    CanViewHopDong = true,
                    CanCreateHopDong = true,
                    CanEditHopDong = true,
                    CanDeleteHopDong = true,
                    CanPrintHopDong = true,
                    CanManageHoaDon = true,
                    CanViewHoaDon = true,
                    CanCreateHoaDon = true,
                    CanEditHoaDon = true,
                    CanDeleteHoaDon = true,
                    CanPrintHoaDon = true,
                    CanManageUser = true,
                    CanViewUser = true,
                    CanCreateUser = true,
                    CanEditUser = true,
                    CanDeleteUser = true,
                    CanViewBaoCao = true,
                    CanViewThongKe = true,
                    CanExportBaoCao = true,
                    CanManageCart = true,
                    CanViewCart = true,
                    CanCheckout = true,
                    CanDatCho = true,
                    CanViewDatCho = true,
                    CanManageHinhAnhXe = true,
                    CanViewHinhAnhXe = true,
                    CanUploadHinhAnhXe = true,
                    CanEditHinhAnhXe = true,
                    CanDeleteHinhAnhXe = true
                };
            }

            if (string.Equals(role, "Staff", StringComparison.OrdinalIgnoreCase))
            {
                return new PermissionSetInput
                {
                    CanManageXe = true,
                    CanViewXe = true,
                    CanCreateXe = true,
                    CanEditXe = true,
                    CanDeleteXe = true,
                    CanManageLoaiXe = true,
                    CanViewLoaiXe = true,
                    CanCreateLoaiXe = true,
                    CanEditLoaiXe = true,
                    CanDeleteLoaiXe = false,
                    CanManageHopDong = true,
                    CanViewHopDong = true,
                    CanCreateHopDong = true,
                    CanEditHopDong = true,
                    CanDeleteHopDong = false,
                    CanPrintHopDong = true,
                    CanManageHoaDon = true,
                    CanViewHoaDon = true,
                    CanCreateHoaDon = true,
                    CanEditHoaDon = true,
                    CanDeleteHoaDon = false,
                    CanPrintHoaDon = true,
                    CanManageUser = false,
                    CanViewUser = false,
                    CanCreateUser = false,
                    CanEditUser = false,
                    CanDeleteUser = false,
                    CanViewBaoCao = true,
                    CanViewThongKe = true,
                    CanExportBaoCao = true,
                    CanManageCart = true,
                    CanViewCart = true,
                    CanCheckout = true,
                    CanDatCho = true,
                    CanViewDatCho = true,
                    CanManageHinhAnhXe = true,
                    CanViewHinhAnhXe = true,
                    CanUploadHinhAnhXe = true,
                    CanEditHinhAnhXe = true,
                    CanDeleteHinhAnhXe = true
                };
            }

            return new PermissionSetInput();
        }

        private static void ApplyRestrictedPermissions(string role, PhanQuyen permission)
        {
            if (!string.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase))
            {
                permission.CanManageUser = false;
                permission.CanViewUser = false;
                permission.CanCreateUser = false;
                permission.CanEditUser = false;
                permission.CanDeleteUser = false;
            }
        }
    }
}
