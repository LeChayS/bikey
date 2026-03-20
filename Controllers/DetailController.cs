using System.Globalization;
using System.Text;
using bikey.Repository;
using bikey.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace bikey.Controllers
{
    public class DetailController : Controller
    {
        private readonly BikeyDbContext _context;

        public DetailController(BikeyDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Detail(string slug)
        {
            if (string.IsNullOrWhiteSpace(slug))
            {
                return RedirectToAction("DanhSachXe", "TrangChu");
            }

            var danhSachXe = await _context.Xe
                .AsNoTracking()
                .Include(xe => xe.LoaiXe)
                .Include(xe => xe.HinhAnhXes)
                .ToListAsync();

            var xe = danhSachXe.FirstOrDefault(item =>
                string.Equals(ToSlug(item.TenXe), slug, StringComparison.OrdinalIgnoreCase));

            if (xe is null)
            {
                return NotFound();
            }

            var hinhAnh = xe.HinhAnhXes
                .OrderBy(item => item.ThuTu)
                .Select(item => NormalizeImageUrl(item.TenFile))
                .Where(item => !string.IsNullOrWhiteSpace(item))
                .Cast<string>()
                .ToList();

            var model = new ChiTietXeViewModel
            {
                MaXe = xe.MaXe,
                Slug = slug,
                TenXe = xe.TenXe,
                HangXe = xe.HangXe,
                DongXe = xe.DongXe,
                LoaiXe = xe.LoaiXe?.TenLoaiXe ?? "Xe máy",
                TrangThai = xe.TrangThai,
                GiaThue = xe.GiaThue,
                BienSoXe = xe.BienSoXe,
                MoTa = BuildMoTa(xe),
                HinhAnh = hinhAnh
            };

            return View(model);
        }

        private static string BuildMoTa(Models.Xe xe)
        {
            return xe.TrangThai switch
            {
                "Sẵn sàng" => "Mau xe phu hop cho nhu cau di chuyen trong noi thanh va thue ngan ngay. Thu tuc nhan xe nhanh, de dang dat lich theo ngay.",
                "Đang thuê" => "Mau xe nay hien dang co khach su dung. Ban van co the xem thong tin chi tiet va chon mot xe khac tu danh sach.",
                _ => "Thong tin xe dang duoc cap nhat. Vui long lien he hotline de duoc tu van them ve tinh trang va lich trong."
            };
        }

        private static string ToSlug(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return string.Empty;
            }

            var normalized = value.Normalize(NormalizationForm.FormD);
            var builder = new StringBuilder();

            foreach (var c in normalized)
            {
                var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory == UnicodeCategory.NonSpacingMark)
                {
                    continue;
                }

                builder.Append(c);
            }

            return builder
                .ToString()
                .Normalize(NormalizationForm.FormC)
                .ToLowerInvariant()
                .Replace('đ', 'd')
                .Replace(' ', '-');
        }

        private static string? NormalizeImageUrl(string? tenFile)
        {
            if (string.IsNullOrWhiteSpace(tenFile))
            {
                return null;
            }

            if (tenFile.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                tenFile.StartsWith("https://", StringComparison.OrdinalIgnoreCase) ||
                tenFile.StartsWith("/", StringComparison.OrdinalIgnoreCase))
            {
                return tenFile;
            }

            return $"/{tenFile.TrimStart('~', '/')}";
        }
    }
}
