using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace bikey.Models
{
    public class DatCho
    {
        [Key]
        public int MaDatCho { get; set; }

        // Thông tin xe
        [Display(Name = "Xe")]
        public int MaXe { get; set; }

        [ForeignKey("MaXe")]
        public Xe? Xe { get; set; }

        // Thông tin người đặt
        [Display(Name = "Người đặt")]
        public int? MaUser { get; set; }

        [ForeignKey("MaUser")]
        public NguoiDung? User { get; set; }

        // Thông tin liên hệ (nếu không đăng nhập)
        [Display(Name = "Họ tên")]
        [StringLength(100)]
        public string? HoTen { get; set; }

        [Display(Name = "Số điện thoại")]
        [StringLength(20)]
        public string? SoDienThoai { get; set; }

        [Display(Name = "Địa chỉ")]
        [StringLength(255)]
        public string? DiaChi { get; set; }

        [Display(Name = "Căn cước công dân")]
        [StringLength(12)]
        public string? SoCanCuoc { get; set; }

        [Display(Name = "Email")]
        [StringLength(100)]
        public string? Email { get; set; }

        // Thời gian thuê
        [Required(ErrorMessage = "Vui lòng chọn ngày nhận xe")]
        [Display(Name = "Ngày nhận xe dự kiến")]
        [DataType(DataType.Date)]
        public DateTime NgayNhanXe { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn ngày trả xe")]
        [Display(Name = "Ngày trả xe dự kiến")]
        [DataType(DataType.Date)]
        public DateTime NgayTraXe { get; set; }

        // Thông tin khác
        [Display(Name = "Ghi chú")]
        [StringLength(500)]
        public string? GhiChu { get; set; }

        [Display(Name = "Ngày đặt")]
        public DateTime NgayDat { get; set; } = DateTime.Now;

        [Display(Name = "Trạng thái")]
        [StringLength(50)]
        public string TrangThai { get; set; } = DatChoTrangThai.ChoXacNhan;

        /// <summary>Giá trị trạng thái đơn đặt chỗ — dùng thống nhất trong controller/view.</summary>
        public static class DatChoTrangThai
        {
            public const string ChoXacNhan = "Chờ xác nhận";
            public const string DangGiuCho = "Đang giữ chỗ";
            public const string DaXuLy = "Đã xử lý";
            public const string Huy = "Hủy";

            /// <summary>Đơn vẫn nằm trong hàng đợi xử lý (duyệt giữ chỗ / tạo HĐ / hủy).</summary>
            public static bool IsChoStaffQueue(string? trangThai) =>
                string.Equals(trangThai, ChoXacNhan, StringComparison.OrdinalIgnoreCase)
                || string.Equals(trangThai, DangGiuCho, StringComparison.OrdinalIgnoreCase);
        }

        // Tính số ngày thuê
        [NotMapped]
        public int SoNgayThue => (NgayTraXe - NgayNhanXe).Days;

        // Tính tổng tiền dự kiến
        [NotMapped]
        public decimal TongTienDuKien => Xe != null ? Xe.GiaThue * SoNgayThue : 0;
    }
}
