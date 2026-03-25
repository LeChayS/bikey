using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace bikey.Models
{
    public class NguoiDung
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên")]
        [Display(Name = "Họ và tên")]
        [StringLength(100)]
        public string? Ten { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập email")]
        [Display(Name = "Email")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        [StringLength(100)]
        public string? Email { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu")]
        [Display(Name = "Mật khẩu")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Mật khẩu phải có ít nhất 6 ký tự")]
        [DataType(DataType.Password)]
        public string? MatKhau { get; set; }

        [NotMapped]
        [DataType(DataType.Password)]
        [Display(Name = "Xác nhận mật khẩu")]
        [Compare("MatKhau", ErrorMessage = "Mật khẩu và xác nhận mật khẩu không khớp.")]
        public string? XacNhanMatKhau { get; set; }

        [Display(Name = "Vai trò")]
        [StringLength(20)]
        public string VaiTro { get; set; } = "User"; // User, Staff, Admin
        [Display(Name = "Số điện thoại")]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        [RegularExpression(@"^\d{10}$", ErrorMessage = "Số điện thoại phải có đúng 10 chữ số.")]
        [StringLength(20)]
        public string? SoDienThoai { get; set; }

        [Display(Name = "Ảnh đại diện")]
        [StringLength(255)]
        public string? Avatar { get; set; }

        [Display(Name = "Địa chỉ")]
        [StringLength(255)]
        public string? DiaChi { get; set; }

        [Display(Name = "Trạng thái")]
        public bool IsActive { get; set; } = true;

        [Display(Name = "Tài khoản hệ thống")]
        public bool IsSystemAccount { get; set; } = false;

        [Display(Name = "Ngày tạo")]
        public DateTime NgayTao { get; set; } = DateTime.Now;

        // Navigation properties
        public virtual ICollection<HopDong> HopDongKhachHang { get; set; } = new List<HopDong>();
        public virtual ICollection<HopDong> HopDongNguoiTao { get; set; } = new List<HopDong>();
        public virtual PhanQuyen? UserPermission { get; set; }
    }
}
