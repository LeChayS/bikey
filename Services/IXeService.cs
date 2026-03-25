using bikey.Models;
using Microsoft.AspNetCore.Http;

namespace bikey.Services
{
    /// <summary>
    /// Service interface for Vehicle (Xe) business operations.
    /// Consolidates all vehicle-related logic and database operations.
    /// </summary>
    public interface IXeService
    {
        /// <summary>
        /// Gets all vehicles with optional filtering.
        /// </summary>
        Task<List<Xe>> GetAllAsync(bool excludeDeleted = true);

        /// <summary>
        /// Gets a vehicle by ID with related data.
        /// </summary>
        Task<Xe?> GetByIdAsync(int maXe);

        /// <summary>
        /// Creates a new vehicle with uploaded images.
        /// </summary>
        Task<Xe> CreateAsync(Xe xe, IFormFile hinhAnh, IEnumerable<IFormFile>? hinhAnhKhac);

        /// <summary>
        /// Updates an existing vehicle, including its images.
        /// </summary>
        Task<Xe> UpdateAsync(Xe xe, IFormFile? hinhAnh, IEnumerable<IFormFile>? hinhAnhKhac, IEnumerable<int>? removeImageIds);


        /// <summary>
        /// Soft deletes a vehicle by setting status to "Đã xóa".
        /// </summary>
        Task DeleteAsync(int maXe);

        /// <summary>
        /// Restores a soft-deleted vehicle.
        /// </summary>
        Task RestoreAsync(int maXe);

        /// <summary>
        /// Gets vehicle statistics.
        /// </summary>
        Task<XeStatisticsDto> GetStatisticsAsync();

        /// <summary>
        /// Filters vehicles based on criteria.
        /// </summary>
        Task<List<Xe>> FilterAsync(
            string? searchString = null,
            string? loaiXe = null,
            string? hangXe = null,
            string? trangThai = null,
            bool showDeleted = false);

        /// <summary>
        /// Gets vehicles available for rental.
        /// </summary>
        Task<List<Xe>> GetAvailableForRentalAsync();

        /// <summary>
        /// Checks if a vehicle has any contract history.
        /// </summary>
        Task<bool> CheckXeContractHistoryAsync(int maXe);

        /// <summary>
        /// Gets a distinct list of vehicle brands (HangXe).
        /// </summary>
        Task<List<string>> GetHangXeListAsync();

        /// <summary>
        /// Gets a distinct list of vehicle statuses (TrangThai).
        /// </summary>
        Task<List<string>> GetTrangThaiListAsync();

        /// <summary>
        /// Gets the count of vehicles by status.
        /// </summary>
        Task<int> GetCountByStatusAsync(string status);
    }

    /// <summary>
    /// Data transfer object for vehicle statistics.
    /// </summary>
    public class XeStatisticsDto
    {
        public int TotalCount { get; set; }
        public int AvailableCount { get; set; }
        public int RentedCount { get; set; }
        public int MaintenanceCount { get; set; }
        public int DamagedCount { get; set; }
    }
}
