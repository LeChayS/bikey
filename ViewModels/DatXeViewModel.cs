using System.ComponentModel.DataAnnotations;

namespace bikey.ViewModels
{
    public class DatXeViewModel
    {
        [Required]
        public int MaXe { get; set; }

        public string TenXe { get; set; } = string.Empty;

        public string BienSoXe { get; set; } = string.Empty;

        public string HangXe { get; set; } = string.Empty;

        public string DongXe { get; set; } = string.Empty;

        public string TrangThaiXe { get; set; } = string.Empty;

        public decimal GiaThueNgay { get; set; }

        [Display(Name = "Giá trị xe")]
        public decimal GiaTriXe { get; set; }

        [Display(Name = "Loại xe")]
        public string TenLoaiXe { get; set; } = string.Empty;

        public string? HinhAnhXe { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập họ tên khách hàng")]
        [Display(Name = "Họ tên khách hàng")]
        public string HoTenKhachHang { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
        [Display(Name = "SĐT")]
        [RegularExpression(@"^\d{10}$", ErrorMessage = "Số điện thoại phải có 10 chữ số")]
        public string SoDienThoai { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập địa chỉ")]
        [Display(Name = "Địa chỉ")]
        public string DiaChi { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập căn cước")]
        [Display(Name = "Căn cước")]
        [RegularExpression(@"^\d{12}$", ErrorMessage = "Căn cước phải gồm đúng 12 chữ số")]
        public string CanCuoc { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập email")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        [Display(Name = "Email")]
        [StringLength(100)]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng chọn ngày nhận")]
        [Display(Name = "Ngày nhận dự kiến")]
        [DataType(DataType.Date)]
        public DateTime NgayNhanDuKien { get; set; } = DateTime.Today.AddDays(1);

        [Required(ErrorMessage = "Vui lòng chọn ngày trả")]
        [Display(Name = "Ngày trả dự kiến")]
        [DataType(DataType.Date)]
        public DateTime NgayTraDuKien { get; set; } = DateTime.Today.AddDays(2);

        [Display(Name = "Ghi chú")]
        [StringLength(500)]
        public string? GhiChu { get; set; }

        [Display(Name = "Tổng tiền")]
        public decimal TongTienDuKien { get; set; }

        public int SoNgayThue =>
            NgayTraDuKien > NgayNhanDuKien
                ? (NgayTraDuKien - NgayNhanDuKien).Days
                : 0;
    }
}
