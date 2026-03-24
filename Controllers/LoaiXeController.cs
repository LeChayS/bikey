using System;
using System.Threading.Tasks;
using bikey.Repository;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace bikey.Controllers
{
    public class LoaiXeController : Controller
    {
        private readonly BikeyDbContext _context;

        public LoaiXeController(BikeyDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var loaiXeList = await _context.LoaiXe
                .OrderByDescending(x => x.NgayTao)
                .ToListAsync();
            return View(loaiXeList);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string TenLoaiXe)
        {
            var userId = int.TryParse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value, out var parsedUserId)
                ? parsedUserId
                : 0;

            var permission = userId > 0
                ? await _context.PhanQuyen.AsNoTracking().FirstOrDefaultAsync(p => p.UserId == userId)
                : null;

            if (permission?.CanCreateLoaiXe != true)
            {
                return Redirect("/AccessDenied");
            }

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

            var tenLoaiXeNormalized = TenLoaiXe.ToLower();
            var exists = await _context.LoaiXe.AnyAsync(x => x.TenLoaiXe.ToLower() == tenLoaiXeNormalized);
            if (exists)
            {
                TempData["Error"] = "Loại xe này đã tồn tại.";
                return RedirectToAction(nameof(Index));
            }

            _context.LoaiXe.Add(new Models.LoaiXe
            {
                TenLoaiXe = TenLoaiXe,
                NgayTao = DateTime.Now,
                NgayCapNhat = null
            });

            await _context.SaveChangesAsync();
            TempData["Success"] = "Thêm loại xe thành công.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int MaLoaiXe, string TenLoaiXe)
        {
            var userId = int.TryParse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value, out var parsedUserId)
                ? parsedUserId
                : 0;

            var permission = userId > 0
                ? await _context.PhanQuyen.AsNoTracking().FirstOrDefaultAsync(p => p.UserId == userId)
                : null;

            if (permission?.CanEditLoaiXe != true)
            {
                return Redirect("/AccessDenied");
            }

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

            var loaiXe = await _context.LoaiXe.FirstOrDefaultAsync(x => x.MaLoaiXe == MaLoaiXe);
            if (loaiXe is null)
            {
                return NotFound();
            }

            loaiXe.TenLoaiXe = TenLoaiXe;
            loaiXe.NgayCapNhat = DateTime.Now;

            await _context.SaveChangesAsync();
            TempData["Success"] = "Cập nhật loại xe thành công.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = int.TryParse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value, out var parsedUserId)
                ? parsedUserId
                : 0;

            var permission = userId > 0
                ? await _context.PhanQuyen.AsNoTracking().FirstOrDefaultAsync(p => p.UserId == userId)
                : null;

            if (permission?.CanDeleteLoaiXe != true)
            {
                return Redirect("/AccessDenied");
            }

            var loaiXe = await _context.LoaiXe.FirstOrDefaultAsync(x => x.MaLoaiXe == id);
            if (loaiXe is null)
            {
                return NotFound();
            }

            var hasXe = await _context.Xe.AnyAsync(x => x.MaLoaiXe == id);
            if (hasXe)
            {
                TempData["Error"] = "Không thể xóa loại xe này vì vẫn còn xe thuộc loại đó.";
                return RedirectToAction(nameof(Index));
            }

            _context.LoaiXe.Remove(loaiXe);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Xóa loại xe thành công.";
            return RedirectToAction(nameof(Index));
        }
    }
}
