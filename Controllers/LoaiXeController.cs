using bikey.Services;
using Microsoft.AspNetCore.Mvc;

namespace bikey.Controllers
{
    public class LoaiXeController : BaseController
    {
        private readonly ILoaiXeService _loaiXeService;
        private readonly IDataChangeCheckService _dataChangeCheckService;

        public LoaiXeController(IUserService userService, ILoaiXeService loaiXeService, IDataChangeCheckService dataChangeCheckService)
            : base(userService)
        {
            _loaiXeService = loaiXeService;
            _dataChangeCheckService = dataChangeCheckService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userId = GetCurrentUserId();
            var permission = userId.HasValue ? await _userService.GetPermissionAsync(userId.Value) : null;
            ViewBag.CanCreateLoaiXe = permission?.CanCreateLoaiXe ?? false;
            ViewBag.CanEditLoaiXe = permission?.CanEditLoaiXe ?? false;
            ViewBag.CanDeleteLoaiXe = permission?.CanDeleteLoaiXe ?? false;

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

        /// <summary>
        /// API endpoint for auto-refresh feature - returns checksum of current vehicle type list
        /// </summary>
        [HttpPost]
        [Route("LoaiXe/GetDataChecksum")]
        public async Task<IActionResult> GetDataChecksum([FromBody] DataChecksumRequest request)
        {
            var userId = GetCurrentUserId();
            var permission = userId.HasValue ? await _userService.GetPermissionAsync(userId.Value) : null;

            if (permission == null || !permission.CanViewXe)
            {
                return Json(new { success = false, checksum = Guid.NewGuid().ToString() });
            }

            try
            {
                var types = await _loaiXeService.GetAllAsync();
                var checksum = _dataChangeCheckService.GenerateChecksum(types);
                return Json(new { success = true, checksum = checksum });
            }
            catch (Exception)
            {
                return Json(new { success = false, checksum = Guid.NewGuid().ToString() });
            }
        }
    }
}
