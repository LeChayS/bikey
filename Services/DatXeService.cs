using System.Security.Claims;
using bikey.Common;
using bikey.Models;
using bikey.Repository;
using bikey.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace bikey.Services
{
    public class DatXeService : IDatXeService
    {
        private readonly BikeyDbContext _context;

        public DatXeService(BikeyDbContext context)
        {
            _context = context;
        }

        public DatXeViewModel BuildDatXeViewModel(Xe xe, ClaimsPrincipal user)
        {
            var model = new DatXeViewModel
            {
                MaXe = xe.MaXe,
                HoTenKhachHang = user.FindFirstValue(ClaimTypes.Name) ?? string.Empty,
                SoDienThoai = user.FindFirstValue(ClaimTypes.MobilePhone) ?? string.Empty,
                DiaChi = user.FindFirstValue(ClaimTypes.StreetAddress) ?? string.Empty,
                Email = user.FindFirstValue(ClaimTypes.Email) ?? string.Empty
            };

            PopulateVehicleInfo(model, xe);
            return model;
        }

        public void PopulateVehicleInfo(DatXeViewModel model, Xe xe)
        {
            model.TenXe = xe.TenXe;
            model.BienSoXe = xe.BienSoXe;
            model.HangXe = xe.HangXe;
            model.DongXe = xe.DongXe;
            model.TrangThaiXe = xe.TrangThai;
            model.GiaThueNgay = xe.GiaThue;
            model.GiaTriXe = xe.GiaTriXe;
            model.TenLoaiXe = xe.LoaiXe?.TenLoaiXe ?? "—";
            model.HinhAnhXe = StringHelpers.NormalizeImageUrl(xe.HinhAnhHienThi);
            model.TongTienDuKien = model.SoNgayThue * xe.GiaThue;
        }

        public async Task<Xe?> GetXeForBookingAsync(int maXe)
        {
            return await _context.Xe
                .AsNoTracking()
                .Include(item => item.LoaiXe)
                .Include(item => item.HinhAnhXes)
                .FirstOrDefaultAsync(item => item.MaXe == maXe);
        }

        public async Task<DatCho> CreateDatChoAsync(DatCho datCho)
        {
            _context.DatCho.Add(datCho);
            await _context.SaveChangesAsync();
            return datCho;
        }

        public async Task<DatCho?> GetDatChoByIdAsync(int id)
        {
            return await _context.DatCho
                .AsNoTracking()
                .Include(item => item.Xe)
                    .ThenInclude(x => x!.LoaiXe)
                .Include(item => item.Xe)
                    .ThenInclude(x => x!.HinhAnhXes)
                .FirstOrDefaultAsync(item => item.MaDatCho == id);
        }
    }
}
