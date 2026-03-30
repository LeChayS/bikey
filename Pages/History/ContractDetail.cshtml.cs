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
    public class ContractDetailModel : PageModel
    {
        private readonly BikeyDbContext _context;

        public ContractDetailModel(BikeyDbContext context)
        {
            _context = context;
        }

        public HopDong Contract { get; set; } = null!;
        public DatCho? Order { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var userId = int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var parsedUserId)
                ? parsedUserId
                : (int?)null;
            var phone = User.FindFirstValue(ClaimTypes.MobilePhone);

            // Find the order first
            var order = await _context.DatCho
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.MaDatCho == id);

            if (order is null)
            {
                return NotFound();
            }

            // Check if user has permission to view this order
            var hasPermission = (userId.HasValue && order.MaUser == userId.Value) || (!string.IsNullOrWhiteSpace(phone) && order.SoDienThoai == phone);

            if (!hasPermission)
            {
                return Forbid();
            }

            Order = order;

            // Find the contract associated with this order
            var contract = await _context.HopDong
                .AsNoTracking()
                .Include(h => h.DatCho)
                    .ThenInclude(d => d!.Xe)
                        .ThenInclude(x => x!.LoaiXe)
                .Include(h => h.DatCho)
                    .ThenInclude(d => d!.Xe)
                        .ThenInclude(x => x!.HinhAnhXes)
                .Include(h => h.ChiTietHopDong)
                    .ThenInclude(ct => ct.Xe)
                        .ThenInclude(x => x!.LoaiXe)
                .Include(h => h.NguoiTao)
                .FirstOrDefaultAsync(h => h.MaDatCho == id);

            if (contract is null)
            {
                // No contract yet, redirect to order detail
                return RedirectToPage("/History/OrderDetail", new { id });
            }

            Contract = contract;
            return Page();
        }
    }
}
