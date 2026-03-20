namespace bikey.ViewModels
{
    public class ChiTietXeViewModel
    {
        public int MaXe { get; init; }

        public string Slug { get; init; } = string.Empty;

        public string TenXe { get; init; } = string.Empty;

        public string HangXe { get; init; } = string.Empty;

        public string DongXe { get; init; } = string.Empty;

        public string LoaiXe { get; init; } = string.Empty;

        public string TrangThai { get; init; } = string.Empty;

        public decimal GiaThue { get; init; }

        public string BienSoXe { get; init; } = string.Empty;

        public string MoTa { get; init; } = string.Empty;

        public IReadOnlyList<string> HinhAnh { get; init; } = [];

        public bool CoTheDatXe => string.Equals(TrangThai, "Sẵn sàng", StringComparison.OrdinalIgnoreCase);

        public string TrangThaiHienThi => CoTheDatXe ? "Sẵn sàng" : TrangThai;

        public string NutTrangThaiText => CoTheDatXe ? "Xe sẵn sàng để thuê" : "Xe đang được thuê";

        public string NutTrangThaiCssClass => CoTheDatXe ? "success" : "warning";
    }
}
