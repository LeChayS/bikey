using bikey.Models;
using bikey.Repository;
using bikey.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace bikey.Services
{
    public class NguoiDungService : INguoiDungService
    {
        private readonly BikeyDbContext _context;

        public NguoiDungService(BikeyDbContext context)
        {
            _context = context;
        }

        public async Task<List<NguoiDung>> GetAllAsync()
        {
            return await _context.NguoiDung
                .AsNoTracking()
                .OrderByDescending(item => item.NgayTao)
                .ToListAsync();
        }

        public async Task<NguoiDung?> GetByIdAsync(int id)
        {
            return await _context.NguoiDung
                .AsNoTracking()
                .FirstOrDefaultAsync(item => item.Id == id);
        }

        public async Task<NguoiDung> CreateAsync(string hoTen, string email, string soDienThoai, string matKhau, string vaiTro)
        {
            var user = new NguoiDung
            {
                Ten = hoTen.Trim(),
                Email = email,
                SoDienThoai = soDienThoai.Trim(),
                MatKhau = matKhau,
                VaiTro = vaiTro,
                IsActive = true,
                NgayTao = DateTime.Now
            };

            _context.NguoiDung.Add(user);
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task UpdateAsync(int id, string hoTen, string email, string soDienThoai, string? diaChi, string vaiTro, bool isActive, string? matKhauMoi = null)
        {
            var user = await _context.NguoiDung.FirstOrDefaultAsync(item => item.Id == id);
            if (user is null)
            {
                throw new KeyNotFoundException($"User with ID {id} not found.");
            }

            user.Ten = hoTen.Trim();
            user.Email = email;
            user.SoDienThoai = soDienThoai.Trim();
            user.DiaChi = string.IsNullOrWhiteSpace(diaChi) ? null : diaChi.Trim();
            user.VaiTro = vaiTro;
            user.IsActive = isActive;

            if (!string.IsNullOrWhiteSpace(matKhauMoi))
            {
                user.MatKhau = matKhauMoi;
            }

            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var user = await _context.NguoiDung.FirstOrDefaultAsync(item => item.Id == id);
            if (user is null)
            {
                throw new KeyNotFoundException($"User with ID {id} not found.");
            }

            var permission = await _context.PhanQuyen.FirstOrDefaultAsync(item => item.UserId == user.Id);
            if (permission is not null)
            {
                _context.PhanQuyen.Remove(permission);
            }

            _context.NguoiDung.Remove(user);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> EmailExistsAsync(string email, int? excludeUserId = null)
        {
            var normalizedEmail = email.Trim().ToLowerInvariant();
            var query = _context.NguoiDung.Where(item => item.Email != null && item.Email.ToLower() == normalizedEmail);

            if (excludeUserId.HasValue)
            {
                query = query.Where(item => item.Id != excludeUserId.Value);
            }

            return await query.AnyAsync();
        }

        public async Task<bool> PhoneExistsAsync(string phone, int? excludeUserId = null)
        {
            var query = _context.NguoiDung.Where(item => item.SoDienThoai == phone);

            if (excludeUserId.HasValue)
            {
                query = query.Where(item => item.Id != excludeUserId.Value);
            }

            return await query.AnyAsync();
        }

        public PhanQuyen CreatePermissionEntity(int userId, PermissionSetInput input)
        {
            var permission = new PhanQuyen { UserId = userId };
            ApplyPermissionSet(permission, input);
            return permission;
        }

        public async Task<PhanQuyen> EnsurePermissionEntityAsync(int userId, string role, BikeyDbContext context)
        {
            var permission = await context.PhanQuyen.FirstOrDefaultAsync(item => item.UserId == userId);
            if (permission is not null)
            {
                return permission;
            }

            permission = CreatePermissionEntity(userId, GetDefaultPermissionsByRole(role));
            context.PhanQuyen.Add(permission);
            return permission;
        }

        public PermissionSetInput MapPermissionSet(PhanQuyen permission)
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

        public void ApplyPermissionSet(PhanQuyen target, PermissionSetInput input)
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

        public void EnforceViewPermissionLogic(PhanQuyen permission)
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

        public async Task CreateDefaultPermissionsAsync(int userId, string role)
        {
            var permission = CreatePermissionEntity(userId, GetDefaultPermissionsByRole(role));
            _context.PhanQuyen.Add(permission);
            await _context.SaveChangesAsync();
        }

        public async Task UpdatePermissionsAsync(int userId, string role, PermissionSetInput permissions)
        {
            var permission = await EnsurePermissionEntityAsync(userId, role, _context);
            ApplyPermissionSet(permission, permissions);
            EnforceViewPermissionLogic(permission);
            await _context.SaveChangesAsync();
        }

        public PermissionSetInput GetDefaultPermissionsByRole(string role)
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
