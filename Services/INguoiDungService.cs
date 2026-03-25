using bikey.Models;
using bikey.Repository;
using bikey.ViewModels;

namespace bikey.Services
{
    public interface INguoiDungService
    {
        Task<List<NguoiDung>> GetAllAsync();
        Task<NguoiDung?> GetByIdAsync(int id);
        Task<NguoiDung> CreateAsync(string hoTen, string email, string soDienThoai, string matKhau, string vaiTro);
        Task UpdateAsync(int id, string hoTen, string email, string soDienThoai, string? diaChi, string vaiTro, bool isActive, string? matKhauMoi = null);
        Task DeleteAsync(int id);
        Task<bool> EmailExistsAsync(string email, int? excludeUserId = null);
        Task<bool> PhoneExistsAsync(string phone, int? excludeUserId = null);
        PhanQuyen CreatePermissionEntity(int userId, PermissionSetInput permissions);
        Task<PhanQuyen> EnsurePermissionEntityAsync(int userId, string role, BikeyDbContext context);
        PermissionSetInput MapPermissionSet(PhanQuyen permission);
        void ApplyPermissionSet(PhanQuyen target, PermissionSetInput input);
        void EnforceViewPermissionLogic(PhanQuyen permission);
        PermissionSetInput GetDefaultPermissionsByRole(string role);
        Task CreateDefaultPermissionsAsync(int userId, string role);
        Task UpdatePermissionsAsync(int userId, string role, PermissionSetInput permissions);
    }
}
