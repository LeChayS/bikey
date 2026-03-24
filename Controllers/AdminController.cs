using bikey.Repository;
using bikey.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace bikey.Controllers
{
    [Authorize(Roles = "Admin,Staff")]
    public class AdminController : Controller
    {
        private const string TrangHoanThanh = "Hoàn thành";
        private readonly BikeyDbContext _context;

        public AdminController(BikeyDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var vm = await BuildDashboardAsync();
            return View(vm);
        }

        private async Task<AdminDashboardViewModel> BuildDashboardAsync()
        {
            var today = DateTime.Today;
            var startThisMonth = new DateTime(today.Year, today.Month, 1);
            var startLastMonth = startThisMonth.AddMonths(-1);

            var hdHoanThanhThangNay = await _context.HopDong.AsNoTracking()
                .CountAsync(h => h.TrangThai == TrangHoanThanh
                    && h.NgayTraXeThucTe.HasValue
                    && h.NgayTraXeThucTe.Value.Date >= startThisMonth);

            var hdHoanThanhThangTruoc = await _context.HopDong.AsNoTracking()
                .CountAsync(h => h.TrangThai == TrangHoanThanh
                    && h.NgayTraXeThucTe.HasValue
                    && h.NgayTraXeThucTe.Value.Date >= startLastMonth
                    && h.NgayTraXeThucTe.Value.Date < startThisMonth);

            var phanTramHd = hdHoanThanhThangTruoc == 0
                ? (hdHoanThanhThangNay > 0 ? 100m : 0m)
                : Math.Round((hdHoanThanhThangNay - hdHoanThanhThangTruoc) * 100m / hdHoanThanhThangTruoc, 1);

            async Task<decimal> SumDoanhThuNgay(DateTime ngay) =>
                await _context.HopDong.AsNoTracking()
                    .Where(h => h.TrangThai == TrangHoanThanh
                        && h.NgayTraXeThucTe.HasValue
                        && h.NgayTraXeThucTe.Value.Date == ngay)
                    .SumAsync(h => (decimal?)h.TongTien) ?? 0m;

            var doanhThuHomNay = await SumDoanhThuNgay(today);
            var doanhThuHomTruoc = await SumDoanhThuNgay(today.AddDays(-1));
            var phanTramDt = doanhThuHomTruoc == 0m
                ? (doanhThuHomNay > 0 ? 100m : 0m)
                : Math.Round((doanhThuHomNay - doanhThuHomTruoc) * 100m / doanhThuHomTruoc, 1);

            var xeDangThue = await _context.Xe.AsNoTracking()
                .CountAsync(x => x.TrangThai == "Đang thuê");

            var hdDangThue = await _context.HopDong.AsNoTracking()
                .CountAsync(h => h.TrangThai == "Đang thuê");

            var khachMoi = await _context.NguoiDung.AsNoTracking()
                .CountAsync(u => u.VaiTro == "User" && u.NgayTao.Date == today);

            var tongXe = await _context.Xe.AsNoTracking()
                .CountAsync(x => x.TrangThai != "Đã xóa");

            var topXe = await BuildTopXeAllTimeAsync();
            var recent = await BuildRecentHopDongAsync();

            return new AdminDashboardViewModel
            {
                HopDongHoanThanh = hdHoanThanhThangNay,
                PhanTramHopDongVsKyTruoc = phanTramHd,
                DoanhThuHomNay = doanhThuHomNay,
                PhanTramDoanhThuVsHomTruoc = phanTramDt,
                XeDangChoThue = xeDangThue,
                HopDongHoatDong = hdDangThue,
                KhachHangMoiHomNay = khachMoi,
                TongSoXe = tongXe,
                TopXeThueNhieu = topXe,
                HopDongGanDay = recent
            };
        }

        private async Task<List<TopXeThueNhieuItem>> BuildTopXeAllTimeAsync()
        {
            var rows = await _context.ChiTietHopDong.AsNoTracking()
                .Include(c => c.Xe)
                .Include(c => c.HopDong)
                .Where(c => c.HopDong.TrangThai == TrangHoanThanh
                    && c.Xe != null
                    && c.Xe.TrangThai != "Đã xóa")
                .ToListAsync();

            return rows
                .GroupBy(c => c.MaXe)
                .Select(g =>
                {
                    var xe = g.First().Xe!;
                    return new TopXeThueNhieuItem
                    {
                        TenXe = xe.TenXe,
                        BienSo = xe.BienSoXe,
                        GiaThueNgay = xe.GiaThue,
                        TrangThaiXe = xe.TrangThai,
                        SoLanThue = g.Count(),
                        DoanhThu = g.Sum(x => x.ThanhTien)
                    };
                })
                .OrderByDescending(x => x.SoLanThue)
                .Take(5)
                .ToList();
        }

        private async Task<List<RecentHopDongRow>> BuildRecentHopDongAsync()
        {
            var list = await _context.HopDong.AsNoTracking()
                .Include(h => h.KhachHang)
                .Include(h => h.ChiTietHopDong).ThenInclude(c => c.Xe)
                .OrderByDescending(h => h.NgayTao)
                .Take(8)
                .ToListAsync();

            return list.Select(h =>
            {
                var tenKhach = h.HoTenKhach ?? h.KhachHang?.Ten ?? "—";
                var firstXe = h.ChiTietHopDong?.FirstOrDefault()?.Xe;
                var tenXe = firstXe != null ? $"{firstXe.TenXe} ({firstXe.BienSoXe})" : "—";
                var ngay = h.TrangThai == TrangHoanThanh && h.NgayTraXeThucTe.HasValue
                    ? h.NgayTraXeThucTe
                    : h.NgayTao;

                return new RecentHopDongRow
                {
                    MaHopDong = h.MaHopDong,
                    TenKhach = tenKhach,
                    TenXe = tenXe,
                    TrangThai = h.TrangThai,
                    NgayHienThi = ngay,
                    TongTien = h.TongTien
                };
            }).ToList();
        }
    }
}
