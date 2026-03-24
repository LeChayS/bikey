namespace bikey.ViewModels
{
    public class ChartDataItem
    {
        public string Label { get; set; } = "";
        public decimal Value { get; set; }
    }

    public class TopXeThueNhieuItem
    {
        public string TenXe { get; set; } = "";
        public string BienSo { get; set; } = "";
        public decimal GiaThueNgay { get; set; }
        public string TrangThaiXe { get; set; } = "";
        public int SoLanThue { get; set; }
        public decimal DoanhThu { get; set; }
    }

    public class ThongKeBaoCaoViewModel
    {
        public string ChartFilter { get; set; } = "month";
        public string PeriodLabel { get; set; } = "";

        public decimal DoanhThu { get; set; }
        public decimal DoanhThuGoc { get; set; }
        public decimal TongChiTieu { get; set; }
        public decimal TongThietHai { get; set; }
        public int TongDonDat { get; set; }
        public int KhachHangMoi { get; set; }
        public int XeDangChoThue { get; set; }

        public List<ChartDataItem> BieuDoDoanhThu { get; set; } = new();
        public List<ChartDataItem> BieuDoChiTieu { get; set; } = new();
        public List<ChartDataItem> BieuDoThietHai { get; set; } = new();
        public List<TopXeThueNhieuItem> TopXeThueNhieu { get; set; } = new();
        public List<ChartDataItem> BieuDoLoaiXe { get; set; } = new();
        public List<ChartDataItem> BieuDoHopDongTrangThai { get; set; } = new();
    }

    public class RecentHopDongRow
    {
        public int MaHopDong { get; set; }
        public string TenKhach { get; set; } = "";
        public string TenXe { get; set; } = "";
        public string TrangThai { get; set; } = "";
        public DateTime? NgayHienThi { get; set; }
        public decimal TongTien { get; set; }
    }

    public class AdminDashboardViewModel
    {
        public int HopDongHoanThanh { get; set; }
        public decimal PhanTramHopDongVsKyTruoc { get; set; }

        public decimal DoanhThuHomNay { get; set; }
        public decimal PhanTramDoanhThuVsHomTruoc { get; set; }

        public int XeDangChoThue { get; set; }
        public int HopDongHoatDong { get; set; }
        public int KhachHangMoiHomNay { get; set; }
        public int TongSoXe { get; set; }

        public List<TopXeThueNhieuItem> TopXeThueNhieu { get; set; } = new();
        public List<RecentHopDongRow> HopDongGanDay { get; set; } = new();
    }
}
