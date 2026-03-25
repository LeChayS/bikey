using bikey.Common;
using bikey.Models;
using bikey.Repository;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace bikey.Services
{
    public class XeService : IXeService
    {
        private readonly BikeyDbContext _context;
        private readonly IWebHostEnvironment _env;

        public XeService(BikeyDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        public async Task<Xe> CreateAsync(Xe xe, IFormFile hinhAnh, IEnumerable<IFormFile>? hinhAnhKhac)
        {
            // Save Xe entity to generate MaXe
            _context.Xe.Add(xe);
            await _context.SaveChangesAsync();

            var uploadDir = Path.Combine(_env.WebRootPath, "images", "xe");
            Directory.CreateDirectory(uploadDir);

            async Task<string> SaveImageAsync(IFormFile file)
            {
                var ext = Path.GetExtension(file.FileName);
                var fileName = $"{Guid.NewGuid():N}{ext}";
                var fullPath = Path.Combine(uploadDir, fileName);

                await using var stream = new FileStream(fullPath, FileMode.Create);
                await file.CopyToAsync(stream);

                return Path.Combine("images", "xe", fileName).Replace("\\", "/");
            }

            // Save main image
            var mainImagePath = await SaveImageAsync(hinhAnh);
            var mainImage = new HinhAnhXe
            {
                MaXe = xe.MaXe,
                TenFile = mainImagePath,
                LaAnhChinh = true,
                ThuTu = 1
            };
            _context.HinhAnhXe.Add(mainImage);

            // Save other images (enforce max 10 total including main)
            const int maxTotalImages = 10;
            if (hinhAnhKhac != null)
            {
                int order = 2;
                var filesToAdd = hinhAnhKhac.Take(maxTotalImages - 1);
                foreach (var file in filesToAdd)
                {
                    var imagePath = await SaveImageAsync(file);
                    var otherImage = new HinhAnhXe
                    {
                        MaXe = xe.MaXe,
                        TenFile = imagePath,
                        LaAnhChinh = false,
                        ThuTu = order++
                    };
                    _context.HinhAnhXe.Add(otherImage);
                }
            }

            await _context.SaveChangesAsync();
            return xe;
        }

        public async Task<List<Xe>> GetAllAsync(bool excludeDeleted = true)
        {
            var query = _context.Xe
                .AsNoTracking()
                .Include(x => x.LoaiXe)
                .Include(x => x.HinhAnhXes)
                .AsQueryable();

            if (excludeDeleted)
            {
                query = query.Where(x => x.TrangThai != StatusConstants.XeStatus.DaXoa);
            }

            return await query.OrderByDescending(x => x.MaXe).ToListAsync();
        }

        public async Task<Xe?> GetByIdAsync(int maXe)
        {
            return await _context.Xe
                .AsNoTracking()
                .Include(x => x.LoaiXe)
                .Include(x => x.HinhAnhXes)
                .FirstOrDefaultAsync(x => x.MaXe == maXe);
        }

        public async Task<Xe> UpdateAsync(Xe xe, IFormFile? hinhAnh, IEnumerable<IFormFile>? hinhAnhKhac, IEnumerable<int>? removeImageIds)
        {
            var xeToUpdate = await _context.Xe.Include(x => x.HinhAnhXes).FirstOrDefaultAsync(x => x.MaXe == xe.MaXe);
            if (xeToUpdate == null)
            {
                throw new Exception("Không tìm thấy xe để cập nhật.");
            }

            // Update properties
            _context.Entry(xeToUpdate).CurrentValues.SetValues(xe);

            var uploadDir = Path.Combine(_env.WebRootPath, "images", "xe");
            Directory.CreateDirectory(uploadDir);

            // Remove images
            if (removeImageIds != null)
            {
                var imagesToRemove = xeToUpdate.HinhAnhXes.Where(img => removeImageIds.Contains(img.MaHinhAnh)).ToList();
                foreach (var image in imagesToRemove)
                {
                    DeleteXeImageFileIfExists(image.TenFile);
                    _context.HinhAnhXe.Remove(image);
                }
            }

            async Task<string> SaveImageAsync(IFormFile file)
            {
                var ext = Path.GetExtension(file.FileName);
                var fileName = $"{Guid.NewGuid():N}{ext}";
                var fullPath = Path.Combine(uploadDir, fileName);

                await using var stream = new FileStream(fullPath, FileMode.Create);
                await file.CopyToAsync(stream);

                return Path.Combine("images", "xe", fileName).Replace("\\", "/");
            }

            // Add new main image
            if (hinhAnh != null)
            {
                var mainImage = xeToUpdate.HinhAnhXes.FirstOrDefault(img => img.LaAnhChinh);
                if (mainImage != null)
                {
                    DeleteXeImageFileIfExists(mainImage.TenFile);
                    mainImage.TenFile = await SaveImageAsync(hinhAnh);
                }
                else
                {
                    var newMainImage = new HinhAnhXe
                    {
                        MaXe = xeToUpdate.MaXe,
                        TenFile = await SaveImageAsync(hinhAnh),
                        LaAnhChinh = true,
                        ThuTu = 1
                    };
                    xeToUpdate.HinhAnhXes.Add(newMainImage);
                }
            }

            // Add new other images (enforce max 10 total)
            const int maxTotalImages = 10;
            if (hinhAnhKhac != null)
            {
                int maxOrder = xeToUpdate.HinhAnhXes.Any() ? xeToUpdate.HinhAnhXes.Max(h => h.ThuTu) : 0;
                var filesToAdd = hinhAnhKhac.Take(Math.Max(0, maxTotalImages - xeToUpdate.HinhAnhXes.Count));
                foreach (var file in filesToAdd)
                {
                    var imagePath = await SaveImageAsync(file);
                    var otherImage = new HinhAnhXe
                    {
                        MaXe = xeToUpdate.MaXe,
                        TenFile = imagePath,
                        LaAnhChinh = false,
                        ThuTu = ++maxOrder
                    };
                    xeToUpdate.HinhAnhXes.Add(otherImage);
                }
            }

            // Ensure there is a main image
            if (xeToUpdate.HinhAnhXes.Any() && !xeToUpdate.HinhAnhXes.Any(h => h.LaAnhChinh))
            {
                xeToUpdate.HinhAnhXes.OrderBy(h => h.ThuTu).First().LaAnhChinh = true;
            }


            await _context.SaveChangesAsync();
            return xeToUpdate;
        }

        private void DeleteXeImageFileIfExists(string? tenFile)
        {
            if (string.IsNullOrWhiteSpace(tenFile)) return;

            var normalized = tenFile.Replace("/", Path.DirectorySeparatorChar.ToString())
                .Replace("\\", Path.DirectorySeparatorChar.ToString())
                .TrimStart(Path.DirectorySeparatorChar);

            var fullPath = Path.Combine(_env.WebRootPath, normalized);
            if (System.IO.File.Exists(fullPath))
            {
                System.IO.File.Delete(fullPath);
            }
        }


        public async Task DeleteAsync(int maXe)
        {
            var xe = await _context.Xe.FindAsync(maXe);
            if (xe != null)
            {
                xe.TrangThai = StatusConstants.XeStatus.DaXoa;
                _context.Xe.Update(xe);
                await _context.SaveChangesAsync();
            }
        }


        public async Task RestoreAsync(int maXe)
        {
            var xe = await _context.Xe.FindAsync(maXe);
            if (xe != null)
            {
                xe.TrangThai = StatusConstants.XeStatus.SanSang;
                _context.Xe.Update(xe);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<XeStatisticsDto> GetStatisticsAsync()
        {
            var xeList = await GetAllAsync(excludeDeleted: true);

            return new XeStatisticsDto
            {
                TotalCount = xeList.Count,
                AvailableCount = xeList.Count(x => x.TrangThai == StatusConstants.XeStatus.SanSang),
                RentedCount = xeList.Count(x => x.TrangThai == StatusConstants.XeStatus.DangThue),
                MaintenanceCount = xeList.Count(x => x.TrangThai == StatusConstants.XeStatus.BaoTri),
                DamagedCount = xeList.Count(x => x.TrangThai == StatusConstants.XeStatus.HuHong)
            };
        }

        public async Task<List<Xe>> FilterAsync(
            string? searchString = null,
            string? loaiXe = null,
            string? hangXe = null,
            string? trangThai = null,
            bool showDeleted = false)
        {
            var query = _context.Xe
                .AsNoTracking()
                .Include(x => x.LoaiXe)
                .Include(x => x.HinhAnhXes)
                .AsQueryable();

            if (!showDeleted)
            {
                query = query.Where(x => x.TrangThai != StatusConstants.XeStatus.DaXoa);
            }

            if (!string.IsNullOrWhiteSpace(searchString))
            {
                var s = searchString.Trim();
                query = query.Where(x => x.TenXe.Contains(s) || x.BienSoXe.Contains(s));
            }

            if (!string.IsNullOrWhiteSpace(loaiXe))
            {
                query = query.Where(x => x.LoaiXe != null && x.LoaiXe.TenLoaiXe == loaiXe);
            }

            if (!string.IsNullOrWhiteSpace(hangXe))
            {
                query = query.Where(x => x.HangXe.Contains(hangXe));
            }

            if (!string.IsNullOrWhiteSpace(trangThai))
            {
                query = query.Where(x => x.TrangThai == trangThai);
            }

            return await query.OrderByDescending(x => x.MaXe).ToListAsync();
        }

        public async Task<List<Xe>> GetAvailableForRentalAsync()
        {
            return await _context.Xe
                .AsNoTracking()
                .Include(x => x.LoaiXe)
                .Include(x => x.HinhAnhXes)
                .Where(x => x.TrangThai == StatusConstants.XeStatus.SanSang)
                .OrderByDescending(x => x.MaXe)
                .ToListAsync();
        }

        public async Task<bool> CheckXeContractHistoryAsync(int maXe)
        {
            return await _context.ChiTietHopDong.AsNoTracking().AnyAsync(x => x.MaXe == maXe);
        }

        public async Task<List<string>> GetHangXeListAsync()
        {
            return await _context.Xe
                .AsNoTracking()
                .Where(x => !string.IsNullOrEmpty(x.HangXe))
                .Select(x => x.HangXe!)
                .Distinct()
                .OrderBy(x => x)
                .ToListAsync();
        }

        public async Task<List<string>> GetTrangThaiListAsync()
        {
            return await _context.Xe
                .AsNoTracking()
                .Where(x => !string.IsNullOrEmpty(x.TrangThai))
                .Select(x => x.TrangThai!)
                .Distinct()
                .OrderBy(x => x)
                .ToListAsync();
        }

        public async Task<int> GetCountByStatusAsync(string status)
        {
            return await _context.Xe
                .AsNoTracking()
                .CountAsync(x => x.TrangThai == status);
        }
    }
}
