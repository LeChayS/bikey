using bikey.Common;
using bikey.Models;
using bikey.Services;
using bikey.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace bikey.Controllers
{
    [Authorize(Roles = "Admin,Staff")]
    public class HopDongController : BaseController
    {
        private readonly IHopDongService _hopDongService;

        public HopDongController(IUserService userService, IHopDongService hopDongService)
            : base(userService)
        {
            _hopDongService = hopDongService;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string? trangThai, string? tuKhoa, int page = 1)
        {
            var permissionCheck = await RequirePermissionAsync(p => p.CanViewHopDong);
            if (permissionCheck != null) return permissionCheck;

            const int pageSize = 10;
            page = Math.Max(1, page);

            var result = await _hopDongService.GetPaginatedAsync(page, pageSize, trangThai, tuKhoa);
            var stats = await _hopDongService.GetStatisticsAsync();

            var viewModel = new HopDongIndexViewModel
            {
                HopDongList = result.Items,
                TotalItems = result.Total,
                PageSize = pageSize,
                CurrentPage = page,
                TrangThai = trangThai,
                TuKhoa = tuKhoa,
                DonChoXuLy = stats.TotalPending,
                DonChoXuLyMoi = stats.TotalNewToday,
                TongDangThue = stats.TotalActive,
                TongHoanThanh = stats.TotalCompleted
            };

            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> DonChoXuLy(string? searchString, DateTime? tuNgay, DateTime? denNgay)
        {
            var permissionCheck = await RequirePermissionAsync(p => p.CanViewHopDong && p.CanProcessBooking);
            if (permissionCheck != null) return permissionCheck;

            var model = await _hopDongService.GetDonChoXuLyAsync(searchString, tuNgay, denNgay);

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
            var permissionCheck = await RequirePermissionAsync(p => p.CanProcessBooking);
            if (permissionCheck != null) return permissionCheck;

            try
            {
                await _hopDongService.XuLyDonAsync(id, GetCurrentUserId());
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
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
            var permissionCheck = await RequirePermissionAsync(p => p.CanProcessBooking);
            if (permissionCheck != null) return permissionCheck;

            try
            {
                await _hopDongService.HuyDonAsync(id);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (InvalidOperationException ex)
            {
                TempData["HopDongMessage"] = ex.Message;
                return RedirectToAction(nameof(DonChoXuLy));
            }

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
            var danhSachPhieu = await _hopDongService.TimPhieuDatChoAsync(soDienThoai);
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

            var permissionCheck = await RequirePermissionAsync(p => p.CanViewUser);
            if (permissionCheck != null) return permissionCheck;

            ViewBag.HasSearched = true;
            var list = await _hopDongService.GetLichSuKhachHangAsync(soDienThoai);
            return View(list);
        }

        [HttpGet]
        public async Task<IActionResult> ChiTiet(int id)
        {
            var permissionCheck = await RequirePermissionAsync(p => p.CanViewHopDong);
            if (permissionCheck != null) return permissionCheck;

            var hopDong = await _hopDongService.GetChiTietAsync(id);
            if (hopDong is null)
            {
                return NotFound();
            }

            return View(hopDong);
        }

        [HttpGet]
        public async Task<IActionResult> TraXe(int id)
        {
            var permissionCheck = await RequirePermissionAsync(p => p.CanViewHopDong && p.CanReturnVehicle);
            if (permissionCheck != null) return permissionCheck;

            var hopDong = await _hopDongService.GetChiTietAsync(id);
            if (hopDong is null)
            {
                return NotFound();
            }

            if (hopDong.TrangThai != StatusConstants.HopDongStatus.DangThue)
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
            var permissionCheck = await RequirePermissionAsync(p => p.CanReturnVehicle);
            if (permissionCheck != null) return permissionCheck;

            var maNguoiTao = GetCurrentUserId();
            if (!maNguoiTao.HasValue)
            {
                TempData["HopDongMessage"] = "Không thể xác định người dùng hiện tại.";
                return RedirectToAction(nameof(ChiTiet), new { id });
            }

            try
            {
                await _hopDongService.TraXeAsync(id, ngayTraThucTe, phuPhi, tinhTrangXe, loaiThietHai, moTaThietHai, chiPhiThietHai, ghiChu, maNguoiTao.Value);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
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
    }
}
