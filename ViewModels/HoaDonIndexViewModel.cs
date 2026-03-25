using bikey.Models;

namespace bikey.ViewModels
{
    /// <summary>
    /// ViewModel for the Invoice (HoaDon) Index page.
    /// Replaces multiple ViewBag properties with a typed model.
    /// </summary>
    public class HoaDonIndexViewModel
    {
        public List<HoaDon> HoaDonList { get; set; } = new();
        
        // Pagination
        public int TotalItems { get; set; }
        public int PageSize { get; set; } = 10;
        public int CurrentPage { get; set; } = 1;
        
        // Statistics
        public decimal TongDoanhThu { get; set; }
        public int SoHoaDonHomNay { get; set; }
        
        // Filter values
        public string? SearchString { get; set; }
        public DateTime? TuNgay { get; set; }
        public DateTime? DenNgay { get; set; }
    }
}
