using bikey.Models;

namespace bikey.Services
{
    /// <summary>
    /// Service interface for Vehicle Type (LoaiXe) business operations.
    /// </summary>
    public interface ILoaiXeService
    {
        /// <summary>
        /// Gets all vehicle types.
        /// </summary>
        Task<List<LoaiXe>> GetAllAsync();

        /// <summary>
        /// Gets a vehicle type by ID.
        /// </summary>
        Task<LoaiXe?> GetByIdAsync(int maLoaiXe);

        /// <summary>
        /// Creates a new vehicle type.
        /// </summary>
        Task<LoaiXe> CreateAsync(string tenLoaiXe);

        /// <summary>
        /// Updates an existing vehicle type.
        /// </summary>
        Task<LoaiXe> UpdateAsync(int maLoaiXe, string tenLoaiXe);

        /// <summary>
        /// Deletes a vehicle type.
        /// </summary>
        Task DeleteAsync(int maLoaiXe);

        /// <summary>
        /// Checks if a vehicle type name already exists.
        /// </summary>
        Task<bool> ExistsAsync(string tenLoaiXe);
        Task<bool> HasVehiclesAsync(int maLoaiXe);
    }
}
