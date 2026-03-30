using bikey.Common;
using bikey.Models;
using bikey.Services;
using bikey.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace bikey.Controllers
{
    public class DatXeController : BaseController
    {
        private readonly IDatXeService _datXeService;

        public DatXeController(IUserService userService, IDatXeService datXeService)
            : base(userService)
        {
            _datXeService = datXeService;
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Create(int maXe)
        {
            var xe = await _datXeService.GetXeForBookingAsync(maXe);
            if (xe is null)
            {
                return NotFound();
            }

            var model = _datXeService.BuildDatXeViewModel(xe, User);
            return View(model);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(DatXeViewModel model)
        {
            var xe = await _datXeService.GetXeForBookingAsync(model.MaXe);
            if (xe is null)
            {
                return NotFound();
            }

            _datXeService.PopulateVehicleInfo(model, xe);

            if (model.NgayTraDuKien <= model.NgayNhanDuKien)
            {
                ModelState.AddModelError(nameof(model.NgayTraDuKien), "Ngày trả phải sau ngày nhận ít nhất 1 ngày.");
            }

            if (xe.TrangThai != StatusConstants.XeStatus.SanSang)
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
                TrangThai = DatCho.DatChoTrangThai.ChoXacNhan,
                NgayDat = DateTime.Now
            };

            await _datXeService.CreateDatChoAsync(datCho);

            TempData["BookingSuccessMessage"] = "Đơn đặt xe đã được tạo và chuyển sang màn hình chờ admin/nhân viên duyệt.";
            return RedirectToAction(nameof(Success), new { id = datCho.MaDatCho });
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Success(int id)
        {
            var datCho = await _datXeService.GetDatChoByIdAsync(id);
            if (datCho is null)
            {
                return NotFound();
            }

            return View(datCho);
        }

    }
}
