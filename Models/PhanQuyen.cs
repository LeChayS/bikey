using System.ComponentModel.DataAnnotations;

namespace bikey.Models
{
    public class PhanQuyen
    {
        [Key]
        public int Id { get; set; }

        public int UserId { get; set; }
        public virtual NguoiDung User { get; set; }

        // Quyền quản lý xe
        public bool CanViewXe { get; set; } = true;
        public bool CanCreateXe { get; set; } = false;
        public bool CanEditXe { get; set; } = false;
        public bool CanDeleteXe { get; set; } = false;

        // Quyền quản lý loại xe
        public bool CanViewLoaiXe { get; set; } = true;
        public bool CanCreateLoaiXe { get; set; } = false;
        public bool CanEditLoaiXe { get; set; } = false;
        public bool CanDeleteLoaiXe { get; set; } = false;

        // Quyền quản lý hợp đồng
        public bool CanViewHopDong { get; set; } = true;
        public bool CanCreateHopDong { get; set; } = false;
        public bool CanEditHopDong { get; set; } = false;
        public bool CanDeleteHopDong { get; set; } = false;
        public bool CanPrintHopDong { get; set; } = false;

        // Quyền quản lý hóa đơn
        public bool CanViewHoaDon { get; set; } = true;
        public bool CanCreateHoaDon { get; set; } = false;
        public bool CanEditHoaDon { get; set; } = false;
        public bool CanDeleteHoaDon { get; set; } = false;
        public bool CanPrintHoaDon { get; set; } = false;

        // Quyền quản lý người dùng
        public bool CanViewUser { get; set; } = false;
        public bool CanCreateUser { get; set; } = false;
        public bool CanEditUser { get; set; } = false;
        public bool CanDeleteUser { get; set; } = false;

        // Quyền báo cáo thống kê
        public bool CanViewBaoCao { get; set; } = false;
        public bool CanViewThongKe { get; set; } = false;
        public bool CanExportBaoCao { get; set; } = false;

        // Quyền đặt chỗ
        public bool CanDatCho { get; set; } = true;
        public bool CanViewDatCho { get; set; } = true;

        // Quyền quản lý hình ảnh xe
        public bool CanViewHinhAnhXe { get; set; } = true;
        public bool CanUploadHinhAnhXe { get; set; } = false;
        public bool CanEditHinhAnhXe { get; set; } = false;
        public bool CanDeleteHinhAnhXe { get; set; } = false;
    }
}
