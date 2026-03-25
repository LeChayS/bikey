using System.Globalization;
using System.Text;
using bikey.Models;
using bikey.Repository;
using bikey.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace bikey.Services
{
    public class TrangChuService : ITrangChuService
    {
        private readonly BikeyDbContext _context;

        public TrangChuService(BikeyDbContext context)
        {
            _context = context;
        }

        public async Task<TrangChuViewModel> BuildTrangChuViewModelAsync()
        {
            var danhSachXeDb = await _context.Xe
                .AsNoTracking()
                .Include(xe => xe.LoaiXe)
                .Include(xe => xe.HinhAnhXes)
                .Where(xe => xe.TrangThai == "Sẵn sàng")
                .OrderBy(xe => xe.GiaThue)
                .ToListAsync();

            var mappedDanhSachXe = danhSachXeDb
                .Select(xe => new XeNoiBatViewModel
                {
                    MaXe = xe.MaXe,
                    Slug = GenerateSlug(xe.TenXe),
                    TenXe = xe.TenXe,
                    LoaiXe = xe.LoaiXe != null ? xe.LoaiXe.TenLoaiXe : "Xe máy",
                    GiaTheoNgay = $"{xe.GiaThue:N0}đ / ngày",
                    HinhAnh = NormalizeImageUrl(xe.HinhAnhHienThi),
                    DiaDiemNhanXe = "Liên hệ cửa hàng",
                    HopSo = string.Empty,
                    NhienLieu = "Xăng",
                    MoTaNgan = $"{xe.HangXe} {xe.DongXe} - trạng thái: {xe.TrangThai}",
                    BieuTuong = GetBikeIcon(xe)
                })
                .ToList();

            return new TrangChuViewModel
            {
                DanhSachXe = mappedDanhSachXe
            };
        }

        public async Task<ChiTietXeViewModel?> GetChiTietXeAsync(string slug)
        {
            if (string.IsNullOrWhiteSpace(slug))
            {
                return null;
            }

            var danhSachXe = await _context.Xe
                .AsNoTracking()
                .Include(xe => xe.LoaiXe)
                .Include(xe => xe.HinhAnhXes)
                .ToListAsync();

            var xe = danhSachXe.FirstOrDefault(item =>
                string.Equals(GenerateSlug(item.TenXe), slug, StringComparison.OrdinalIgnoreCase));

            if (xe is null)
            {
                return null;
            }

            var hinhAnh = xe.HinhAnhXes
                .OrderBy(item => item.ThuTu)
                .Select(item => NormalizeImageUrl(item.TenFile))
                .Where(item => !string.IsNullOrWhiteSpace(item))
                .Cast<string>()
                .ToList();

            return new ChiTietXeViewModel
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
        }

        private static string BuildMoTa(Xe xe)
        {
            return xe.TrangThai switch
            {
                "Sẵn sàng" => "Mau xe phu hop cho nhu cau di chuyen trong noi thanh va thue ngan ngay. Thu tuc nhan xe nhanh, de dang dat lich theo ngay.",
                "Đang thuê" => "Mau xe nay hien dang co khach su dung. Ban van co the xem thong tin chi tiet va chon mot xe khac tu danh sach.",
                _ => "Thong tin xe dang duoc cap nhat. Vui long lien he hotline de duoc tu van them ve tinh trang va lich trong."
            };
        }

        private static string GenerateSlug(string? value)
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

        private static string GetBikeIcon(Xe xe)
        {
            var text = $"{xe.LoaiXe?.TenLoaiXe} {xe.DongXe}".ToLowerInvariant();
            if (text.Contains("côn") || text.Contains("exciter"))
            {
                return "bi-lightning";
            }

            if (text.Contains("số") || text.Contains("wave") || text.Contains("future"))
            {
                return "bi-speedometer2";
            }

            return "bi-scooter";
        }
    }
}
