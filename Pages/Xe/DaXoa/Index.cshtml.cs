using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using bikey.Repository;
using XeEntity = bikey.Models.Xe;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace bikey.Pages.Xe.DaXoa
{
    [Authorize(Roles = "Admin,Staff")]
    public class IndexModel : PageModel
    {
        private readonly BikeyDbContext _context;

        public IReadOnlyList<XeEntity> XeDaXoa { get; private set; } = new List<XeEntity>();

        public IndexModel(BikeyDbContext context)
        {
            _context = context;
        }

        public async Task OnGetAsync()
        {
            XeDaXoa = await _context.Xe
                .AsNoTracking()
                .Include(x => x.LoaiXe)
                .Include(x => x.HinhAnhXes)
                .Where(x => x.TrangThai == "Đã xóa")
                .OrderByDescending(x => x.MaXe)
                .ToListAsync();
        }

        public async Task<IActionResult> OnPostRestoreAsync(int id)
        {
            var xe = await _context.Xe
                .FirstOrDefaultAsync(x => x.MaXe == id);

            if (xe is null)
            {
                return NotFound();
            }

            xe.TrangThai = "Sẵn sàng";
            await _context.SaveChangesAsync();
            TempData["XeManagementSuccess"] = "Khôi phục xe thành công.";
            return RedirectToPage();
        }
    }
}

