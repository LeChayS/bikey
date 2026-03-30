using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using bikey.Common;
using bikey.Models;
using bikey.Services;
using bikey.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace bikey.Controllers
{
    [Microsoft.AspNetCore.Authorization.Authorize(Roles = "Admin,Staff")]
    public class XeController : BaseController
    {
        private readonly IXeService _xeService;
        private readonly ILoaiXeService _loaiXeService;

        public XeController(
            IUserService userService,
            IXeService xeService,
            ILoaiXeService loaiXeService)
            : base(userService)
        {
            _xeService = xeService;
            _loaiXeService = loaiXeService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var statistics = await _xeService.GetStatisticsAsync();
            var xeList = await _xeService.GetAllAsync(excludeDeleted: true);
            var loaiXeList = await _loaiXeService.GetAllAsync();
            var hangXeList = await _xeService.GetHangXeListAsync();
            var trangThaiList = await _xeService.GetTrangThaiListAsync();

            var viewModel = new XeIndexViewModel
            {
                XeList = xeList,
                TongSoXe = statistics.TotalCount,
                XeSanSang = statistics.AvailableCount,
                DangChoThue = statistics.RentedCount,
                BaoTri = statistics.MaintenanceCount,
                LoaiXeList = loaiXeList.Select(l => l.TenLoaiXe).ToList(),
                HangXeList = hangXeList,
                TrangThaiList = trangThaiList
            };

            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var permissionCheck = await RequirePermissionAsync(p => p.CanCreateXe);
            if (permissionCheck != null) return permissionCheck;

            await PopulateCreateViewBagsAsync();
            return View(new Xe());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Xe xe, IFormFile hinhAnh, IFormFile[]? hinhAnhKhac)
        {
            var permissionCheck = await RequirePermissionAsync(p => p.CanCreateXe);
            if (permissionCheck != null) return permissionCheck;

            if (!ModelState.IsValid)
            {
                await PopulateCreateViewBagsAsync(xe);
                return View(xe);
            }

            const long maxFileSizeBytes = 5L * 1024 * 1024; // 5MB
            var allowedExt = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { ".jpg", ".jpeg", ".png", ".webp" };

            if (hinhAnh == null || hinhAnh.Length == 0)
            {
                ModelState.AddModelError("hinhAnh", "Hình ảnh chính là bắt buộc.");
            }
            else
            {
                var ext = Path.GetExtension(hinhAnh.FileName);
                if (string.IsNullOrWhiteSpace(ext) || !allowedExt.Contains(ext) || hinhAnh.Length > maxFileSizeBytes)
                {
                    ModelState.AddModelError("hinhAnh", "Hình ảnh chính không hợp lệ (JPG/PNG/WebP, tối đa 5MB).");
                }
            }

            var subImages = (hinhAnhKhac ?? Array.Empty<IFormFile>()).Where(f => f != null && f.Length > 0).ToList();
            if (subImages.Count > 10)
            {
                ModelState.AddModelError("hinhAnhKhac", "Tối đa 10 hình ảnh phụ.");
            }

            foreach (var file in subImages)
            {
                var ext = Path.GetExtension(file.FileName);
                if (string.IsNullOrWhiteSpace(ext) || !allowedExt.Contains(ext) || file.Length > maxFileSizeBytes)
                {
                    ModelState.AddModelError("hinhAnhKhac", "Một hoặc nhiều ảnh phụ có định dạng không hợp lệ hoặc quá lớn.");
                    break;
                }
            }

            if (!ModelState.IsValid)
            {
                await PopulateCreateViewBagsAsync(xe);
                return View(xe);
            }

            await _xeService.CreateAsync(xe, hinhAnh, subImages);

            TempData["Success"] = "Thêm xe thành công.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> GetXeDetails(int xeId)
        {
            var xe = await _xeService.GetByIdAsync(xeId);

            if (xe is null)
            {
                return Json(new { success = false, message = "Không tìm thấy xe." });
            }

            return Json(new
            {
                success = true,
                xe = new
                {
                    maXe = xe.MaXe,
                    tenXe = xe.TenXe,
                    bienSoXe = xe.BienSoXe,
                    hangXe = xe.HangXe,
                    dongXe = xe.DongXe,
                    loaiXe = xe.LoaiXe?.TenLoaiXe,
                    trangThai = xe.TrangThai,
                    giaThue = xe.GiaThue,
                    giaTriXe = xe.GiaTriXe,
                    tongChiPhi = xe.ChiPhiSuaChua,
                    chiPhiSuaChua = xe.ChiPhiSuaChua,
                    hinhAnhHienThi = xe.HinhAnhHienThi
                }
            });
        }

        [HttpGet]
        public async Task<IActionResult> FilterXe(string? searchString, string? loaiXe, string? hangXe, string? trangThai, bool showDeleted = false)
        {
            var data = await _xeService.FilterAsync(searchString, loaiXe, hangXe, trangThai, showDeleted);
            return PartialView("_XeTablePartial", new XeTablePartialModel
            {
                Items = data,
                Variant = XeTableVariant.QuanLy
            });
        }

        [HttpGet]
        public async Task<IActionResult> CheckXeContractHistory(int xeId)
        {
            var hasContracts = await _xeService.CheckXeContractHistoryAsync(xeId);
            return Json(new
            {
                success = true,
                hasContracts
            });
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var permissionCheck = await RequirePermissionAsync(p => p.CanEditXe);
            if (permissionCheck != null) return permissionCheck;

            var xe = await _xeService.GetByIdAsync(id);

            if (xe is null)
            {
                TempData["Error"] = "Không tìm thấy xe cần chỉnh sửa.";
                return RedirectToAction(nameof(Index));
            }

            await PopulateCreateViewBagsAsync(xe);
            return View(xe);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Xe model, IFormFile? hinhAnh, IFormFile[]? hinhAnhKhac, int[]? removeImageIds)
        {
            var permissionCheck = await RequirePermissionAsync(p => p.CanEditXe);
            if (permissionCheck != null) return permissionCheck;

            if (id != model.MaXe) return NotFound();

            if (!ModelState.IsValid)
            {
                await PopulateCreateViewBagsAsync(model);
                return View(model);
            }

            try
            {
                await _xeService.UpdateAsync(model, hinhAnh, hinhAnhKhac, removeImageIds);
                TempData["Success"] = "Cập nhật xe thành công.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Lỗi khi cập nhật xe: {ex.Message}";
                return RedirectToAction(nameof(Edit), new { id });
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var permissionCheck = await RequirePermissionAsync(p => p.CanDeleteXe);
            if (permissionCheck != null) return permissionCheck;

            var xe = await _xeService.GetByIdAsync(id);
            if (xe is null)
            {
                TempData["Error"] = "Không tìm thấy xe để xóa.";
                return RedirectToAction(nameof(Index));
            }

            if (xe.TrangThai == StatusConstants.XeStatus.DangThue)
            {
                TempData["Error"] = "Không thể xóa xe đang cho thuê.";
                return RedirectToAction(nameof(Index));
            }

            await _xeService.DeleteAsync(id);
            TempData["Success"] = "Đã xóa xe (soft-delete) và có thể khôi phục lại từ trang xe đã xóa.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Restore(int id)
        {
            await _xeService.RestoreAsync(id);
            TempData["Success"] = "Khôi phục xe thành công.";
            return RedirectToAction(nameof(Index));
        }

        private async Task PopulateCreateViewBagsAsync(Xe? xeModel = null)
        {
            var loaiXeList = await _loaiXeService.GetAllAsync();
            ViewBag.MaLoaiXe = new SelectList(loaiXeList.OrderBy(x => x.TenLoaiXe), "MaLoaiXe", "TenLoaiXe", xeModel?.MaLoaiXe);
            ViewBag.TrangThaiList = new SelectList(StatusConstants.XeStatus.AllStatuses, xeModel?.TrangThai);
        }
    }
}
