using bikey.Models;

namespace bikey.ViewModels
{
    /// <summary>
    /// ViewModel for the Vehicle (Xe) Index page.
    /// Replaces multiple ViewBag properties with a typed model.
    /// </summary>
    public class XeIndexViewModel
    {
        public List<Xe> XeList { get; set; } = new();
        
        // Statistics
        public int TongSoXe { get; set; }
        public int XeSanSang { get; set; }
        public int DangChoThue { get; set; }
        public int BaoTri { get; set; }
        
        // Filter options
        public List<string> LoaiXeList { get; set; } = new();
        public List<string> HangXeList { get; set; } = new();
        public List<string> TrangThaiList { get; set; } = new();
        
        // Current filter values
        public string? SearchString { get; set; }
        public string? SelectedLoaiXe { get; set; }
        public string? SelectedHangXe { get; set; }
        public string? SelectedTrangThai { get; set; }
        public bool ShowDeleted { get; set; }
    }
}
