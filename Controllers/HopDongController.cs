using System.Security.Claims;
using bikey.Models;
using bikey.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace bikey.Controllers
{
    [Authorize(Roles = "Admin,Staff")]
    public class HopDongController : Controller
    {
        private const decimal DepositRate = 0.13m;
        private const decimal TimeFactor = 0.35m;
        private readonly BikeyDbContext _context;

        public HopDongController(BikeyDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string? trangThai, int page = 1)
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
            ViewBag.DonChoXuLy = await _context.DatCho.CountAsync(item => item.TrangThai == "Chờ xác nhận");
            ViewBag.DonChoXuLyMoi = await _context.DatCho.CountAsync(item => item.TrangThai == "Chờ xác nhận" && item.NgayDat.Date == DateTime.Today);
            ViewBag.TongDangThue = await allContracts.CountAsync(item => item.TrangThai == "Đang thuê");
            ViewBag.TongHoanThanh = await allContracts.CountAsync(item => item.TrangThai == "Hoàn thành");

            return View(hopDongList);
        }

        [HttpGet]
        public async Task<IActionResult> DonChoXuLy(string? searchString, DateTime? tuNgay, DateTime? denNgay)
        {
            var query = _context.DatCho
                .AsNoTracking()
                .Include(item => item.Xe)
                .Where(item => item.TrangThai == "Chờ xác nhận")
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
            // EnableRetryOnFailure (SqlServerRetryingExecutionStrategy) không cho phép gọi BeginTransactionAsync
            // trực tiếp ngoài execution strategy — phải bọc toàn bộ transaction trong ExecuteAsync.
            var strategy = _context.Database.CreateExecutionStrategy();
            try
            {
                await strategy.ExecuteAsync(async () =>
                {
                    await using var transaction = await _context.Database.BeginTransactionAsync();
                    try
                    {
                        var datCho = await _context.DatCho
                            .Include(item => item.Xe)
                            .FirstOrDefaultAsync(item => item.MaDatCho == id);

                        if (datCho is null)
                        {
                            throw new XuLyDonNotFoundException();
                        }

                        if (!string.Equals(datCho.TrangThai, "Chờ xác nhận", StringComparison.OrdinalIgnoreCase))
                        {
                            throw new XuLyDonUserMessageException("Đơn này đã được xử lý trước đó.");
                        }

                        if (datCho.Xe is null)
                        {
                            throw new XuLyDonUserMessageException("Không tìm thấy thông tin xe cho đơn đặt chỗ này.");
                        }

                        if (!string.Equals(datCho.Xe.TrangThai, "Sẵn sàng", StringComparison.OrdinalIgnoreCase))
                        {
                            throw new XuLyDonUserMessageException("Xe không còn sẵn sàng để tạo hợp đồng.");
                        }

                        var cccd = NormalizeDigits(datCho.SoCanCuoc);
                        if (cccd is null || cccd.Length != 12)
                        {
                            throw new XuLyDonUserMessageException("Đơn chưa có CCCD hợp lệ (12 số), không thể tạo hợp đồng.");
                        }

                        var soDienThoai = NormalizeDigits(datCho.SoDienThoai);
                        if (soDienThoai is null || (soDienThoai.Length != 10 && soDienThoai.Length != 11))
                        {
                            throw new XuLyDonUserMessageException("Đơn chưa có số điện thoại hợp lệ, không thể tạo hợp đồng.");
                        }

                        var rentalDays = Math.Max(1, (datCho.NgayTraXe - datCho.NgayNhanXe).Days);
                        var rentalTotal = datCho.Xe.GiaThue * rentalDays;
                        var deposit = Math.Max(datCho.Xe.GiaTriXe * DepositRate, rentalTotal * TimeFactor);
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

                        _context.HopDong.Add(hopDong);
                        await _context.SaveChangesAsync();

                        var chiTietHopDong = new ChiTietHopDong
                        {
                            MaHopDong = hopDong.MaHopDong,
                            MaXe = datCho.Xe.MaXe,
                            GiaThueNgay = datCho.Xe.GiaThue,
                            NgayNhanXe = datCho.NgayNhanXe,
                            NgayTraXeDuKien = datCho.NgayTraXe,
                            SoNgayThue = rentalDays,
                            ThanhTien = rentalTotal,
                            TrangThaiXe = "Đang thuê",
                            NgayTao = DateTime.Now
                        };

                        _context.ChiTietHopDong.Add(chiTietHopDong);
                        datCho.Xe.TrangThai = "Đang thuê";
                        datCho.TrangThai = "Đã xử lý";

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
            catch (XuLyDonNotFoundException)
            {
                return NotFound();
            }
            catch (XuLyDonUserMessageException ex)
            {
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

            if (!string.Equals(datCho.TrangThai, "Chờ xác nhận", StringComparison.OrdinalIgnoreCase))
            {
                TempData["HopDongMessage"] = "Đơn này đã được xử lý trước đó.";
                return RedirectToAction(nameof(DonChoXuLy));
            }

            datCho.TrangThai = "Hủy";
            await _context.SaveChangesAsync();

            TempData["HopDongMessage"] = "Đã hủy đơn chờ xử lý.";
            return RedirectToAction(nameof(DonChoXuLy));
        }

        public IActionResult TimPhieuDatCho()
        {
            return View();
        }

        public IActionResult LichSuKhachHang()
        {
            return View();
        }

        public IActionResult ChiTiet()
        {
            return View();
        }

        public IActionResult TraXe()
        {
            return View();
        }

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

        /// <summary>Đơn không tồn tại — trả NotFound.</summary>
        private sealed class XuLyDonNotFoundException : Exception
        {
        }

        /// <summary>Lỗi nghiệp vụ có thông báo hiển thị cho người dùng.</summary>
        private sealed class XuLyDonUserMessageException : Exception
        {
            public XuLyDonUserMessageException(string message)
                : base(message)
            {
            }
        }
    }
}
