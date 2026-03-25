using System.Security.Claims;
using bikey.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace bikey.Pages.Account
{
    [Authorize]
    public class OrdersModel : PageModel
    {
        private readonly BikeyDbContext _context;

        public OrdersModel(BikeyDbContext context)
        {
            _context = context;
        }

        public IReadOnlyList<OrderHistoryItem> Orders { get; private set; } = [];

        public int TotalOrders => Orders.Count;

        public int ProcessingOrders => Orders.Count(order =>
            order.StatusCssClass == "pending" || order.StatusCssClass == "info");

        public async Task OnGetAsync()
        {
            var phone = User.FindFirstValue(ClaimTypes.MobilePhone);
            var userId = int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var parsedUserId)
                ? parsedUserId
                : (int?)null;

            Orders = await _context.DatCho
                .AsNoTracking()
                .Include(datCho => datCho.Xe)
                .Where(datCho =>
                    (userId.HasValue && datCho.MaUser == userId.Value) ||
                    (!string.IsNullOrWhiteSpace(phone) && datCho.SoDienThoai == phone))
                .OrderByDescending(datCho => datCho.NgayDat)
                .Select(datCho => new OrderHistoryItem
                {
                    MaDatCho = datCho.MaDatCho,
                    MaDon = $"DC-{datCho.MaDatCho:D5}",
                    TenXe = datCho.Xe != null ? datCho.Xe.TenXe : "Xe không tồn tại",
                    ThoiGianThue = $"{(datCho.NgayTraXe - datCho.NgayNhanXe).Days} ngay",
                    TongTien = $"{(datCho.Xe != null ? datCho.Xe.GiaThue * (datCho.NgayTraXe - datCho.NgayNhanXe).Days : 0):N0} VND",
                    TrangThai = datCho.TrangThai,
                    StatusCssClass = MapStatusCssClass(datCho.TrangThai),
                    NgayDat = datCho.NgayDat.ToString("dd/MM/yyyy"),
                    NgayNhan = datCho.NgayNhanXe.ToString("dd/MM/yyyy"),
                    NgayTra = datCho.NgayTraXe.ToString("dd/MM/yyyy")
                })
                .ToListAsync();
        }

        private static string MapStatusCssClass(string trangThai) => trangThai switch
        {
            "Đã xử lý" => "success",
            "Chờ xác nhận" => "warning",
            "Hủy" => "danger",
            "Hoàn thành" => "info",
            _ => "warning"
        };

        public class OrderHistoryItem
        {
            public int MaDatCho { get; set; }

            public string MaDon { get; set; } = string.Empty;

            public string TenXe { get; set; } = string.Empty;

            public string ThoiGianThue { get; set; } = string.Empty;

            public string TongTien { get; set; } = string.Empty;

            public string TrangThai { get; set; } = string.Empty;

            public string StatusCssClass { get; set; } = "info";

            public string NgayDat { get; set; } = string.Empty;

            public string NgayNhan { get; set; } = string.Empty;

            public string NgayTra { get; set; } = string.Empty;
        }
    }
}
