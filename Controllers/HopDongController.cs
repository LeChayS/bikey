using System.Security.Claims;
using bikey.Models;
using bikey.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static bikey.Models.DatCho.DatChoTrangThai;

namespace bikey.Controllers
{
    [Authorize(Roles = "Admin,Staff")]
    public class HopDongController : Controller
    {
        private const decimal DepositRate = 0.13m;
        private const decimal TimeFactor = 0.4m;
        private readonly BikeyDbContext _context;

        public HopDongController(BikeyDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string? trangThai, string? tuKhoa, int page = 1)
        {
            const int pageSize = 10;
            page = page < 1 ? 1 : page;

            var hopDongQuery = _context.HopDong
                .AsNoTracking()
                .Include(item => item.ChiTietHopDong)
                    .ThenInclude(item => item.Xe)
                .Include(item => item.HoaDon)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(trangThai))
            {
                hopDongQuery = hopDongQuery.Where(item => item.TrangThai == trangThai);
            }

            if (!string.IsNullOrWhiteSpace(tuKhoa))
            {
                var keyword = tuKhoa.Trim();
                hopDongQuery = hopDongQuery.Where(item =>
                    (item.HoTenKhach != null && item.HoTenKhach.Contains(keyword))
                    || item.ChiTietHopDong.Any(ct =>
                        ct.Xe != null
                        && (
                            (ct.Xe.TenXe != null && ct.Xe.TenXe.Contains(keyword))
                            || (ct.Xe.BienSoXe != null && ct.Xe.BienSoXe.Contains(keyword)))));
            }

            var totalItems = await hopDongQuery.CountAsync();
            var hopDongList = await hopDongQuery
                .OrderByDescending(item => item.NgayTao)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var allContracts = _context.HopDong.AsNoTracking();
            ViewBag.TotalItems = totalItems;
            ViewBag.PageSize = pageSize;
            ViewBag.CurrentPage = page;
            ViewBag.TrangThai = trangThai;
            ViewBag.TuKhoa = tuKhoa;
            ViewBag.DonChoXuLy = await CountDatChoTrongHangDoiAsync();
            ViewBag.DonChoXuLyMoi = await _context.DatCho.CountAsync(item =>
                (item.TrangThai == ChoXacNhan || item.TrangThai == DangGiuCho)
                && item.NgayDat.Date == DateTime.Today);
            ViewBag.TongDangThue = await allContracts.CountAsync(item => item.TrangThai == "Đang thuê");
            ViewBag.TongHoanThanh = await allContracts.CountAsync(item => item.TrangThai == "Hoàn thành");

            return View(hopDongList);
        }

        [HttpGet]
        public async Task<IActionResult> DonChoXuLy(string? searchString, DateTime? tuNgay, DateTime? denNgay)
        {
            var userId = int.TryParse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value, out var parsedUserId)
                ? parsedUserId
                : 0;

            var permission = userId > 0
                ? await _context.PhanQuyen.AsNoTracking().FirstOrDefaultAsync(p => p.UserId == userId)
                : null;

            if (permission?.CanProcessBooking != true)
            {
                return Redirect("/AccessDenied");
            }

            var query = _context.DatCho
                .AsNoTracking()
                .Include(item => item.Xe)
                .Where(item => item.TrangThai == ChoXacNhan || item.TrangThai == DangGiuCho)
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
            {
                query = query.Where(item => item.NgayDat.Date >= tuNgay.Value.Date);
            }

            if (denNgay.HasValue)
            {
                query = query.Where(item => item.NgayDat.Date <= denNgay.Value.Date);
            }

            var model = await query
                .OrderByDescending(item => item.NgayDat)
                .ToListAsync();

            ViewBag.SearchString = searchString;
            ViewBag.TuNgay = tuNgay;
            ViewBag.DenNgay = denNgay;
            ViewBag.TongDonCho = model.Count;

            return View(model);
        }

        [HttpPost("/HopDong/XuLyDon")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> XuLyDon(int id)
        {
            var userId = int.TryParse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value, out var parsedUserId)
                ? parsedUserId
                : 0;

            var permission = userId > 0
                ? await _context.PhanQuyen.AsNoTracking().FirstOrDefaultAsync(p => p.UserId == userId)
                : null;

            if (permission?.CanProcessBooking != true)
            {
                return Redirect("/AccessDenied");
            }

            var strategy = _context.Database.CreateExecutionStrategy();
            try
            {
                await strategy.ExecuteAsync(async () =>
                {
                    await using var transaction = await _context.Database.BeginTransactionAsync();
                    try
                    {
                        await XuLyDonTrongGiaoDichAsync(id);
                        await transaction.CommitAsync();
                    }
                    catch
                    {
                        await transaction.RollbackAsync();
                        throw;
                    }
                });
            }
            catch (XuLyDonFlowException ex)
            {
                if (ex.NotFound)
                {
                    return NotFound();
                }

                TempData["HopDongMessage"] = ex.Message;
                return RedirectToAction(nameof(DonChoXuLy));
            }
            catch (Exception ex)
            {
                var rootError = ex.InnerException?.Message ?? ex.Message;
                TempData["HopDongMessage"] = $"Có lỗi khi tạo hợp đồng từ đơn chờ xử lý: {rootError}";
                return RedirectToAction(nameof(DonChoXuLy));
            }

            TempData["HopDongMessage"] = "Đã xác nhận đơn và tạo hợp đồng thành công.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost("/HopDong/HuyDon")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> HuyDon(int id)
        {
            var datCho = await _context.DatCho.FirstOrDefaultAsync(item => item.MaDatCho == id);
            if (datCho is null)
            {
                return NotFound();
            }

            if (!IsChoStaffQueue(datCho.TrangThai))
            {
                TempData["HopDongMessage"] = "Đơn này đã được xử lý trước đó.";
                return RedirectToAction(nameof(DonChoXuLy));
            }

            datCho.TrangThai = Huy;
            await _context.SaveChangesAsync();

            TempData["HopDongMessage"] = "Đã hủy đơn chờ xử lý.";
            return RedirectToAction(nameof(DonChoXuLy));
        }

        [HttpGet]
        public async Task<IActionResult> TimPhieuDatCho(string? soDienThoai)
        {
            ViewBag.SoDienThoai = soDienThoai;

            if (string.IsNullOrWhiteSpace(soDienThoai))
            {
                ViewBag.HasSearched = false;
                return View(new List<DatCho>());
            }

            ViewBag.HasSearched = true;
            var phone = soDienThoai.Trim();

            var danhSachPhieu = await _context.DatCho
                .AsNoTracking()
                .Include(item => item.Xe)
                .Where(item => item.SoDienThoai != null && item.SoDienThoai.Contains(phone))
                .OrderByDescending(item => item.NgayDat)
                .ToListAsync();

            return View(danhSachPhieu);
        }

        [HttpGet]
        public async Task<IActionResult> LichSuKhachHang(string? soDienThoai)
        {
            ViewBag.SoDienThoai = soDienThoai;

            if (string.IsNullOrWhiteSpace(soDienThoai))
            {
                ViewBag.HasSearched = false;
                return View(Array.Empty<HopDong>());
            }

            ViewBag.HasSearched = true;
            var phone = soDienThoai.Trim();
            var list = await _context.HopDong
                .AsNoTracking()
                .Include(h => h.ChiTietHopDong).ThenInclude(ct => ct.Xe)
                .Include(h => h.KhachHang)
                .Where(h => h.SoDienThoai == phone)
                .OrderByDescending(h => h.NgayTao)
                .ToListAsync();

            return View(list);
        }

        [HttpGet]
        public async Task<IActionResult> ChiTiet(int id)
        {
            var hopDong = await _context.HopDong
                .Include(h => h.ChiTietHopDong)
                    .ThenInclude(ct => ct.Xe)
                .Include(h => h.HoaDon)
                .FirstOrDefaultAsync(h => h.MaHopDong == id);

            if (hopDong is null)
            {
                return NotFound();
            }

            return View(hopDong);
        }

        [HttpGet]
        public async Task<IActionResult> TraXe(int id)
        {
            var userId = int.TryParse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value, out var parsedUserId)
                ? parsedUserId
                : 0;

            var permission = userId > 0
                ? await _context.PhanQuyen.AsNoTracking().FirstOrDefaultAsync(p => p.UserId == userId)
                : null;

            if (permission?.CanReturnVehicle != true)
            {
                return Redirect("/AccessDenied");
            }

            var hopDong = await _context.HopDong
                .Include(h => h.ChiTietHopDong)
                    .ThenInclude(ct => ct.Xe)
                .Include(h => h.HoaDon)
                .FirstOrDefaultAsync(h => h.MaHopDong == id);

            if (hopDong is null)
            {
                return NotFound();
            }

            if (!string.Equals(hopDong.TrangThai, "Đang thuê", StringComparison.OrdinalIgnoreCase))
            {
                TempData["HopDongMessage"] = "Hợp đồng này không còn ở trạng thái đang thuê để thực hiện trả xe.";
                return RedirectToAction(nameof(ChiTiet), new { id });
            }

            if (hopDong.ChiTietHopDong == null || !hopDong.ChiTietHopDong.Any())
            {
                TempData["HopDongMessage"] = "Hợp đồng này không có thông tin xe thuê.";
                return RedirectToAction(nameof(ChiTiet), new { id });
            }

            return View(hopDong);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TraXe(
            int id,
            DateTime ngayTraThucTe,
            decimal phuPhi,
            string tinhTrangXe,
            string? loaiThietHai,
            DateTime? ngayXayRaThietHai,
            string? moTaThietHai,
            decimal chiPhiThietHai,
            string? ghiChu)
        {
            var userId = int.TryParse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value, out var parsedUserId)
                ? parsedUserId
                : 0;

            var permission = userId > 0
                ? await _context.PhanQuyen.AsNoTracking().FirstOrDefaultAsync(p => p.UserId == userId)
                : null;

            if (permission?.CanReturnVehicle != true)
            {
                return Redirect("/AccessDenied");
            }

            var strategy = _context.Database.CreateExecutionStrategy();
            try
            {
                await strategy.ExecuteAsync(async () =>
                {
                    await using var transaction = await _context.Database.BeginTransactionAsync();
                    try
                    {
                        var hopDong = await _context.HopDong
                            .Include(h => h.ChiTietHopDong)
                                .ThenInclude(ct => ct.Xe)
                            .Include(h => h.HoaDon)
                            .FirstOrDefaultAsync(h => h.MaHopDong == id);

                        if (hopDong is null)
                        {
                            throw new TraXeFlowException(notFound: true);
                        }

                        if (!string.Equals(hopDong.TrangThai, "Đang thuê", StringComparison.OrdinalIgnoreCase))
                        {
                            throw new TraXeFlowException(notFound: false, message: "Hợp đồng này không còn ở trạng thái đang thuê.");
                        }

                        if (hopDong.ChiTietHopDong == null || !hopDong.ChiTietHopDong.Any())
                        {
                            throw new TraXeFlowException(notFound: false, message: "Hợp đồng này không có thông tin xe thuê.");
                        }

                        var laCoSuCo = string.Equals(tinhTrangXe, "Có sự cố", StringComparison.OrdinalIgnoreCase);

                        var tienCocDuKien = hopDong.ChiTietHopDong.Sum(ct => TinhTienCocTheoDuKien(
                            ct.NgayNhanXe,
                            ct.NgayTraXeDuKien,
                            ct.GiaThueNgay,
                            ct.Xe?.GiaTriXe ?? 0));

                        // Cập nhật hợp đồng & chi tiết theo ngày trả thực tế
                        foreach (var ct in hopDong.ChiTietHopDong)
                        {
                            var soNgayThueThucTe = CalcRentalDays(ct.NgayNhanXe, ngayTraThucTe);
                            ct.NgayTraXeThucTe = ngayTraThucTe;
                            ct.SoNgayThue = soNgayThueThucTe;
                            ct.ThanhTien = ct.GiaThueNgay * soNgayThueThucTe;

                            ct.TrangThaiXe = "Đã trả";
                            ct.TinhTrangTraXe = tinhTrangXe; // map raw theo giá trị từ UI
                            ct.PhiDenBu = laCoSuCo ? chiPhiThietHai : 0;
                            ct.MoTaThietHai = laCoSuCo ? moTaThietHai : null;
                            ct.GhiChu = ghiChu;

                            if (ct.Xe != null)
                            {
                                if (!laCoSuCo)
                                {
                                    ct.Xe.TrangThai = "Sẵn sàng";
                                }
                                else if (string.Equals(loaiThietHai, "Mất xe", StringComparison.OrdinalIgnoreCase))
                                {
                                    ct.Xe.TrangThai = "Mất";
                                }
                                else
                                {
                                    ct.Xe.TrangThai = "Hư hỏng";
                                }
                            }
                        }

                        hopDong.NgayTraXeThucTe = ngayTraThucTe;
                        hopDong.PhuPhi = phuPhi;
                        hopDong.TongTien = hopDong.ChiTietHopDong.Sum(ct => ct.ThanhTien) + phuPhi + hopDong.ChiTietHopDong.Sum(ct => ct.PhiDenBu);
                        hopDong.TienCoc = tienCocDuKien; // thống nhất để tính hóa đơn
                        hopDong.GhiChu = ghiChu;
                        hopDong.TrangThai = "Hoàn thành";

                        // Cập nhật trạng thái DatCho thành Hoàn thành
                        if (hopDong.MaDatCho.HasValue)
                        {
                            var datCho = await _context.DatCho.FirstOrDefaultAsync(d => d.MaDatCho == hopDong.MaDatCho.Value);
                            if (datCho != null)
                            {
                                datCho.TrangThai = bikey.Models.DatCho.DatChoTrangThai.HoanThanh;
                            }
                        }

                        var tongPhiDenBu = hopDong.ChiTietHopDong.Sum(ct => ct.PhiDenBu);
                        var tongTienThueXe = hopDong.ChiTietHopDong.Sum(ct => ct.ThanhTien);
                        var tongCong = tongTienThueXe + phuPhi + tongPhiDenBu;
                        // Tiền cọc sẽ được hoàn lại, nên số tiền ghi hóa đơn chỉ gồm:
                        // tiền thuê + phụ phí + phí đền bù.
                        var soTienHoaDon = Math.Max(0m, tongCong);

                        if (hopDong.HoaDon is null)
                        {
                            hopDong.HoaDon = new HoaDon
                            {
                                MaHopDong = hopDong.MaHopDong,
                                NgayThanhToan = DateTime.Now,
                                SoTien = soTienHoaDon,
                                TrangThai = "Đã thanh toán",
                                GhiChu = ghiChu,
                                NgayTao = DateTime.Now,
                                MaNguoiTao = GetCurrentUserId()
                            };
                        }
                        else
                        {
                            hopDong.HoaDon.NgayThanhToan = DateTime.Now;
                            hopDong.HoaDon.SoTien = soTienHoaDon;
                            hopDong.HoaDon.TrangThai = "Đã thanh toán";
                            hopDong.HoaDon.GhiChu = ghiChu;
                            hopDong.HoaDon.NgayTao = DateTime.Now;
                            hopDong.HoaDon.MaNguoiTao = GetCurrentUserId();
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
            catch (TraXeFlowException ex)
            {
                if (ex.NotFound)
                {
                    return NotFound();
                }

                TempData["HopDongMessage"] = ex.Message;
                return RedirectToAction(nameof(ChiTiet), new { id });
            }
            catch (Exception ex)
            {
                var rootError = ex.InnerException?.Message ?? ex.Message;
                TempData["HopDongMessage"] = $"Có lỗi khi xử lý trả xe: {rootError}";
                return RedirectToAction(nameof(ChiTiet), new { id });
            }

            TempData["HopDongMessage"] = "Đã xử lý trả xe và tạo hóa đơn thành công.";
            return RedirectToAction(nameof(ChiTiet), new { id });
        }

        /// <summary>Tạo hợp đồng + chi tiết trong một transaction; một lần SaveChanges.</summary>
        private async Task XuLyDonTrongGiaoDichAsync(int id)
        {
            var datCho = await _context.DatCho
                .Include(item => item.Xe)
                .FirstOrDefaultAsync(item => item.MaDatCho == id);

            if (datCho is null)
            {
                throw new XuLyDonFlowException(notFound: true);
            }

            var loi = ValidateDatChoTruocKhiTaoHopDong(datCho);
            if (loi is not null)
            {
                throw new XuLyDonFlowException(notFound: false, message: loi);
            }

            var xe = datCho.Xe!;
            var rentalDays = Math.Max(1, (datCho.NgayTraXe - datCho.NgayNhanXe).Days);
            var rentalTotal = xe.GiaThue * rentalDays;
            var deposit = Math.Max(xe.GiaTriXe * DepositRate, rentalTotal * TimeFactor);
            var cccd = NormalizeDigits(datCho.SoCanCuoc);
            var soDienThoai = NormalizeDigits(datCho.SoDienThoai);

            var maKhachHangHopLe = await ResolveExistingUserIdAsync(datCho.MaUser);
            var maNguoiTaoHopLe = await ResolveExistingUserIdAsync(GetCurrentUserId());

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
                TrangThai = "Đang thuê",
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
                TrangThaiXe = "Đang thuê",
                NgayTao = DateTime.Now,
                HopDong = hopDong
            });

            _context.HopDong.Add(hopDong);
            xe.TrangThai = "Đang thuê";
            datCho.TrangThai = DaXuLy;

            await _context.SaveChangesAsync();
        }
        private static string? ValidateDatChoTruocKhiTaoHopDong(DatCho datCho)
        {
            if (!IsChoStaffQueue(datCho.TrangThai))
            {
                return "Đơn này đã được xử lý trước đó.";
            }

            if (datCho.Xe is null)
            {
                return "Không tìm thấy thông tin xe cho đơn đặt chỗ này.";
            }

            if (!string.Equals(datCho.Xe.TrangThai, "Sẵn sàng", StringComparison.OrdinalIgnoreCase))
            {
                return "Xe không còn sẵn sàng để tạo hợp đồng.";
            }

            var cccd = NormalizeDigits(datCho.SoCanCuoc);
            if (cccd is null || cccd.Length != 12)
            {
                return "Đơn chưa có CCCD hợp lệ (12 số), không thể tạo hợp đồng.";
            }

            var soDienThoai = NormalizeDigits(datCho.SoDienThoai);
            if (soDienThoai is null || (soDienThoai.Length != 10 && soDienThoai.Length != 11))
            {
                return "Đơn chưa có số điện thoại hợp lệ, không thể tạo hợp đồng.";
            }

            return null;
        }
        private Task<int> CountDatChoTrongHangDoiAsync() =>
            _context.DatCho.CountAsync(item => item.TrangThai == ChoXacNhan || item.TrangThai == DangGiuCho);
        private static string? NormalizeDigits(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            return new string(value.Where(char.IsDigit).ToArray());
        }
        private int? GetCurrentUserId()
        {
            return int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId)
                ? userId
                : null;
        }
        private async Task<int?> ResolveExistingUserIdAsync(int? userId)
        {
            if (!userId.HasValue)
            {
                return null;
            }

            var exists = await _context.NguoiDung.AnyAsync(item => item.Id == userId.Value);
            return exists ? userId : null;
        }
        /// <summary>Lỗi luồng xử lý đơn — NotFound hoặc thông báo hiển thị.</summary>
        private sealed class XuLyDonFlowException : Exception
        {
            public bool NotFound { get; }

            public XuLyDonFlowException(bool notFound, string? message = null)
                : base(message ?? string.Empty)
            {
                NotFound = notFound;
            }
        }
        private static int CalcRentalDays(DateTime ngayNhan, DateTime ngayTra)
        {
            var days = (ngayTra.Date - ngayNhan.Date).Days;
            return Math.Max(1, days);
        }
        private decimal TinhTienCocTheoDuKien(
            DateTime ngayNhanXe,
            DateTime ngayTraXeDuKien,
            decimal giaThueNgay,
            decimal giaTriXe)
        {
            var rentalDaysDuKien = CalcRentalDays(ngayNhanXe, ngayTraXeDuKien);
            var depositTheoXe = giaTriXe * DepositRate;
            var depositTheoThue = giaThueNgay * rentalDaysDuKien * TimeFactor;
            return Math.Max(depositTheoXe, depositTheoThue);
        }
        private sealed class TraXeFlowException : Exception
        {
            public bool NotFound { get; }

            public TraXeFlowException(bool notFound, string? message = null)
                : base(message ?? string.Empty)
            {
                NotFound = notFound;
            }
        }
    }
}
