namespace bikey.Common
{
    /// <summary>
    /// Centralized status constants for domain entities.
    /// Ensures consistency and makes it easy to find and update status values.
    /// </summary>
    public static class StatusConstants
    {
        /// <summary>
        /// Vehicle (Xe) status constants
        /// </summary>
        public static class XeStatus
        {
            public const string SanSang = "Sẵn sàng"; // Ready for rental
            public const string DangThue = "Đang thuê"; // Currently rented
            public const string BaoTri = "Bảo trì"; // Under maintenance
            public const string HuHong = "Hư hỏng"; // Damaged
            public const string Mat = "Mất"; // Lost/Stolen
            public const string DaXoa = "Đã xóa"; // Deleted

            public static List<string> AllStatuses => new()
            {
                SanSang, DangThue, BaoTri, HuHong, Mat, DaXoa
            };
        }

        /// <summary>
        /// Booking (DatCho) status constants
        /// </summary>
        public static class DatChoStatus
        {
            public const string ChoXacNhan = "Chờ xác nhận"; // Pending confirmation
            public const string DaXuLy = "Đã xử lý"; // Processed
            public const string HoanThanh = "Hoàn thành"; // Completed
            public const string Huy = "Hủy"; // Cancelled

            public static List<string> AllStatuses => new()
            {
                ChoXacNhan, DaXuLy, HoanThanh, Huy
            };
        }

        /// <summary>
        /// Contract (HopDong) status constants
        /// </summary>
        public static class HopDongStatus
        {
            public const string DangThue = "Đang thuê"; // Active rental
            public const string HoanThanh = "Hoàn thành"; // Completed
            public const string Huy = "Hủy"; // Cancelled

            public static List<string> AllStatuses => new()
            {
                DangThue, HoanThanh, Huy
            };
        }

        /// <summary>
        /// Invoice (HoaDon) status constants
        /// </summary>
        public static class HoaDonStatus
        {
            public const string DaThanhToan = "Đã thanh toán"; // Paid
            public const string ChuaThanhToan = "Chưa thanh toán"; // Unpaid
            public const string Huy = "Hủy"; // Cancelled

            public static List<string> AllStatuses => new()
            {
                DaThanhToan, ChuaThanhToan, Huy
            };
        }

        /// <summary>
        /// User role status constants
        /// </summary>
        public static class RoleStatus
        {
            public const string Admin = "Admin";
            public const string Staff = "Staff";
            public const string Customer = "Customer";

            public static List<string> AllRoles => new()
            {
                Admin, Staff, Customer
            };
        }
    }
}
