using System.Globalization;
using System.Text;
using bikey.Models;
using bikey.Repository;
using bikey.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace bikey.Controllers
{
    public class TrangChuController : Controller
    {
        private readonly BikeyDbContext _context;

        public TrangChuController(BikeyDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View(BuildTrangChuViewModel());
        }

        public IActionResult DanhSachXe()
        {
            return View(BuildTrangChuViewModel());
        }

        public IActionResult GioiThieu()
        {
            return View();
        }

        public IActionResult ChiNhanh()
        {
            return View();
        }

        public IActionResult TinTuc()
        {
            return View();
        }

        public IActionResult LienHe()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> ChiTietXe(string slug)
        {
            if (string.IsNullOrWhiteSpace(slug))
            {
                return RedirectToAction(nameof(DanhSachXe));
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

        private TrangChuViewModel BuildTrangChuViewModel()
        {
            var danhSachXe = new List<XeNoiBatViewModel>
            {
                new()
                {
                    Slug = "honda-air-blade-160",
                    TenXe = "Honda Air Blade 160",
                    LoaiXe = "Xe tay ga cao cấp",
                    GiaTheoNgay = "180.000đ / ngày",
                    DiaDiemNhanXe = "Phú Nhuận, TP.HCM",
                    HopSo = "Tự động",
                    NhienLieu = "Xăng",
                    MoTaNgan = "Phù hợp đi nội thành, cốp rộng và tiết kiệm nhiên liệu.",
                    BieuTuong = "bi-scooter"
                },
                new()
                {
                    Slug = "honda-vision",
                    TenXe = "Honda Vision",
                    LoaiXe = "Xe tay ga phổ thông",
                    GiaTheoNgay = "140.000đ / ngày",
                    DiaDiemNhanXe = "Tân Bình, TP.HCM",
                    HopSo = "Tự động",
                    NhienLieu = "Xăng",
                    MoTaNgan = "Nhẹ, dễ lái, hợp với khách đi làm và du lịch ngắn ngày.",
                    BieuTuong = "bi-lightning-charge"
                },
                new()
                {
                    Slug = "yamaha-janus",
                    TenXe = "Yamaha Janus",
                    LoaiXe = "Xe tay ga thời trang",
                    GiaTheoNgay = "150.000đ / ngày",
                    DiaDiemNhanXe = "Quận 3, TP.HCM",
                    HopSo = "Tự động",
                    NhienLieu = "Xăng",
                    MoTaNgan = "Thiết kế gọn đẹp, phù hợp khách hàng trẻ và di chuyển linh hoạt.",
                    BieuTuong = "bi-stars"
                },
                new()
                {
                    Slug = "honda-wave-alpha",
                    TenXe = "Honda Wave Alpha",
                    LoaiXe = "Xe số tiết kiệm",
                    GiaTheoNgay = "110.000đ / ngày",
                    DiaDiemNhanXe = "Gò Vấp, TP.HCM",
                    HopSo = "Số",
                    NhienLieu = "Xăng",
                    MoTaNgan = "Giá thuê tốt, bền bỉ, thích hợp đi xa và tiết kiệm chi phí.",
                    BieuTuong = "bi-speedometer2"
                },
                new()
                {
                    Slug = "honda-future-125",
                    TenXe = "Honda Future 125",
                    LoaiXe = "Xe số mạnh mẽ",
                    GiaTheoNgay = "130.000đ / ngày",
                    DiaDiemNhanXe = "Bình Thạnh, TP.HCM",
                    HopSo = "Số",
                    NhienLieu = "Xăng",
                    MoTaNgan = "Máy khỏe, chạy êm, hợp cả nhu cầu đi làm lẫn đi tỉnh.",
                    BieuTuong = "bi-rocket-takeoff"
                },
                new()
                {
                    Slug = "yamaha-exciter-155",
                    TenXe = "Yamaha Exciter 155",
                    LoaiXe = "Xe côn tay",
                    GiaTheoNgay = "220.000đ / ngày",
                    DiaDiemNhanXe = "Quận 1, TP.HCM",
                    HopSo = "Côn tay",
                    NhienLieu = "Xăng",
                    MoTaNgan = "Phong cách thể thao, phù hợp khách thích trải nghiệm lái mạnh mẽ.",
                    BieuTuong = "bi-lightning"
                }
            };

            var maXeTheoTen = _context.Xe
                .Where(xe => danhSachXe.Select(item => item.TenXe).Contains(xe.TenXe))
                .ToDictionary(xe => xe.TenXe, xe => xe.MaXe);

            var mappedDanhSachXe = danhSachXe
                .Select(item => new XeNoiBatViewModel
                {
                    MaXe = maXeTheoTen.GetValueOrDefault(item.TenXe),
                    Slug = item.Slug,
                    TenXe = item.TenXe,
                    LoaiXe = item.LoaiXe,
                    GiaTheoNgay = item.GiaTheoNgay,
                    DiaDiemNhanXe = item.DiaDiemNhanXe,
                    HopSo = item.HopSo,
                    NhienLieu = item.NhienLieu,
                    MoTaNgan = item.MoTaNgan,
                    BieuTuong = item.BieuTuong
                })
                .ToList();

            return new TrangChuViewModel
            {
                DanhSachXe = mappedDanhSachXe
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
