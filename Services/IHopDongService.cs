using bikey.Models;

namespace bikey.Services
{
    /// <summary>
    /// Service interface for Contract (HopDong) business operations.
    /// Consolidates all contract-related logic and calculations.
    /// </summary>
    public interface IHopDongService
    {
        /// <summary>
        /// Gets all contracts with related data.
        /// </summary>
        Task<List<HopDong>> GetAllAsync();

        /// <summary>
        /// Gets a contract by ID with related data.
        /// </summary>
        Task<HopDong?> GetByIdAsync(int maHopDong);

        /// <summary>
        /// Creates a new contract.
        /// </summary>
        Task<HopDong> CreateAsync(HopDong hopDong);

        /// <summary>
        /// Updates an existing contract.
        /// </summary>
        Task<HopDong> UpdateAsync(HopDong hopDong);

        /// <summary>
        /// Gets paginated contracts with optional filtering.
        /// </summary>
        Task<PaginatedResult<HopDong>> GetPaginatedAsync(
            int pageNumber = 1,
            int pageSize = 10,
            string? trangThai = null,
            string? tuKhoa = null);

        /// <summary>
        /// Gets contracts awaiting processing.
        /// </summary>
        Task<List<HopDong>> GetPendingAsync();

        /// <summary>
        /// Gets contract statistics.
        /// </summary>
        Task<HopDongStatisticsDto> GetStatisticsAsync();

        /// <summary>
        /// Calculates the deposit amount for a contract.
        /// </summary>
        decimal CalculateDeposit(decimal rentalPrice);

        /// <summary>
        /// Calculates rental duration and total price.
        /// </summary>
        HopDongCalculationDto CalculateRentalPrice(
            DateTime startDate,
            DateTime endDate,
            decimal dailyRate);

        /// <summary>
        /// Gets count of bookings pending confirmation.
        /// </summary>
        Task<int> GetCountPendingBookingsAsync();

        /// <summary>
        /// Gets count of new bookings from today.
        /// </summary>
        Task<int> GetCountNewBookingsAsync();

        Task<List<DatCho>> GetDonChoXuLyAsync(string? searchString, DateTime? tuNgay, DateTime? denNgay);
        Task XuLyDonAsync(int datChoId, int? maNguoiTao);
        Task HuyDonAsync(int datChoId);
        Task<HopDong?> GetChiTietAsync(int maHopDong);
        Task TraXeAsync(int maHopDong, DateTime ngayTraThucTe, decimal phuPhi, string tinhTrangXe, string? loaiThietHai, string? moTaThietHai, decimal chiPhiThietHai, string? ghiChu, int maNguoiTao);
        Task<List<DatCho>> TimPhieuDatChoAsync(string soDienThoai);
        Task<List<HopDong>> GetLichSuKhachHangAsync(string soDienThoai);
    }

    /// <summary>
    /// Data transfer object for paginated results.
    /// </summary>
    public class PaginatedResult<T>
    {
        public List<T> Items { get; set; } = new();
        public int Total { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => (Total + PageSize - 1) / PageSize;
    }

    /// <summary>
    /// Data transfer object for contract statistics.
    /// </summary>
    public class HopDongStatisticsDto
    {
        public int TotalPending { get; set; }
        public int TotalNewToday { get; set; }
        public int TotalActive { get; set; }
        public int TotalCompleted { get; set; }
    }

    /// <summary>
    /// Data transfer object for rental price calculations.
    /// </summary>
    public class HopDongCalculationDto
    {
        public int Days { get; set; }
        public decimal DailyRate { get; set; }
        public decimal TotalPrice { get; set; }
        public decimal Deposit { get; set; }
    }
}
