using bikey.Models;

namespace bikey.Services
{
    /// <summary>
    /// Service interface for Invoice (HoaDon) business operations.
    /// Handles all invoice-related logic and queries.
    /// </summary>
    public interface IHoaDonService
    {
        /// <summary>
        /// Gets all invoices with related data.
        /// </summary>
        Task<List<HoaDon>> GetAllAsync();

        /// <summary>
        /// Gets an invoice by ID with related data.
        /// </summary>
        Task<HoaDon?> GetByIdAsync(int maHoaDon);

        /// <summary>
        /// Creates a new invoice.
        /// </summary>
        Task<HoaDon> CreateAsync(HoaDon hoaDon);

        /// <summary>
        /// Updates an existing invoice.
        /// </summary>
        Task<HoaDon> UpdateAsync(HoaDon hoaDon);

        /// <summary>
        /// Gets paginated and filtered invoices.
        /// </summary>
        Task<PaginatedResult<HoaDon>> GetPaginatedAsync(
            int pageNumber = 1,
            int pageSize = 10,
            string? searchString = null,
            DateTime? fromDate = null,
            DateTime? toDate = null);

        /// <summary>
        /// Gets invoice statistics.
        /// </summary>
        Task<HoaDonStatisticsDto> GetStatisticsAsync(DateTime? fromDate = null, DateTime? toDate = null);

        /// <summary>
        /// Gets count of invoices created today.
        /// </summary>
        Task<int> GetCountTodayAsync();

        /// <summary>
        /// Gets total revenue for a date range.
        /// </summary>
        Task<decimal> GetTotalRevenueAsync(DateTime? fromDate = null, DateTime? toDate = null);
    }

    /// <summary>
    /// Data transfer object for invoice statistics.
    /// </summary>
    public class HoaDonStatisticsDto
    {
        public int TotalCount { get; set; }
        public int TodayCount { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal TodayRevenue { get; set; }
        public int PaidCount { get; set; }
        public int UnpaidCount { get; set; }
    }
}
