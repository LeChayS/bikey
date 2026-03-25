using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using bikey.Models;
using bikey.Repository;
using bikey.Services;
using bikey.ViewModels;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace bikey.Controllers
{
    [Microsoft.AspNetCore.Authorization.Authorize(Roles = "Admin,Staff")]
    public class XeController : Controller
    {
        private readonly BikeyDbContext _context;
        private readonly IWebHostEnvironment _env;
        private readonly IUserService _userService;

        public XeController(BikeyDbContext context, IWebHostEnvironment env, IUserService userService)
        {
            _context = context;
            _env = env;
            _userService = userService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var xeList = await _context.Xe
                .AsNoTracking()
                .Include(x => x.LoaiXe)
                .Include(x => x.HinhAnhXes)
                .OrderByDescending(x => x.MaXe)
                .ToListAsync();

            var xeKhongXoa = xeList.Where(x => x.TrangThai != "Đã xóa").ToList();
            ViewBag.TongSoXe = xeKhongXoa.Count;
            ViewBag.XeSanSang = xeKhongXoa.Count(x => x.TrangThai == "Sẵn sàng");
            ViewBag.DangChoThue = xeKhongXoa.Count(x => x.TrangThai == "Đang thuê");
            ViewBag.BaoTri = xeKhongXoa.Count(x => x.TrangThai == "Bảo trì");

            await PopulateIndexFilterViewBagsAsync();
            return View(xeKhongXoa);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var userId = _userService.GetUserIdFromClaims(User);
            var permission = userId.HasValue ? await _userService.GetPermissionAsync(userId.Value) : null;

            if (permission?.CanCreateXe != true)
            {
                return Redirect("/AccessDenied");
            }

            await PopulateCreateViewBagsAsync();
            return View(new Xe());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Xe xe, IFormFile hinhAnh, IFormFile[]? hinhAnhKhac)
        {
            var userId = _userService.GetUserIdFromClaims(User);
            var permission = userId.HasValue ? await _userService.GetPermissionAsync(userId.Value) : null;

            if (permission?.CanCreateXe != true)
            {
                return Redirect("/AccessDenied");
            }

            // Validate model fields first (DataAnnotations on Xe)
            if (!ModelState.IsValid)
            {
                await PopulateCreateViewBagsAsync(xe);
                return View(xe);
            }

            const long maxFileSizeBytes = 5L * 1024 * 1024; // 5MB
            var allowedExt = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                ".jpg",
                ".jpeg",
                ".png",
                ".webp"
            };

            // Validate main image
            if (hinhAnh == null || hinhAnh.Length == 0)
            {
                ModelState.AddModelError("hinhAnh", "Hình ảnh chính là bắt buộc.");
            }
            else
            {
                var ext = Path.GetExtension(hinhAnh.FileName);
                if (string.IsNullOrWhiteSpace(ext) || !allowedExt.Contains(ext))
                {
                    ModelState.AddModelError("hinhAnh", "Định dạng hình ảnh chính không hợp lệ (JPG/PNG/WebP).");
                }

                if (hinhAnh.Length > maxFileSizeBytes)
                {
                    ModelState.AddModelError("hinhAnh", "Hình ảnh chính vượt quá dung lượng 5MB.");
                }
            }

            // Validate additional images
            var subImages = (hinhAnhKhac ?? Array.Empty<IFormFile>()).Where(f => f != null && f.Length > 0).ToList();
            if (subImages.Count > 10)
            {
                ModelState.AddModelError("hinhAnhKhac", "Tối đa 10 hình ảnh phụ.");
            }

            for (var i = 0; i < subImages.Count; i++)
            {
                var file = subImages[i];
                var ext = Path.GetExtension(file.FileName);
                if (string.IsNullOrWhiteSpace(ext) || !allowedExt.Contains(ext))
                {
                    ModelState.AddModelError($"hinhAnhKhac[{i}]", "Một hoặc nhiều ảnh phụ có định dạng không hợp lệ (JPG/PNG/WebP).");
                }

                if (file.Length > maxFileSizeBytes)
                {
                    ModelState.AddModelError($"hinhAnhKhac[{i}]", "Một hoặc nhiều ảnh phụ vượt quá dung lượng 5MB.");
                }
            }

            if (!ModelState.IsValid)
            {
                await PopulateCreateViewBagsAsync(xe);
                return View(xe);
            }

            // Validate upload directory and save files first (collect relative paths for DB)
            var uploadDir = Path.Combine(_env.WebRootPath, "images", "xe");
            Directory.CreateDirectory(uploadDir);

            async Task<string> SaveAsLocalXeImageAsync(IFormFile file)
            {
                var ext = Path.GetExtension(file.FileName);
                var fileName = $"{Guid.NewGuid():N}{ext}";
                var fullPath = Path.Combine(uploadDir, fileName);

                await using var stream = System.IO.File.Create(fullPath);
                await file.CopyToAsync(stream);

                // TenFile lưu dạng "images/xe/<fileName>" để TrangChuController.ChiTietXe normalize URL đúng.
                return Path.Combine("images", "xe", fileName).Replace("\\", "/");
            }

            var mainTenFile = await SaveAsLocalXeImageAsync(hinhAnh!);
            var subTenFiles = new List<string>(subImages.Count);
            foreach (var file in subImages)
            {
                subTenFiles.Add(await SaveAsLocalXeImageAsync(file));
            }

            // Save Xe entity
            _context.Xe.Add(xe);
            await _context.SaveChangesAsync();

            // Save images (main + sub)
            var images = new List<HinhAnhXe>
            {
                new HinhAnhXe
                {
                    MaXe = xe.MaXe,
                    TenFile = mainTenFile,
                    ThuTu = 1,
                    LaAnhChinh = true
                }
            };

            var order = 2;
            foreach (var tenFile in subTenFiles)
            {
                images.Add(new HinhAnhXe
                {
                    MaXe = xe.MaXe,
                    TenFile = tenFile,
                    ThuTu = order++,
                    LaAnhChinh = false
                });
            }

            _context.HinhAnhXe.AddRange(images);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Thêm xe thành công.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> GetXeDetails(int xeId)
        {
            var xe = await _context.Xe
                .AsNoTracking()
                .Include(x => x.LoaiXe)
                .Include(x => x.HinhAnhXes)
                .FirstOrDefaultAsync(x => x.MaXe == xeId);

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
            var query = _context.Xe
                .AsNoTracking()
                .Include(x => x.LoaiXe)
                .Include(x => x.HinhAnhXes)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchString))
            {
                var s = searchString.Trim();
                query = query.Where(x => x.TenXe.Contains(s) || x.BienSoXe.Contains(s));
            }

            if (!string.IsNullOrWhiteSpace(loaiXe))
            {
                var s = loaiXe.Trim();
                query = query.Where(x => x.LoaiXe != null && x.LoaiXe.TenLoaiXe == s);
            }

            if (!string.IsNullOrWhiteSpace(hangXe))
            {
                var s = hangXe.Trim();
                query = query.Where(x => x.HangXe != null && x.HangXe == s);
            }

            if (!string.IsNullOrWhiteSpace(trangThai))
            {
                var s = trangThai.Trim();
                query = query.Where(x => x.TrangThai == s);
            }

            if (!showDeleted)
            {
                query = query.Where(x => x.TrangThai != "Đã xóa");
            }

            var data = await query
                .OrderByDescending(x => x.MaXe)
                .ToListAsync();

            return PartialView("_XeTablePartial", new XeTablePartialModel
            {
                Items = data,
                Variant = XeTableVariant.QuanLy
            });
        }

        [HttpGet]
        public async Task<IActionResult> CheckXeContractHistory(int xeId)
        {
            var hasContracts = await _context.ChiTietHopDong
                .AsNoTracking()
                .AnyAsync(x => x.MaXe == xeId);

            return Json(new
            {
                success = true,
                hasContracts
            });
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var userId = _userService.GetUserIdFromClaims(User);
            var permission = userId.HasValue ? await _userService.GetPermissionAsync(userId.Value) : null;

            if (permission?.CanEditXe != true)
            {
                return Redirect("/AccessDenied");
            }

            var xe = await _context.Xe
                .Include(x => x.HinhAnhXes)
                .FirstOrDefaultAsync(x => x.MaXe == id);

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
            var userId = _userService.GetUserIdFromClaims(User);
            var permission = userId.HasValue ? await _userService.GetPermissionAsync(userId.Value) : null;

            if (permission?.CanEditXe != true)
            {
                return Redirect("/AccessDenied");
            }

            if (id != model.MaXe) return NotFound();

            var xe = await _context.Xe
                .Include(x => x.HinhAnhXes)
                .FirstOrDefaultAsync(x => x.MaXe == id);

            if (xe is null)
            {
                TempData["Error"] = "Không tìm thấy xe cần cập nhật.";
                return RedirectToAction(nameof(Index));
            }

            if (!ModelState.IsValid)
            {
                await PopulateCreateViewBagsAsync(model);
                return View(model);
            }

            xe.TenXe = model.TenXe;
            xe.BienSoXe = model.BienSoXe;
            xe.HangXe = model.HangXe;
            xe.DongXe = model.DongXe;
            xe.MaLoaiXe = model.MaLoaiXe;
            xe.GiaThue = model.GiaThue;
            xe.TrangThai = model.TrangThai;
            xe.GiaTriXe = model.GiaTriXe;
            xe.ChiPhiSuaChua = model.ChiPhiSuaChua;

            const long maxFileSizeBytes = 5L * 1024 * 1024;
            var allowedExt = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { ".jpg", ".jpeg", ".png", ".webp" };
            var uploadDir = Path.Combine(_env.WebRootPath, "images", "xe");
            Directory.CreateDirectory(uploadDir);

            async Task<string> SaveAsLocalXeImageAsync(IFormFile file)
            {
                var ext = Path.GetExtension(file.FileName);
                var fileName = $"{Guid.NewGuid():N}{ext}";
                var fullPath = Path.Combine(uploadDir, fileName);
                await using var stream = System.IO.File.Create(fullPath);
                await file.CopyToAsync(stream);
                return Path.Combine("images", "xe", fileName).Replace("\\", "/");
            }

            var removeIds = new HashSet<int>(removeImageIds ?? Array.Empty<int>());
            var currentMainImage = xe.HinhAnhXes.FirstOrDefault(x => x.LaAnhChinh);
            var isRemovingMainImage = currentMainImage != null && removeIds.Contains(currentMainImage.MaHinhAnh);
            var hasNewMainUpload = hinhAnh is { Length: > 0 };
            var hasNewSubUpload = (hinhAnhKhac ?? Array.Empty<IFormFile>()).Any(f => f != null && f.Length > 0);
            var hasExistingSecondaryAfterRemove = xe.HinhAnhXes.Any(x =>
                !x.LaAnhChinh && !removeIds.Contains(x.MaHinhAnh));

            if (isRemovingMainImage && !hasNewMainUpload && !hasExistingSecondaryAfterRemove && !hasNewSubUpload)
            {
                ModelState.AddModelError(string.Empty, "Không thể xóa ảnh chính khi xe chưa có ảnh phụ.");
                await PopulateCreateViewBagsAsync(model);
                model.HinhAnhXes = xe.HinhAnhXes;
                return View(model);
            }

            var imagesToRemove = xe.HinhAnhXes.Where(x => removeIds.Contains(x.MaHinhAnh)).ToList();
            foreach (var image in imagesToRemove)
            {
                DeleteXeImageFileIfExists(image.TenFile);
            }
            if (imagesToRemove.Count > 0)
            {
                _context.HinhAnhXe.RemoveRange(imagesToRemove);
                foreach (var image in imagesToRemove)
                {
                    xe.HinhAnhXes.Remove(image);
                }
            }

            if (hinhAnh is { Length: > 0 })
            {
                var ext = Path.GetExtension(hinhAnh.FileName);
                if (string.IsNullOrWhiteSpace(ext) || !allowedExt.Contains(ext) || hinhAnh.Length > maxFileSizeBytes)
                {
                    ModelState.AddModelError("hinhAnh", "Hình ảnh chính không hợp lệ (JPG/PNG/WebP, tối đa 5MB).");
                    await PopulateCreateViewBagsAsync(model);
                    return View(model);
                }

                var mainTenFile = await SaveAsLocalXeImageAsync(hinhAnh);
                var currentMain = xe.HinhAnhXes.FirstOrDefault(x => x.LaAnhChinh);
                if (currentMain is null)
                {
                    xe.HinhAnhXes.Add(new HinhAnhXe { MaXe = xe.MaXe, TenFile = mainTenFile, ThuTu = 1, LaAnhChinh = true });
                }
                else
                {
                    DeleteXeImageFileIfExists(currentMain.TenFile);
                    currentMain.TenFile = mainTenFile;
                }
            }

            foreach (var file in (hinhAnhKhac ?? Array.Empty<IFormFile>()).Where(f => f != null && f.Length > 0))
            {
                var ext = Path.GetExtension(file.FileName);
                if (string.IsNullOrWhiteSpace(ext) || !allowedExt.Contains(ext) || file.Length > maxFileSizeBytes)
                {
                    continue;
                }

                var tenFile = await SaveAsLocalXeImageAsync(file);
                var nextOrder = xe.HinhAnhXes.Count == 0 ? 1 : xe.HinhAnhXes.Max(x => x.ThuTu) + 1;
                xe.HinhAnhXes.Add(new HinhAnhXe { MaXe = xe.MaXe, TenFile = tenFile, ThuTu = nextOrder, LaAnhChinh = false });
            }

            var orderedImages = xe.HinhAnhXes
                .OrderBy(x => x.ThuTu)
                .ThenBy(x => x.MaHinhAnh)
                .ToList();
            var currentMainAfterChanges = orderedImages.FirstOrDefault(x => x.LaAnhChinh);

            foreach (var img in orderedImages)
            {
                img.LaAnhChinh = false;
            }

            if (orderedImages.Count > 0)
            {
                (currentMainAfterChanges ?? orderedImages[0]).LaAnhChinh = true;
            }

            await _context.SaveChangesAsync();
            TempData["Success"] = "Cập nhật xe thành công.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = _userService.GetUserIdFromClaims(User);
            var permission = userId.HasValue ? await _userService.GetPermissionAsync(userId.Value) : null;

            if (permission?.CanDeleteXe != true)
            {
                return Redirect("/AccessDenied");
            }

            var xe = await _context.Xe
                .Include(x => x.HinhAnhXes)
                .FirstOrDefaultAsync(x => x.MaXe == id);
            if (xe is null)
            {
                TempData["Error"] = "Không tìm thấy xe để xóa.";
                return RedirectToAction(nameof(Index));
            }

            if (xe.TrangThai == "Đang thuê")
            {
                TempData["Error"] = "Không thể xóa xe đang cho thuê.";
                return RedirectToAction(nameof(Index));
            }

            // Soft-delete: chỉ cập nhật trạng thái để tránh xóa cứng gây xung đột ràng buộc FK.
            xe.TrangThai = "Đã xóa";
            await _context.SaveChangesAsync();
            TempData["Success"] = "Đã xóa xe (soft-delete) và có thể khôi phục lại từ trang xe đã xóa.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Restore(int id)
        {
            var xe = await _context.Xe.FirstOrDefaultAsync(x => x.MaXe == id);
            if (xe is null)
            {
                TempData["Error"] = "Không tìm thấy xe để khôi phục.";
                return RedirectToAction(nameof(Index));
            }

            xe.TrangThai = "Sẵn sàng";
            await _context.SaveChangesAsync();
            TempData["Success"] = "Khôi phục xe thành công.";
            return RedirectToAction(nameof(Index));
        }

        private async Task PopulateCreateViewBagsAsync(Xe? xeModel = null)
        {
            var loaiXeList = await _context.LoaiXe
                .OrderBy(x => x.TenLoaiXe)
                .ToListAsync();

            ViewBag.MaLoaiXe = new SelectList(loaiXeList, "MaLoaiXe", "TenLoaiXe", xeModel?.MaLoaiXe);

            var trangThaiList = new[]
            {
                "Sẵn sàng",
                "Đang thuê",
                "Bảo trì",
                "Hư hỏng",
                "Mất",
                "Đã xóa"
            };

            ViewBag.TrangThaiList = new SelectList(trangThaiList, xeModel?.TrangThai);
        }

        private async Task PopulateIndexFilterViewBagsAsync()
        {
            var loaiXeList = await _context.LoaiXe
                .AsNoTracking()
                .OrderBy(x => x.TenLoaiXe)
                .Select(x => x.TenLoaiXe)
                .ToListAsync();

            var hangXeList = await _context.Xe
                .AsNoTracking()
                .Where(x => !string.IsNullOrEmpty(x.HangXe))
                .Select(x => x.HangXe!)
                .Distinct()
                .OrderBy(x => x)
                .ToListAsync();

            var trangThaiList = await _context.Xe
                .AsNoTracking()
                .Where(x => !string.IsNullOrEmpty(x.TrangThai))
                .Select(x => x.TrangThai!)
                .Distinct()
                .OrderBy(x => x)
                .ToListAsync();

            ViewBag.LoaiXeList = loaiXeList.Select(x => new SelectListItem { Value = x, Text = x }).ToList();
            ViewBag.HangXeList = hangXeList.Select(x => new SelectListItem { Value = x, Text = x }).ToList();
            ViewBag.TrangThaiList = trangThaiList.Select(x => new SelectListItem { Value = x, Text = x }).ToList();
        }

        private void DeleteXeImageFileIfExists(string? tenFile)
        {
            if (string.IsNullOrWhiteSpace(tenFile)) return;

            var normalized = tenFile.Replace("/", Path.DirectorySeparatorChar.ToString())
                .Replace("\\", Path.DirectorySeparatorChar.ToString())
                .TrimStart(Path.DirectorySeparatorChar);

            var fullPath = Path.Combine(_env.WebRootPath, normalized);
            if (System.IO.File.Exists(fullPath))
            {
                System.IO.File.Delete(fullPath);
            }
        }
    }
}
