using bikey.Models;
using bikey.Repository;
using Microsoft.EntityFrameworkCore;

namespace bikey.Services
{
    /// <summary>
    /// Service for Vehicle Type (LoaiXe) business operations.
    /// </summary>
    public class LoaiXeService : ILoaiXeService
    {
        private readonly BikeyDbContext _context;

        public LoaiXeService(BikeyDbContext context)
        {
            _context = context;
        }

        public async Task<List<LoaiXe>> GetAllAsync()
        {
            return await _context.LoaiXe
                .AsNoTracking()
                .OrderByDescending(x => x.NgayTao)
                .ToListAsync();
        }

        public async Task<LoaiXe?> GetByIdAsync(int maLoaiXe)
        {
            return await _context.LoaiXe
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.MaLoaiXe == maLoaiXe);
        }

        public async Task<LoaiXe> CreateAsync(string tenLoaiXe)
        {
            var loaiXe = new LoaiXe
            {
                TenLoaiXe = tenLoaiXe.Trim(),
                NgayTao = DateTime.Now,
                NgayCapNhat = null
            };

            _context.LoaiXe.Add(loaiXe);
            await _context.SaveChangesAsync();
            return loaiXe;
        }

        public async Task<LoaiXe> UpdateAsync(int maLoaiXe, string tenLoaiXe)
        {
            var loaiXe = await _context.LoaiXe.FindAsync(maLoaiXe);
            if (loaiXe == null)
                throw new KeyNotFoundException($"Loại xe với ID {maLoaiXe} không tìm thấy.");

            loaiXe.TenLoaiXe = tenLoaiXe.Trim();
            loaiXe.NgayCapNhat = DateTime.Now;

            _context.LoaiXe.Update(loaiXe);
            await _context.SaveChangesAsync();
            return loaiXe;
        }

        public async Task DeleteAsync(int maLoaiXe)
        {
            var loaiXe = await _context.LoaiXe.FindAsync(maLoaiXe);
            if (loaiXe != null)
            {
                _context.LoaiXe.Remove(loaiXe);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> ExistsAsync(string tenLoaiXe)
        {
            var normalized = tenLoaiXe.Trim().ToLower();
            return await _context.LoaiXe
                .AsNoTracking()
                .AnyAsync(x => x.TenLoaiXe.ToLower() == normalized);
        }

        public async Task<bool> HasVehiclesAsync(int maLoaiXe)
        {
            return await _context.Xe
                .AsNoTracking()
                .AnyAsync(x => x.MaLoaiXe == maLoaiXe);
        }
    }
}
