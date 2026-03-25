using bikey.Common;
using bikey.Models;
using bikey.Repository;
using Microsoft.EntityFrameworkCore;
using static bikey.Models.DatCho.DatChoTrangThai;

namespace bikey.Services
{
    /// <summary>
    /// Service for Contract (HopDong) business operations.
    /// Handles all contract-related logic and calculations.
    /// </summary>
    public class HopDongService : IHopDongService
    {
        private const decimal DepositRate = 0.13m; // 13% of daily rental price
        private const decimal TimeFactor = 0.4m; // Time factor for calculations

        private readonly BikeyDbContext _context;

        public HopDongService(BikeyDbContext context)
        {
            _context = context;
        }

        public async Task<List<HopDong>> GetAllAsync()
        {
            return await _context.HopDong
                .AsNoTracking()
                .Include(h => h.ChiTietHopDong)
                    .ThenInclude(ct => ct.Xe)
                .Include(h => h.HoaDon)
                .Include(h => h.KhachHang)
                .OrderByDescending(h => h.NgayTao)
                .ToListAsync();
        }

        public async Task<HopDong?> GetByIdAsync(int maHopDong)
        {
            return await _context.HopDong
                .AsNoTracking()
                .Include(h => h.ChiTietHopDong)
                    .ThenInclude(ct => ct.Xe)
                .Include(h => h.HoaDon)
                .Include(h => h.KhachHang)
                .FirstOrDefaultAsync(h => h.MaHopDong == maHopDong);
        }

        public async Task<HopDong> CreateAsync(HopDong hopDong)
        {
            _context.HopDong.Add(hopDong);
            await _context.SaveChangesAsync();
            return hopDong;
        }

        public async Task<HopDong> UpdateAsync(HopDong hopDong)
        {
            _context.HopDong.Update(hopDong);
            await _context.SaveChangesAsync();
            return hopDong;
        }

        public async Task<PaginatedResult<HopDong>> GetPaginatedAsync(
            int pageNumber = 1,
            int pageSize = 10,
            string? trangThai = null,
            string? tuKhoa = null)
        {
            const int minPageSize = 1;
            const int maxPageSize = 100;

            pageNumber = Math.Max(minPageSize, pageNumber);
            pageSize = Math.Clamp(pageSize, minPageSize, maxPageSize);

            var query = _context.HopDong
                .AsNoTracking()
                .Include(h => h.ChiTietHopDong)
                    .ThenInclude(ct => ct.Xe)
                .Include(h => h.HoaDon)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(trangThai))
            {
                query = query.Where(h => h.TrangThai == trangThai);
            }

            if (!string.IsNullOrWhiteSpace(tuKhoa))
            {
                var keyword = tuKhoa.Trim();
                query = query.Where(h =>
                    (h.HoTenKhach != null && h.HoTenKhach.Contains(keyword))
                    || h.ChiTietHopDong.Any(ct =>
                        ct.Xe != null
                        && (
                            (ct.Xe.TenXe != null && ct.Xe.TenXe.Contains(keyword))
                            || (ct.Xe.BienSoXe != null && ct.Xe.BienSoXe.Contains(keyword)))));
            }

            var total = await query.CountAsync();
            var items = await query
                .OrderByDescending(h => h.NgayTao)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PaginatedResult<HopDong>
            {
                Items = items,
                Total = total,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }

        public async Task<List<HopDong>> GetPendingAsync()
        {
            return await _context.HopDong
                .AsNoTracking()
                .Include(h => h.ChiTietHopDong)
                    .ThenInclude(ct => ct.Xe)
                .Include(h => h.KhachHang)
                .Where(h => h.TrangThai == StatusConstants.HopDongStatus.DangThue)
                .OrderByDescending(h => h.NgayTao)
                .ToListAsync();
        }

        public async Task<HopDongStatisticsDto> GetStatisticsAsync()
        {
            var allContracts = await _context.HopDong.AsNoTracking().ToListAsync();

            return new HopDongStatisticsDto
            {
                TotalPending = await GetCountPendingBookingsAsync(),
                TotalNewToday = await GetCountNewBookingsAsync(),
                TotalActive = await _context.HopDong
                    .AsNoTracking()
                    .CountAsync(h => h.TrangThai == StatusConstants.HopDongStatus.DangThue),
                TotalCompleted = await _context.HopDong
                    .AsNoTracking()
                    .CountAsync(h => h.TrangThai == StatusConstants.HopDongStatus.HoanThanh)
            };
        }

        public decimal CalculateDeposit(decimal rentalPrice)
        {
            return rentalPrice * DepositRate;
        }

        public HopDongCalculationDto CalculateRentalPrice(
            DateTime startDate,
            DateTime endDate,
            decimal dailyRate)
        {
            var days = Math.Max(1, (int)(endDate - startDate).TotalDays);
            var totalPrice = days * dailyRate;
            var deposit = CalculateDeposit(dailyRate);

            return new HopDongCalculationDto
            {
                Days = days,
                DailyRate = dailyRate,
                TotalPrice = totalPrice,
                Deposit = deposit
            };
        }

        public async Task<int> GetCountPendingBookingsAsync()
        {
            return await _context.DatCho
                .AsNoTracking()
                .CountAsync(dc => dc.TrangThai == ChoXacNhan);
        }

        public async Task<int> GetCountNewBookingsAsync()
        {
            return await _context.DatCho
                .AsNoTracking()
                .CountAsync(dc =>
                    dc.TrangThai == ChoXacNhan
                    && dc.NgayDat.Date == DateTime.Today);
        }

        public async Task<List<DatCho>> GetDonChoXuLyAsync(string? searchString, DateTime? tuNgay, DateTime? denNgay)
        {
            var query = _context.DatCho
                .AsNoTracking()
                .Include(item => item.Xe)
                .Where(item => item.TrangThai == ChoXacNhan)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchString))
            {
                var keyword = searchString.Trim();
                query = query.Where(item =>
                    (item.HoTen != null && item.HoTen.Contains(keyword)) ||
                    (item.SoDienThoai != null && item.SoDienThoai.Contains(keyword)) ||
                    (item.Email != null && item.Email.Contains(keyword)));
            }

            if (tuNgay.HasValue)
                query = query.Where(item => item.NgayDat.Date >= tuNgay.Value.Date);

            if (denNgay.HasValue)
                query = query.Where(item => item.NgayDat.Date <= denNgay.Value.Date);

            return await query.OrderByDescending(item => item.NgayDat).ToListAsync();
        }

        public async Task XuLyDonAsync(int datChoId, int? maNguoiTao)
        {
            var strategy = _context.Database.CreateExecutionStrategy();
            await strategy.ExecuteAsync(async () =>
            {
                await using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    var datCho = await _context.DatCho
                        .Include(item => item.Xe)
                        .FirstOrDefaultAsync(item => item.MaDatCho == datChoId);

                    if (datCho is null)
                        throw new KeyNotFoundException("Không tìm thấy đơn đặt chỗ.");

                    if (!IsChoStaffQueue(datCho.TrangThai))
                        throw new InvalidOperationException("Đơn này đã được xử lý trước đó.");

                    if (datCho.Xe is null)
                        throw new InvalidOperationException("Không tìm thấy thông tin xe cho đơn đặt chỗ này.");

                    if (datCho.Xe.TrangThai != StatusConstants.XeStatus.SanSang)
                        throw new InvalidOperationException("Xe không còn sẵn sàng để tạo hợp đồng.");

                    var cccd = NormalizeDigits(datCho.SoCanCuoc);
                    if (cccd is null || cccd.Length != 12)
                        throw new InvalidOperationException("Đơn chưa có CCCD hợp lệ (12 số), không thể tạo hợp đồng.");

                    var soDienThoai = NormalizeDigits(datCho.SoDienThoai);
                    if (soDienThoai is null || (soDienThoai.Length != 10 && soDienThoai.Length != 11))
                        throw new InvalidOperationException("Đơn chưa có số điện thoại hợp lệ, không thể tạo hợp đồng.");

                    var xe = datCho.Xe;
                    var rentalDays = Math.Max(1, (datCho.NgayTraXe - datCho.NgayNhanXe).Days);
                    var rentalTotal = xe.GiaThue * rentalDays;
                    var calculation = CalculateRentalPrice(datCho.NgayNhanXe, datCho.NgayTraXe, xe.GiaThue);
                    var deposit = calculation.Deposit;

                    var maKhachHangHopLe = await ResolveExistingUserIdAsync(datCho.MaUser);
                    var maNguoiTaoHopLe = await ResolveExistingUserIdAsync(maNguoiTao);

                    var hopDong = new HopDong
                    {
                        MaDatCho = datCho.MaDatCho,
                        MaKhachHang = maKhachHangHopLe,
                        HoTenKhach = string.IsNullOrWhiteSpace(datCho.HoTen) ? "Khách vãng lai" : datCho.HoTen.Trim(),
                        SoDienThoai = soDienThoai,
                        SoCCCD = cccd,
                        DiaChi = datCho.DiaChi,
                        NgayNhanXe = datCho.NgayNhanXe,
                        NgayTraXeDuKien = datCho.NgayTraXe,
                        TienCoc = deposit,
                        TongTien = rentalTotal,
                        GhiChu = datCho.GhiChu,
                        TrangThai = StatusConstants.HopDongStatus.DangThue,
                        NgayTao = DateTime.Now,
                        MaNguoiTao = maNguoiTaoHopLe
                    };

                    hopDong.ChiTietHopDong.Add(new ChiTietHopDong
                    {
                        MaXe = xe.MaXe,
                        GiaThueNgay = xe.GiaThue,
                        NgayNhanXe = datCho.NgayNhanXe,
                        NgayTraXeDuKien = datCho.NgayTraXe,
                        SoNgayThue = rentalDays,
                        ThanhTien = rentalTotal,
                        TrangThaiXe = StatusConstants.XeStatus.DangThue,
                        NgayTao = DateTime.Now,
                        HopDong = hopDong
                    });

                    _context.HopDong.Add(hopDong);
                    xe.TrangThai = StatusConstants.XeStatus.DangThue;
                    datCho.TrangThai = DaXuLy;

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            });
        }

        public async Task HuyDonAsync(int datChoId)
        {
            var datCho = await _context.DatCho.FirstOrDefaultAsync(item => item.MaDatCho == datChoId);
            if (datCho is null)
                throw new KeyNotFoundException("Không tìm thấy đơn đặt chỗ.");

            if (!IsChoStaffQueue(datCho.TrangThai))
                throw new InvalidOperationException("Đơn này đã được xử lý trước đó.");

            datCho.TrangThai = Huy;
            await _context.SaveChangesAsync();
        }

        public async Task<HopDong?> GetChiTietAsync(int maHopDong)
        {
            return await _context.HopDong
                .Include(h => h.ChiTietHopDong)
                    .ThenInclude(ct => ct.Xe)
                .Include(h => h.HoaDon)
                .FirstOrDefaultAsync(h => h.MaHopDong == maHopDong);
        }

        public async Task TraXeAsync(
            int maHopDong,
            DateTime ngayTraThucTe,
            decimal phuPhi,
            string tinhTrangXe,
            string? loaiThietHai,
            string? moTaThietHai,
            decimal chiPhiThietHai,
            string? ghiChu,
            int maNguoiTao)
        {
            var strategy = _context.Database.CreateExecutionStrategy();
            await strategy.ExecuteAsync(async () =>
            {
                await using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    var hopDong = await _context.HopDong
                        .Include(h => h.ChiTietHopDong)
                            .ThenInclude(ct => ct.Xe)
                        .Include(h => h.HoaDon)
                        .FirstOrDefaultAsync(h => h.MaHopDong == maHopDong);

                    if (hopDong is null)
                        throw new KeyNotFoundException("Không tìm thấy hợp đồng.");

                    if (hopDong.TrangThai != StatusConstants.HopDongStatus.DangThue)
                        throw new InvalidOperationException("Hợp đồng này không còn ở trạng thái đang thuê.");

                    if (hopDong.ChiTietHopDong == null || !hopDong.ChiTietHopDong.Any())
                        throw new InvalidOperationException("Hợp đồng này không có thông tin xe thuê.");

                    var laCoSuCo = string.Equals(tinhTrangXe, "Có sự cố", StringComparison.OrdinalIgnoreCase);

                    var tienCocDuKien = hopDong.ChiTietHopDong.Sum(ct =>
                        CalculateRentalPrice(ct.NgayNhanXe, ct.NgayTraXeDuKien, ct.GiaThueNgay).Deposit);

                    foreach (var ct in hopDong.ChiTietHopDong)
                    {
                        var soNgayThueThucTe = Math.Max(1, (ngayTraThucTe.Date - ct.NgayNhanXe.Date).Days);
                        ct.NgayTraXeThucTe = ngayTraThucTe;
                        ct.SoNgayThue = soNgayThueThucTe;
                        ct.ThanhTien = ct.GiaThueNgay * soNgayThueThucTe;
                        ct.TrangThaiXe = "Đã trả";
                        ct.TinhTrangTraXe = tinhTrangXe;
                        ct.PhiDenBu = laCoSuCo ? chiPhiThietHai : 0;
                        ct.MoTaThietHai = laCoSuCo ? moTaThietHai : null;
                        ct.GhiChu = ghiChu;

                        if (ct.Xe != null)
                        {
                            if (!laCoSuCo)
                                ct.Xe.TrangThai = StatusConstants.XeStatus.SanSang;
                            else if (string.Equals(loaiThietHai, "Mất xe", StringComparison.OrdinalIgnoreCase))
                                ct.Xe.TrangThai = StatusConstants.XeStatus.Mat;
                            else
                                ct.Xe.TrangThai = StatusConstants.XeStatus.HuHong;
                        }
                    }

                    hopDong.NgayTraXeThucTe = ngayTraThucTe;
                    hopDong.PhuPhi = phuPhi;
                    hopDong.TongTien = hopDong.ChiTietHopDong.Sum(ct => ct.ThanhTien) + phuPhi + hopDong.ChiTietHopDong.Sum(ct => ct.PhiDenBu);
                    hopDong.TienCoc = tienCocDuKien;
                    hopDong.GhiChu = ghiChu;
                    hopDong.TrangThai = StatusConstants.HopDongStatus.HoanThanh;

                    if (hopDong.MaDatCho.HasValue)
                    {
                        var datCho = await _context.DatCho.FirstOrDefaultAsync(d => d.MaDatCho == hopDong.MaDatCho.Value);
                        if (datCho != null)
                            datCho.TrangThai = DatCho.DatChoTrangThai.HoanThanh;
                    }

                    var tongPhiDenBu = hopDong.ChiTietHopDong.Sum(ct => ct.PhiDenBu);
                    var tongTienThueXe = hopDong.ChiTietHopDong.Sum(ct => ct.ThanhTien);
                    var soTienHoaDon = Math.Max(0m, tongTienThueXe + phuPhi + tongPhiDenBu);

                    if (hopDong.HoaDon is null)
                    {
                        hopDong.HoaDon = new HoaDon
                        {
                            MaHopDong = hopDong.MaHopDong,
                            NgayThanhToan = DateTime.Now,
                            SoTien = soTienHoaDon,
                            TrangThai = StatusConstants.HoaDonStatus.DaThanhToan,
                            GhiChu = ghiChu,
                            NgayTao = DateTime.Now,
                            MaNguoiTao = maNguoiTao
                        };
                    }
                    else
                    {
                        hopDong.HoaDon.NgayThanhToan = DateTime.Now;
                        hopDong.HoaDon.SoTien = soTienHoaDon;
                        hopDong.HoaDon.TrangThai = StatusConstants.HoaDonStatus.DaThanhToan;
                        hopDong.HoaDon.GhiChu = ghiChu;
                        hopDong.HoaDon.NgayTao = DateTime.Now;
                        hopDong.HoaDon.MaNguoiTao = maNguoiTao;
                    }

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            });
        }

        public async Task<List<DatCho>> TimPhieuDatChoAsync(string soDienThoai)
        {
            var phone = soDienThoai.Trim();
            return await _context.DatCho
                .AsNoTracking()
                .Include(item => item.Xe)
                .Where(item => item.SoDienThoai != null && item.SoDienThoai.Contains(phone))
                .OrderByDescending(item => item.NgayDat)
                .ToListAsync();
        }

        public async Task<List<HopDong>> GetLichSuKhachHangAsync(string soDienThoai)
        {
            var phone = soDienThoai.Trim();
            return await _context.HopDong
                .AsNoTracking()
                .Include(h => h.ChiTietHopDong).ThenInclude(ct => ct.Xe)
                .Include(h => h.KhachHang)
                .Where(h => h.SoDienThoai == phone)
                .OrderByDescending(h => h.NgayTao)
                .ToListAsync();
        }

        private static string? NormalizeDigits(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;
            return new string(value.Where(char.IsDigit).ToArray());
        }

        private async Task<int?> ResolveExistingUserIdAsync(int? userId)
        {
            if (!userId.HasValue)
                return null;
            var exists = await _context.NguoiDung.AnyAsync(item => item.Id == userId.Value);
            return exists ? userId : null;
        }

        private static bool IsChoStaffQueue(string? trangThai)
        {
            return string.Equals(trangThai, ChoXacNhan, StringComparison.OrdinalIgnoreCase);
        }
    }
}
