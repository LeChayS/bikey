using System;
using System.Threading.Tasks;
using bikey.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace bikey.Controllers
{
    [Authorize(Roles = "Admin,Staff")]
    public class HoaDonController : Controller
    {
        private readonly BikeyDbContext _context;

        public HoaDonController(BikeyDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index(
            string? searchString,
            DateTime? tuNgay,
            DateTime? denNgay,
            int page = 1,
            int pageSize = 10)
        {
            page = page < 1 ? 1 : page;
            pageSize = pageSize < 1 ? 10 : pageSize;
            searchString = searchString?.Trim();

            var query = _context.HoaDon
                .AsNoTracking()
                .Include(h => h.HopDong)
                    .ThenInclude(hd => hd.ChiTietHopDong)
                        .ThenInclude(ct => ct.Xe)
                .Include(h => h.NguoiTao)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchString))
            {
                var keyword = searchString;
                query = query.Where(h =>
                    h.HopDong != null &&
                    (
                        (h.HopDong.HoTenKhach != null && h.HopDong.HoTenKhach.Contains(keyword)) ||
                        (h.HopDong.SoDienThoai != null && h.HopDong.SoDienThoai.Contains(keyword))
                    ));
            }

            if (tuNgay.HasValue)
            {
                query = query.Where(h => h.NgayThanhToan.Date >= tuNgay.Value.Date);
            }

            if (denNgay.HasValue)
            {
                query = query.Where(h => h.NgayThanhToan.Date <= denNgay.Value.Date);
            }

            var totalItems = await query.CountAsync();
            var tongDoanhThu = (await query.SumAsync(h => (decimal?)h.SoTien)) ?? 0m;

            // "Hóa đơn hôm nay" hiển thị theo toàn hệ thống (không áp filter tìm kiếm).
            var soHoaDonHomNay = await _context.HoaDon
                .AsNoTracking()
                .CountAsync(h => h.NgayThanhToan.Date == DateTime.Today);

            var hoaDons = await query
                .OrderByDescending(h => h.NgayThanhToan)
                .ThenByDescending(h => h.MaHoaDon)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.TotalItems = totalItems;
            ViewBag.TongDoanhThu = tongDoanhThu;
            ViewBag.SoHoaDonHomNay = soHoaDonHomNay;

            ViewBag.SearchString = searchString;
            ViewBag.TuNgay = tuNgay;
            ViewBag.DenNgay = denNgay;
            ViewBag.PageSize = pageSize;
            ViewBag.CurrentPage = page;

            return View(hoaDons);
        }

        [HttpGet]
        public async Task<IActionResult> ChiTiet(int id)
        {
            if (id <= 0) return NotFound();

            var hoaDon = await _context.HoaDon
                .AsNoTracking()
                .Include(h => h.HopDong)
                    .ThenInclude(hd => hd.ChiTietHopDong)
                        .ThenInclude(ct => ct.Xe)
                .Include(h => h.NguoiTao)
                .FirstOrDefaultAsync(h => h.MaHoaDon == id);

            return hoaDon is null ? NotFound() : View(hoaDon);
        }

        [HttpGet]
        public async Task<IActionResult> InHoaDon(int id)
        {
            if (id <= 0) return NotFound();

            var hoaDon = await _context.HoaDon
                .AsNoTracking()
                .Include(h => h.HopDong)
                    .ThenInclude(hd => hd.ChiTietHopDong)
                        .ThenInclude(ct => ct.Xe)
                .Include(h => h.NguoiTao)
                .FirstOrDefaultAsync(h => h.MaHoaDon == id);

            if (hoaDon is null) return NotFound();

            ViewBag.AutoPrint = true;
            return View("ChiTiet", hoaDon);
        }
    }
}
