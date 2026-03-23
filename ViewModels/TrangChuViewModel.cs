using System.Collections.Generic;

namespace bikey.ViewModels
{
    public class TrangChuViewModel
    {
        public IReadOnlyList<XeNoiBatViewModel> DanhSachXe { get; init; } = new List<XeNoiBatViewModel>();
    }

    public class XeNoiBatViewModel
    {
        public int MaXe { get; init; }
        public string Slug { get; init; } = string.Empty;
        public string TenXe { get; init; } = string.Empty;
        public string LoaiXe { get; init; } = string.Empty;
        public string GiaTheoNgay { get; init; } = string.Empty;
        public string? HinhAnh { get; init; }
        public string DiaDiemNhanXe { get; init; } = string.Empty;
        public string HopSo { get; init; } = string.Empty;
        public string NhienLieu { get; init; } = string.Empty;
        public string MoTaNgan { get; init; } = string.Empty;
        public string BieuTuong { get; init; } = "bi-bicycle";
    }
}
