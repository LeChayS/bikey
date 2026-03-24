using bikey.Repository;
using bikey.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace bikey.Controllers
{
    [Authorize(Roles = "Admin,Staff")]
    public class ThongKeBaoCaoController : Controller
    {
        private const string TrangHoanThanh = "Hoàn thành";
        private readonly BikeyDbContext _context;

        public ThongKeBaoCaoController(BikeyDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string chartFilter = "month")
        {
            var model = await BuildViewModelAsync(chartFilter);
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> GetChartData(string filter = "month")
        {
            var vm = await BuildViewModelAsync(filter);
            return Json(new
            {
                success = true,
                doanhThu = new
                {
                    labels = vm.BieuDoDoanhThu.Select(x => x.Label).ToList(),
                    data = vm.BieuDoDoanhThu.Select(x => (double)x.Value).ToList()
                },
                chiTieu = new
                {
                    labels = vm.BieuDoChiTieu.Select(x => x.Label).ToList(),
                    data = vm.BieuDoChiTieu.Select(x => (double)x.Value).ToList()
                },
                thietHai = new
                {
                    labels = vm.BieuDoThietHai.Select(x => x.Label).ToList(),
                    data = vm.BieuDoThietHai.Select(x => (double)x.Value).ToList()
                },
                topXe = new
                {
                    labels = vm.TopXeThueNhieu.Select(x => $"{x.TenXe} ({x.BienSo})").ToList(),
                    data = vm.TopXeThueNhieu.Select(x => (double)x.SoLanThue).ToList()
                },
                loaiXe = new
                {
                    labels = vm.BieuDoLoaiXe.Select(x => x.Label).ToList(),
                    data = vm.BieuDoLoaiXe.Select(x => (double)x.Value).ToList()
                },
                hopDongTrangThai = new
                {
                    labels = vm.BieuDoHopDongTrangThai.Select(x => x.Label).ToList(),
                    data = vm.BieuDoHopDongTrangThai.Select(x => (double)x.Value).ToList()
                }
            });
        }

        [HttpGet]
        public async Task<IActionResult> GetStatistics(string filter = "month")
        {
            var vm = await BuildViewModelAsync(filter);
            return Json(new
            {
                success = true,
                doanhThu = vm.DoanhThu,
                doanhThuGoc = vm.DoanhThuGoc,
                tongChiTieu = vm.TongChiTieu,
                tongThietHai = vm.TongThietHai,
                tongDonDat = vm.TongDonDat,
                khachHangMoi = vm.KhachHangMoi,
                xeDangChoThue = vm.XeDangChoThue
            });
        }

        private async Task<ThongKeBaoCaoViewModel> BuildViewModelAsync(string filter)
        {
            filter = string.IsNullOrWhiteSpace(filter) ? "month" : filter.ToLowerInvariant();
            var periods = BuildPeriods(filter);
            var rangeStart = periods[0].Start.Date;
            var rangeEnd = periods[^1].End.Date;

            var hopDone = await _context.HopDong.AsNoTracking()
                .Where(h => h.TrangThai == TrangHoanThanh
                    && h.NgayTraXeThucTe.HasValue
                    && h.NgayTraXeThucTe.Value.Date >= rangeStart
                    && h.NgayTraXeThucTe.Value.Date <= rangeEnd)
                .Select(h => new
                {
                    h.TongTien,
                    h.PhuPhi,
                    CloseDate = h.NgayTraXeThucTe!.Value.Date
                })
                .ToListAsync();

            var chiTietDone = await _context.ChiTietHopDong.AsNoTracking()
                .Where(c => c.HopDong.TrangThai == TrangHoanThanh
                    && c.HopDong.NgayTraXeThucTe.HasValue
                    && c.HopDong.NgayTraXeThucTe.Value.Date >= rangeStart
                    && c.HopDong.NgayTraXeThucTe.Value.Date <= rangeEnd)
                .Select(c => new
                {
                    c.PhiDenBu,
                    CloseDate = c.HopDong.NgayTraXeThucTe!.Value.Date
                })
                .ToListAsync();

            var datCho = await _context.DatCho.AsNoTracking()
                .Where(d => d.NgayDat.Date >= rangeStart && d.NgayDat.Date <= rangeEnd)
                .Select(d => d.NgayDat.Date)
                .ToListAsync();

            var bieuDoDoanhThu = new List<ChartDataItem>();
            var bieuDoChiTieu = new List<ChartDataItem>();
            var bieuDoThietHai = new List<ChartDataItem>();

            foreach (var p in periods)
            {
                var ds = p.Start.Date;
                var de = p.End.Date;
                var doanhThu = hopDone.Where(x => x.CloseDate >= ds && x.CloseDate <= de).Sum(x => x.TongTien);
                var chiTieu = hopDone.Where(x => x.CloseDate >= ds && x.CloseDate <= de).Sum(x => x.PhuPhi);
                var thietHai = chiTietDone.Where(x => x.CloseDate >= ds && x.CloseDate <= de).Sum(x => x.PhiDenBu);
                bieuDoDoanhThu.Add(new ChartDataItem { Label = p.Label, Value = doanhThu });
                bieuDoChiTieu.Add(new ChartDataItem { Label = p.Label, Value = chiTieu });
                bieuDoThietHai.Add(new ChartDataItem { Label = p.Label, Value = thietHai });
            }

            var topXe = await BuildTopXeAsync(rangeStart, rangeEnd);

            var xeTheoLoai = await _context.Xe.AsNoTracking()
                .Include(x => x.LoaiXe)
                .Where(x => x.TrangThai != "Đã xóa")
                .ToListAsync();

            var bieuDoLoaiXe = xeTheoLoai
                .GroupBy(x => x.LoaiXe?.TenLoaiXe ?? "Khác")
                .Select(g => new ChartDataItem { Label = g.Key, Value = g.Count() })
                .OrderByDescending(x => x.Value)
                .ToList();

            var trangThaiHd = await _context.HopDong.AsNoTracking()
                .Where(h => h.NgayTao.Date >= rangeStart && h.NgayTao.Date <= rangeEnd)
                .Select(h => h.TrangThai)
                .ToListAsync();

            var bieuHd = trangThaiHd
                .GroupBy(t => t)
                .Select(g => new ChartDataItem { Label = g.Key, Value = g.Count() })
                .OrderByDescending(x => x.Value)
                .ToList();

            var xeDangThue = await _context.Xe.AsNoTracking()
                .CountAsync(x => x.TrangThai == "Đang thuê");

            var tongDoanhThu = hopDone.Sum(x => x.TongTien);
            var tongPhuPhi = hopDone.Sum(x => x.PhuPhi);
            var tongPhiDenBu = chiTietDone.Sum(x => x.PhiDenBu);

            var khachMoi = await _context.NguoiDung.AsNoTracking()
                .CountAsync(u => u.VaiTro == "User"
                    && u.NgayTao.Date >= rangeStart
                    && u.NgayTao.Date <= rangeEnd);

            return new ThongKeBaoCaoViewModel
            {
                ChartFilter = filter,
                PeriodLabel = GetPeriodLabel(filter),
                DoanhThu = tongDoanhThu,
                DoanhThuGoc = tongDoanhThu,
                TongChiTieu = tongPhuPhi,
                TongThietHai = tongPhiDenBu,
                TongDonDat = datCho.Count,
                KhachHangMoi = khachMoi,
                XeDangChoThue = xeDangThue,
                BieuDoDoanhThu = bieuDoDoanhThu,
                BieuDoChiTieu = bieuDoChiTieu,
                BieuDoThietHai = bieuDoThietHai,
                TopXeThueNhieu = topXe,
                BieuDoLoaiXe = bieuDoLoaiXe,
                BieuDoHopDongTrangThai = bieuHd
            };
        }

        private async Task<List<TopXeThueNhieuItem>> BuildTopXeAsync(DateTime rangeStart, DateTime rangeEnd)
        {
            var rows = await _context.ChiTietHopDong.AsNoTracking()
                .Include(c => c.Xe)
                .Include(c => c.HopDong)
                .Where(c => c.HopDong.TrangThai == TrangHoanThanh
                    && c.HopDong.NgayTraXeThucTe.HasValue
                    && c.HopDong.NgayTraXeThucTe.Value.Date >= rangeStart
                    && c.HopDong.NgayTraXeThucTe.Value.Date <= rangeEnd
                    && c.Xe != null
                    && c.Xe.TrangThai != "Đã xóa")
                .ToListAsync();

            return rows
                .GroupBy(c => c.MaXe)
                .Select(g =>
                {
                    var first = g.First();
                    var xe = first.Xe!;
                    return new TopXeThueNhieuItem
                    {
                        TenXe = xe.TenXe,
                        BienSo = xe.BienSoXe,
                        GiaThueNgay = xe.GiaThue,
                        TrangThaiXe = xe.TrangThai,
                        SoLanThue = g.Count(),
                        DoanhThu = g.Sum(x => x.ThanhTien)
                    };
                })
                .OrderByDescending(x => x.SoLanThue)
                .Take(5)
                .ToList();
        }

        private static string GetPeriodLabel(string filter) => filter switch
        {
            "today" => "hôm nay",
            "7days" => "7 ngày gần nhất",
            "week" => "tuần này",
            "month" => "30 ngày gần nhất",
            "year" => "12 tháng gần nhất",
            _ => "30 ngày gần nhất"
        };

        private static List<PeriodSlot> BuildPeriods(string filter)
        {
            var periods = new List<PeriodSlot>();
            var now = DateTime.Today;
            switch (filter)
            {
                case "today":
                    periods.Add(new PeriodSlot(now, now, now.ToString("dd/MM")));
                    break;
                case "week":
                    var startOfWeek = now.AddDays(-(int)now.DayOfWeek);
                    for (var i = 0; i < 7; i++)
                    {
                        var d = startOfWeek.AddDays(i);
                        periods.Add(new PeriodSlot(d, d, d.ToString("dd/MM")));
                    }
                    break;
                case "year":
                    for (var i = 11; i >= 0; i--)
                    {
                        var monthStart = new DateTime(now.Year, now.Month, 1).AddMonths(-i);
                        var monthEnd = monthStart.AddMonths(1).AddDays(-1);
                        periods.Add(new PeriodSlot(monthStart, monthEnd, monthStart.ToString("MM/yyyy")));
                    }
                    break;
                case "month":
                    for (var i = 29; i >= 0; i--)
                    {
                        var d = now.AddDays(-i);
                        periods.Add(new PeriodSlot(d, d, d.ToString("dd/MM")));
                    }
                    break;
                case "7days":
                default:
                    for (var i = 6; i >= 0; i--)
                    {
                        var d = now.AddDays(-i);
                        periods.Add(new PeriodSlot(d, d, d.ToString("dd/MM")));
                    }
                    break;
            }

            return periods;
        }

        private readonly struct PeriodSlot
        {
            public DateTime Start { get; }
            public DateTime End { get; }
            public string Label { get; }

            public PeriodSlot(DateTime start, DateTime end, string label)
            {
                Start = start;
                End = end;
                Label = label;
            }
        }
    }
}
