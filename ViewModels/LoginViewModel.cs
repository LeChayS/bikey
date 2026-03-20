using System.ComponentModel.DataAnnotations;

namespace bikey.ViewModels
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập email.")]
        [Display(Name = "Email")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu.")]
        [DataType(DataType.Password)]
        [Display(Name = "Mật khẩu")]
        public string MatKhau { get; set; } = string.Empty;

        public string? ReturnUrl { get; set; }
    }
}
