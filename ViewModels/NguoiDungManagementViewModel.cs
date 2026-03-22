using System.ComponentModel.DataAnnotations;
using bikey.Models;

namespace bikey.ViewModels
{
    public class NguoiDungManagementViewModel
    {
        public IReadOnlyList<NguoiDung> Users { get; init; } = [];

        public CreateNguoiDungInput CreateUser { get; init; } = new();

        public EditNguoiDungInput EditUser { get; init; } = new();

        public RolePermissionMatrixViewModel RolePermissions { get; init; } = new();
    }

    public class CreateNguoiDungInput
    {
        [Required(ErrorMessage = "Vui lòng nhập họ và tên.")]
        public string HoTen { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập email.")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập số điện thoại.")]
        [RegularExpression(@"^\d{10,11}$", ErrorMessage = "Số điện thoại phải có từ 10 đến 11 chữ số.")]
        public string SoDienThoai { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu.")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Mật khẩu phải có ít nhất 6 ký tự.")]
        [DataType(DataType.Password)]
        public string MatKhau { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng xác nhận mật khẩu.")]
        [Compare(nameof(MatKhau), ErrorMessage = "Mật khẩu xác nhận không khớp.")]
        [DataType(DataType.Password)]
        public string XacNhanMatKhau { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng chọn quyền.")]
        public string VaiTro { get; set; } = "Khách hàng";
    }

    public class EditNguoiDungInput
    {
        [Required]
        public int Id { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập họ và tên.")]
        public string HoTen { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập email.")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập số điện thoại.")]
        [RegularExpression(@"^\d{10,11}$", ErrorMessage = "Số điện thoại phải có từ 10 đến 11 chữ số.")]
        public string SoDienThoai { get; set; } = string.Empty;

        public string? DiaChi { get; set; }

        [DataType(DataType.Password)]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Mật khẩu mới phải có ít nhất 6 ký tự.")]
        public string? MatKhauMoi { get; set; }

        [DataType(DataType.Password)]
        [Compare(nameof(MatKhauMoi), ErrorMessage = "Mật khẩu xác nhận không khớp.")]
        public string? XacNhanMatKhauMoi { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn quyền.")]
        public string VaiTro { get; set; } = "Khách hàng";

        public bool IsActive { get; set; } = true;
    }

    public class RolePermissionMatrixViewModel
    {
        public PermissionSetInput Admin { get; set; } = new();

        public PermissionSetInput Staff { get; set; } = new();

        public PermissionSetInput KhachHang { get; set; } = new();
    }

    public class UserPermissionEditorViewModel
    {
        public int UserId { get; set; }

        public string UserName { get; set; } = string.Empty;

        public string UserEmail { get; set; } = string.Empty;

        public string UserRole { get; set; } = string.Empty;

        public PermissionSetInput Permissions { get; set; } = new();
    }

    public class PermissionSetInput
    {
        public bool CanManageXe { get; set; }
        public bool CanViewXe { get; set; }
        public bool CanCreateXe { get; set; }
        public bool CanEditXe { get; set; }
        public bool CanDeleteXe { get; set; }

        public bool CanManageLoaiXe { get; set; }
        public bool CanViewLoaiXe { get; set; }
        public bool CanCreateLoaiXe { get; set; }
        public bool CanEditLoaiXe { get; set; }
        public bool CanDeleteLoaiXe { get; set; }

        public bool CanManageHopDong { get; set; }
        public bool CanViewHopDong { get; set; }
        public bool CanCreateHopDong { get; set; }
        public bool CanEditHopDong { get; set; }
        public bool CanDeleteHopDong { get; set; }
        public bool CanPrintHopDong { get; set; }

        public bool CanManageHoaDon { get; set; }
        public bool CanViewHoaDon { get; set; }
        public bool CanCreateHoaDon { get; set; }
        public bool CanEditHoaDon { get; set; }
        public bool CanDeleteHoaDon { get; set; }
        public bool CanPrintHoaDon { get; set; }

        public bool CanManageUser { get; set; }
        public bool CanViewUser { get; set; }
        public bool CanCreateUser { get; set; }
        public bool CanEditUser { get; set; }
        public bool CanDeleteUser { get; set; }

        public bool CanManageBanner { get; set; }
        public bool CanViewBanner { get; set; }
        public bool CanCreateBanner { get; set; }
        public bool CanEditBanner { get; set; }
        public bool CanDeleteBanner { get; set; }

        public bool CanViewBaoCao { get; set; }
        public bool CanViewThongKe { get; set; }
        public bool CanExportBaoCao { get; set; }

        public bool CanManageCart { get; set; }
        public bool CanViewCart { get; set; }
        public bool CanCheckout { get; set; }

        public bool CanDatCho { get; set; }
        public bool CanViewDatCho { get; set; }

        public bool CanManageHinhAnhXe { get; set; }
        public bool CanViewHinhAnhXe { get; set; }
        public bool CanUploadHinhAnhXe { get; set; }
        public bool CanEditHinhAnhXe { get; set; }
        public bool CanDeleteHinhAnhXe { get; set; }
    }

    public class PermissionGroupMetadata
    {
        public string Title { get; init; } = string.Empty;

        public string Icon { get; init; } = string.Empty;

        public IReadOnlyList<PermissionItemMetadata> Items { get; init; } = [];

        public static IReadOnlyList<PermissionGroupMetadata> All { get; } =
        [
            new PermissionGroupMetadata
            {
                Title = "Quản lý xe",
                Icon = "bi-scooter",
                Items =
                [
                    new PermissionItemMetadata("CanViewXe", "Xem"),
                    new PermissionItemMetadata("CanCreateXe", "Tạo xe mới"),
                    new PermissionItemMetadata("CanEditXe", "Sửa thông tin xe"),
                    new PermissionItemMetadata("CanDeleteXe", "Xóa xe")
                ]
            },
            new PermissionGroupMetadata
            {
                Title = "Quản lý loại xe",
                Icon = "bi-tags",
                Items =
                [
                    new PermissionItemMetadata("CanViewLoaiXe", "Xem"),
                    new PermissionItemMetadata("CanCreateLoaiXe", "Tạo loại xe mới"),
                    new PermissionItemMetadata("CanEditLoaiXe", "Sửa thông tin loại xe"),
                    new PermissionItemMetadata("CanDeleteLoaiXe", "Xóa loại xe")
                ]
            },
            new PermissionGroupMetadata
            {
                Title = "Quản lý hợp đồng",
                Icon = "bi-receipt",
                Items =
                [
                    new PermissionItemMetadata("CanViewHopDong", "Xem"),
                    new PermissionItemMetadata("CanCreateHopDong", "Tạo hợp đồng mới"),
                    new PermissionItemMetadata("CanEditHopDong", "Sửa thông tin hợp đồng"),
                    new PermissionItemMetadata("CanDeleteHopDong", "Xóa hợp đồng"),
                    new PermissionItemMetadata("CanPrintHopDong", "In hợp đồng")
                ]
            },
            new PermissionGroupMetadata
            {
                Title = "Quản lý hóa đơn",
                Icon = "bi-receipt-cutoff",
                Items =
                [
                    new PermissionItemMetadata("CanViewHoaDon", "Xem"),
                    new PermissionItemMetadata("CanCreateHoaDon", "Tạo hóa đơn mới"),
                    new PermissionItemMetadata("CanEditHoaDon", "Sửa thông tin hóa đơn"),
                    new PermissionItemMetadata("CanDeleteHoaDon", "Xóa hóa đơn"),
                    new PermissionItemMetadata("CanPrintHoaDon", "In hóa đơn")
                ]
            },
            new PermissionGroupMetadata
            {
                Title = "Quản lý banner",
                Icon = "bi-badge-ad",
                Items =
                [
                    new PermissionItemMetadata("CanViewBanner", "Xem"),
                    new PermissionItemMetadata("CanCreateBanner", "Tạo banner mới"),
                    new PermissionItemMetadata("CanEditBanner", "Sửa thông tin banner"),
                    new PermissionItemMetadata("CanDeleteBanner", "Xóa banner")
                ]
            },
            new PermissionGroupMetadata
            {
                Title = "Quản lý người dùng",
                Icon = "bi-people",
                Items =
                [
                    new PermissionItemMetadata("CanViewUser", "Xem"),
                    new PermissionItemMetadata("CanCreateUser", "Tạo người dùng mới"),
                    new PermissionItemMetadata("CanEditUser", "Sửa thông tin người dùng"),
                    new PermissionItemMetadata("CanDeleteUser", "Xóa người dùng")
                ]
            },
            new PermissionGroupMetadata
            {
                Title = "Quản lý báo cáo & thống kê",
                Icon = "bi-bar-chart-line",
                Items =
                [
                    new PermissionItemMetadata("CanViewBaoCao", "Xem báo cáo"),
                    new PermissionItemMetadata("CanViewThongKe", "Xem thống kê"),
                    new PermissionItemMetadata("CanExportBaoCao", "In báo cáo")
                ]
            },
            new PermissionGroupMetadata
            {
                Title = "Quản lý hình ảnh xe",
                Icon = "bi-images",
                Items =
                [
                    new PermissionItemMetadata("CanViewHinhAnhXe", "Xem"),
                    new PermissionItemMetadata("CanUploadHinhAnhXe", "Thêm hình ảnh"),
                    new PermissionItemMetadata("CanEditHinhAnhXe", "Sửa hình ảnh"),
                    new PermissionItemMetadata("CanDeleteHinhAnhXe", "Xóa hình ảnh")
                ]
            }
        ];
    }

    public record PermissionItemMetadata(string PropertyName, string Label);
}
