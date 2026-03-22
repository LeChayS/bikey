using System.ComponentModel.DataAnnotations;

namespace bikey.ViewModels
{
    public class ForgotPasswordViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập email.")]
        [Display(Name = "Email")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu mới.")]
        [Display(Name = "Mật khẩu mới")]
        [DataType(DataType.Password)]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Mật khẩu phải có ít nhất 6 ký tự.")]
        public string MatKhauMoi { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng xác nhận mật khẩu mới.")]
        [Display(Name = "Xác nhận mật khẩu mới")]
        [DataType(DataType.Password)]
        [Compare(nameof(MatKhauMoi), ErrorMessage = "Mật khẩu xác nhận không khớp.")]
        public string XacNhanMatKhauMoi { get; set; } = string.Empty;
    }
}
