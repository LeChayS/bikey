using System.Security.Claims;
using bikey.Models;
using bikey.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace bikey.Pages.History
{
    [Authorize]
    public class OrderDetailModel : PageModel
    {
        private readonly BikeyDbContext _context;

        public OrderDetailModel(BikeyDbContext context)
        {
            _context = context;
        }

        public DatCho Order { get; set; } = null!;

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var userId = int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var parsedUserId)
                ? parsedUserId
                : (int?)null;
            var phone = User.FindFirstValue(ClaimTypes.MobilePhone);

            var order = await _context.DatCho
                .AsNoTracking()
                .Include(d => d.Xe)
                    .ThenInclude(x => x!.LoaiXe)
                .Include(d => d.Xe)
                    .ThenInclude(x => x!.HinhAnhXes)
                .FirstOrDefaultAsync(d => d.MaDatCho == id);

            if (order is null)
            {
                return NotFound();
            }

            if (userId.HasValue && order.MaUser == userId.Value)
            {
                Order = order;
                return Page();
            }

            if (!string.IsNullOrWhiteSpace(phone) && order.SoDienThoai == phone)
            {
                Order = order;
                return Page();
            }

            return Forbid();
        }
    }
}
