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
    public class InvoiceDetailModel : PageModel
    {
        private readonly BikeyDbContext _context;

        public InvoiceDetailModel(BikeyDbContext context)
        {
            _context = context;
        }

        public HoaDon Invoice { get; set; } = null!;
        public HopDong Contract { get; set; } = null!;

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var userId = int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var parsedUserId)
                ? parsedUserId
                : (int?)null;
            var phone = User.FindFirstValue(ClaimTypes.MobilePhone);

            // Find the contract by MaDatCho (id is MaDatCho from Orders page)
            var contract = await _context.HopDong
                .AsNoTracking()
                .Include(h => h.DatCho)
                .Include(h => h.HoaDon)
                    .ThenInclude(hd => hd!.NguoiTao)
                .Include(h => h.ChiTietHopDong)
                    .ThenInclude(ct => ct.Xe)
                        .ThenInclude(x => x!.LoaiXe)
                .FirstOrDefaultAsync(h => h.MaDatCho == id);

            if (contract is null)
            {
                return NotFound();
            }

            // Check if user has permission to view this contract
            var hasPermission = (userId.HasValue && contract.MaKhachHang == userId.Value) ||
                               (!string.IsNullOrWhiteSpace(phone) && contract.SoDienThoai == phone);

            if (!hasPermission)
            {
                return Forbid();
            }

            Contract = contract;

            if (contract.HoaDon is null)
            {
                // No invoice yet, redirect to contract detail
                if (contract.MaDatCho.HasValue)
                {
                    return RedirectToPage("/History/ContractDetail", new { id = contract.MaDatCho.Value });
                }
                return NotFound();
            }

            Invoice = contract.HoaDon;
            return Page();
        }
    }
}
