using bikey.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace bikey.Controllers
{
    [Authorize(Roles = "Admin,Staff")]
    public class QuanLyDatChoController : Controller
    {
        private readonly BikeyDbContext _context;

        public QuanLyDatChoController(BikeyDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var danhSachDatCho = await _context.DatCho
                .AsNoTracking()
                .Include(item => item.Xe)
                .OrderByDescending(item => item.NgayDat)
                .ToListAsync();

            return View(danhSachDatCho);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Duyet(int id)
        {
            var datCho = await _context.DatCho.FindAsync(id);
            if (datCho is null)
            {
                return NotFound();
            }

            datCho.TrangThai = "Đang giữ chỗ";
            await _context.SaveChangesAsync();

            TempData["BookingAdminMessage"] = "Đã duyệt đơn đặt chỗ.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TuChoi(int id)
        {
            var datCho = await _context.DatCho.FindAsync(id);
            if (datCho is null)
            {
                return NotFound();
            }

            datCho.TrangThai = "Hủy";
            await _context.SaveChangesAsync();

            TempData["BookingAdminMessage"] = "Đã từ chối đơn đặt chỗ.";
            return RedirectToAction(nameof(Index));
        }
    }
}
