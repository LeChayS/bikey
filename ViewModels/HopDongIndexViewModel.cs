using bikey.Models;
using bikey.Services;

namespace bikey.ViewModels
{
    /// <summary>
    /// ViewModel for the Contract (HopDong) Index page.
    /// Replaces multiple ViewBag properties with a typed model.
    /// </summary>
    public class HopDongIndexViewModel
    {
        public List<HopDong> HopDongList { get; set; } = new();
        
        // Pagination
        public int TotalItems { get; set; }
        public int PageSize { get; set; } = 10;
        public int CurrentPage { get; set; } = 1;
        
        // Statistics
        public int DonChoXuLy { get; set; }
        public int DonChoXuLyMoi { get; set; }
        public int TongDangThue { get; set; }
        public int TongHoanThanh { get; set; }
        
        // Filter values
        public string? TrangThai { get; set; }
        public string? TuKhoa { get; set; }
        
        // Available filter options
        public List<string> TrangThaiList { get; set; } = new();
    }
}
