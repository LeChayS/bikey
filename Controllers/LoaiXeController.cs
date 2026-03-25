using bikey.Services;
using Microsoft.AspNetCore.Mvc;

namespace bikey.Controllers
{
    public class LoaiXeController : BaseController
    {
        private readonly ILoaiXeService _loaiXeService;

        public LoaiXeController(IUserService userService, ILoaiXeService loaiXeService)
            : base(userService)
        {
            _loaiXeService = loaiXeService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var loaiXeList = await _loaiXeService.GetAllAsync();
            return View(loaiXeList);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string TenLoaiXe)
        {
            var permissionCheck = await RequirePermissionAsync(p => p.CanCreateLoaiXe);
            if (permissionCheck != null) return permissionCheck;

            TenLoaiXe = TenLoaiXe?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(TenLoaiXe))
            {
                TempData["Error"] = "Tên loại xe không được để trống.";
                return RedirectToAction(nameof(Index));
            }

            if (TenLoaiXe.Length > 50)
            {
                TempData["Error"] = "Tên loại xe không được vượt quá 50 ký tự.";
                return RedirectToAction(nameof(Index));
            }

            if (await _loaiXeService.ExistsAsync(TenLoaiXe))
            {
                TempData["Error"] = "Loại xe này đã tồn tại.";
                return RedirectToAction(nameof(Index));
            }

            await _loaiXeService.CreateAsync(TenLoaiXe);
            TempData["Success"] = "Thêm loại xe thành công.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int MaLoaiXe, string TenLoaiXe)
        {
            var permissionCheck = await RequirePermissionAsync(p => p.CanEditLoaiXe);
            if (permissionCheck != null) return permissionCheck;

            TenLoaiXe = TenLoaiXe?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(TenLoaiXe))
            {
                TempData["Error"] = "Tên loại xe không được để trống.";
                return RedirectToAction(nameof(Index));
            }

            if (TenLoaiXe.Length > 50)
            {
                TempData["Error"] = "Tên loại xe không được vượt quá 50 ký tự.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                await _loaiXeService.UpdateAsync(MaLoaiXe, TenLoaiXe);
                TempData["Success"] = "Cập nhật loại xe thành công.";
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var permissionCheck = await RequirePermissionAsync(p => p.CanDeleteLoaiXe);
            if (permissionCheck != null) return permissionCheck;

            var loaiXe = await _loaiXeService.GetByIdAsync(id);
            if (loaiXe is null)
            {
                return NotFound();
            }

            if (await _loaiXeService.HasVehiclesAsync(id))
            {
                TempData["Error"] = "Không thể xóa loại xe này vì vẫn còn xe thuộc loại đó.";
                return RedirectToAction(nameof(Index));
            }

            await _loaiXeService.DeleteAsync(id);

            TempData["Success"] = "Xóa loại xe thành công.";
            return RedirectToAction(nameof(Index));
        }
    }
}
