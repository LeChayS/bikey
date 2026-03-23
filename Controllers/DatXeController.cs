using System.Security.Claims;
using bikey.Models;
using bikey.Repository;
using bikey.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace bikey.Controllers
{
    public class DatXeController : Controller
    {
        private readonly BikeyDbContext _context;

        public DatXeController(BikeyDbContext context)
        {
            _context = context;
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Create(int maXe)
        {
            var xe = await _context.Xe
                .AsNoTracking()
                .Include(item => item.LoaiXe)
                .FirstOrDefaultAsync(item => item.MaXe == maXe);

            if (xe is null)
            {
                return NotFound();
            }

            var model = BuildDatXeViewModel(xe);
            return View(model);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(DatXeViewModel model)
        {
            var xe = await _context.Xe
                .AsNoTracking()
                .Include(item => item.LoaiXe)
                .FirstOrDefaultAsync(item => item.MaXe == model.MaXe);

            if (xe is null)
            {
                return NotFound();
            }

            PopulateVehicleInfo(model, xe);

            if (model.NgayTraDuKien <= model.NgayNhanDuKien)
            {
                ModelState.AddModelError(nameof(model.NgayTraDuKien), "Ngày trả phải sau ngày nhận ít nhất 1 ngày.");
            }

            if (!string.Equals(xe.TrangThai, "Sẵn sàng", StringComparison.OrdinalIgnoreCase))
            {
                ModelState.AddModelError(string.Empty, "Xe này hiện chưa sẵn sàng để đặt.");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var datCho = new DatCho
            {
                MaXe = xe.MaXe,
                MaUser = GetCurrentUserId(),
                HoTen = model.HoTenKhachHang,
                SoDienThoai = model.SoDienThoai,
                DiaChi = model.DiaChi,
                SoCanCuoc = model.CanCuoc,
                Email = model.Email.Trim(),
                NgayNhanXe = model.NgayNhanDuKien,
                NgayTraXe = model.NgayTraDuKien,
                GhiChu = model.GhiChu,
                TrangThai = "Chờ xác nhận",
                NgayDat = DateTime.Now
            };

            _context.DatCho.Add(datCho);
            await _context.SaveChangesAsync();

            TempData["BookingSuccessMessage"] = "Đơn đặt xe đã được tạo và chuyển sang màn hình chờ admin/nhân viên duyệt.";
            return RedirectToAction(nameof(Success), new { id = datCho.MaDatCho });
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Success(int id)
        {
            var datCho = await _context.DatCho
                .AsNoTracking()
                .Include(item => item.Xe)
                    .ThenInclude(x => x!.LoaiXe)
                .Include(item => item.Xe)
                    .ThenInclude(x => x!.HinhAnhXes)
                .FirstOrDefaultAsync(item => item.MaDatCho == id);

            if (datCho is null)
            {
                return NotFound();
            }

            return View(datCho);
        }

        private DatXeViewModel BuildDatXeViewModel(Xe xe)
        {
            var model = new DatXeViewModel
            {
                MaXe = xe.MaXe,
                HoTenKhachHang = User.FindFirstValue(ClaimTypes.Name) ?? string.Empty,
                SoDienThoai = User.FindFirstValue(ClaimTypes.MobilePhone) ?? string.Empty,
                DiaChi = User.FindFirstValue(ClaimTypes.StreetAddress) ?? string.Empty,
                Email = User.FindFirstValue(ClaimTypes.Email) ?? string.Empty
            };

            PopulateVehicleInfo(model, xe);
            return model;
        }

        private static void PopulateVehicleInfo(DatXeViewModel model, Xe xe)
        {
            model.TenXe = xe.TenXe;
            model.BienSoXe = xe.BienSoXe;
            model.HangXe = xe.HangXe;
            model.DongXe = xe.DongXe;
            model.TrangThaiXe = xe.TrangThai;
            model.GiaThueNgay = xe.GiaThue;
            model.GiaTriXe = xe.GiaTriXe;
            model.TenLoaiXe = xe.LoaiXe?.TenLoaiXe ?? "—";
            model.TongTienDuKien = model.SoNgayThue * xe.GiaThue;
        }

        private int? GetCurrentUserId()
        {
            return int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId)
                ? userId
                : null;
        }
    }
}
