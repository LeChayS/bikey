using bikey.Common;
using bikey.Models;
using bikey.Repository;
using Microsoft.EntityFrameworkCore;

namespace bikey.Services
{
    /// <summary>
    /// Service for Invoice (HoaDon) business operations.
    /// Handles all invoice-related logic and queries.
    /// </summary>
    public class HoaDonService : IHoaDonService
    {
        private readonly BikeyDbContext _context;

        public HoaDonService(BikeyDbContext context)
        {
            _context = context;
        }

        public async Task<List<HoaDon>> GetAllAsync()
        {
            return await _context.HoaDon
                .AsNoTracking()
                .Include(h => h.HopDong)
                    .ThenInclude(hd => hd.ChiTietHopDong)
                        .ThenInclude(ct => ct.Xe)
                .Include(h => h.NguoiTao)
                .OrderByDescending(h => h.NgayThanhToan)
                .ToListAsync();
        }

        public async Task<HoaDon?> GetByIdAsync(int maHoaDon)
        {
            return await _context.HoaDon
                .AsNoTracking()
                .Include(h => h.HopDong)
                    .ThenInclude(hd => hd.ChiTietHopDong)
                        .ThenInclude(ct => ct.Xe)
                .Include(h => h.NguoiTao)
                .FirstOrDefaultAsync(h => h.MaHoaDon == maHoaDon);
        }

        public async Task<HoaDon> CreateAsync(HoaDon hoaDon)
        {
            _context.HoaDon.Add(hoaDon);
            await _context.SaveChangesAsync();
            return hoaDon;
        }

        public async Task<HoaDon> UpdateAsync(HoaDon hoaDon)
        {
            _context.HoaDon.Update(hoaDon);
            await _context.SaveChangesAsync();
            return hoaDon;
        }

        public async Task<PaginatedResult<HoaDon>> GetPaginatedAsync(
            int pageNumber = 1,
            int pageSize = 10,
            string? searchString = null,
            DateTime? fromDate = null,
            DateTime? toDate = null)
        {
            const int minPageSize = 1;
            const int maxPageSize = 100;

            pageNumber = Math.Max(minPageSize, pageNumber);
            pageSize = Math.Clamp(pageSize, minPageSize, maxPageSize);
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

            if (fromDate.HasValue)
            {
                query = query.Where(h => h.NgayThanhToan.Date >= fromDate.Value.Date);
            }

            if (toDate.HasValue)
            {
                query = query.Where(h => h.NgayThanhToan.Date <= toDate.Value.Date);
            }

            var total = await query.CountAsync();
            var items = await query
                .OrderByDescending(h => h.NgayThanhToan)
                .ThenByDescending(h => h.MaHoaDon)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PaginatedResult<HoaDon>
            {
                Items = items,
                Total = total,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }

        public async Task<HoaDonStatisticsDto> GetStatisticsAsync(DateTime? fromDate = null, DateTime? toDate = null)
        {
            var query = _context.HoaDon.AsNoTracking();

            if (fromDate.HasValue)
                query = query.Where(h => h.NgayThanhToan.Date >= fromDate.Value.Date);

            if (toDate.HasValue)
                query = query.Where(h => h.NgayThanhToan.Date <= toDate.Value.Date);

            var todayQuery = _context.HoaDon
                .AsNoTracking()
                .Where(h => h.NgayThanhToan.Date == DateTime.Today);

            return new HoaDonStatisticsDto
            {
                TotalCount = await query.CountAsync(),
                TodayCount = await todayQuery.CountAsync(),
                TotalRevenue = await query.SumAsync(h => (decimal?)h.SoTien) ?? 0m,
                TodayRevenue = await todayQuery.SumAsync(h => (decimal?)h.SoTien) ?? 0m,
                PaidCount = await query.CountAsync(h => h.TrangThai == StatusConstants.HoaDonStatus.DaThanhToan),
                UnpaidCount = await query.CountAsync(h => h.TrangThai == StatusConstants.HoaDonStatus.ChuaThanhToan)
            };
        }

        public async Task<int> GetCountTodayAsync()
        {
            return await _context.HoaDon
                .AsNoTracking()
                .CountAsync(h => h.NgayThanhToan.Date == DateTime.Today);
        }

        public async Task<decimal> GetTotalRevenueAsync(DateTime? fromDate = null, DateTime? toDate = null)
        {
            var query = _context.HoaDon.AsNoTracking();

            if (fromDate.HasValue)
                query = query.Where(h => h.NgayThanhToan.Date >= fromDate.Value.Date);

            if (toDate.HasValue)
                query = query.Where(h => h.NgayThanhToan.Date <= toDate.Value.Date);

            return await query.SumAsync(h => (decimal?)h.SoTien) ?? 0m;
        }
    }
}
