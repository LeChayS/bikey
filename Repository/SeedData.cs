using bikey.Models;
using Microsoft.EntityFrameworkCore;

namespace bikey.Repository
{
    public class SeedData
    {
        public static void EnsurePopulated(IApplicationBuilder app)
        {
            using (var scope = app.ApplicationServices.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<BikeyDbContext>();
                if (context.Database.GetPendingMigrations().Any())
                {
                    context.Database.Migrate();
                }
                if (!context.LoaiXe.Any())
                {
                    context.LoaiXe.AddRange(
                        new LoaiXe
                        {
                            TenLoaiXe = "Xe Máy",
                            NgayTao = DateTime.Now,
                        },
                        new LoaiXe
                        {
                            TenLoaiXe = "Xe Ô Tô",
                            NgayTao = DateTime.Now,
                        },
                        new LoaiXe
                        {
                            TenLoaiXe = "Xe Tải",
                            NgayTao = DateTime.Now,
                        },
                        new LoaiXe
                        {
                            TenLoaiXe = "Xe Bán Tải",
                            NgayTao = DateTime.Now,
                        }
                    );
                    context.SaveChanges();
                }

                if (!context.NguoiDung.Any())
                {
                    context.NguoiDung.AddRange(
                        new NguoiDung
                        {
                            Ten = "admin",
                            Email = "admin@gmail.com",
                            MatKhau = "123456",
                            VaiTro = "Admin",
                            SoDienThoai = "0987654321",
                            IsActive = true,
                            NgayTao = DateTime.Now,
                        },
                        new NguoiDung
                        {
                            Ten = "tram",
                            Email = "tram@gmail.com",
                            MatKhau = "123456",
                            VaiTro = "User",
                            SoDienThoai = "0987654322",
                            IsActive = true,
                            NgayTao = DateTime.Now,
                            DiaChi = "39 Điện Biên Phủ, Hồ Chí Minh"
                        },
                        new NguoiDung
                        {
                            Ten = "staff",
                            Email = "staff@gmail.com",
                            MatKhau = "123456",
                            VaiTro = "Staff",
                            SoDienThoai = "0987654323",
                            IsActive = true,
                            NgayTao = DateTime.Now,
                        }
                    ); context.SaveChanges();

                    context.PhanQuyen.AddRange(
                        new PhanQuyen
                        {
                            UserId = 1,

                            CanManageXe = true,
                            CanViewXe = true,
                            CanCreateXe = true,
                            CanEditXe = true,
                            CanDeleteXe = true,

                            CanManageLoaiXe = true,
                            CanViewLoaiXe = true,
                            CanCreateLoaiXe = true,
                            CanEditLoaiXe = true,
                            CanDeleteLoaiXe = true,

                            CanManageHopDong = true,
                            CanViewHopDong = true,
                            CanCreateHopDong = true,
                            CanEditHopDong = true,
                            CanDeleteHopDong = true,
                            CanPrintHopDong = true,

                            CanManageHoaDon = true,
                            CanViewHoaDon = true,
                            CanCreateHoaDon = true,
                            CanEditHoaDon = true,
                            CanDeleteHoaDon = true,
                            CanPrintHoaDon = true,

                            CanManageUser = true,
                            CanViewUser = true,
                            CanCreateUser = true,
                            CanEditUser = true,
                            CanDeleteUser = true,

                            CanViewBaoCao = true,
                            CanViewThongKe = true,
                            CanExportBaoCao = true,

                            CanDatCho = true,
                            CanViewDatCho = true,

                            CanManageCart = true,
                            CanViewCart = true,
                            CanCheckout = true,

                            CanManageHinhAnhXe = true,
                            CanViewHinhAnhXe = true,
                            CanUploadHinhAnhXe = true,
                            CanEditHinhAnhXe = true,
                            CanDeleteHinhAnhXe = true,
                        },
                        new PhanQuyen
                        {
                            UserId = 3,

                            CanManageXe = true,
                            CanViewXe = true,
                            CanCreateXe = true,
                            CanEditXe = true,
                            CanDeleteXe = true,

                            CanManageLoaiXe = true,
                            CanViewLoaiXe = true,
                            CanCreateLoaiXe = true,
                            CanEditLoaiXe = true,
                            CanDeleteLoaiXe = false,

                            CanManageHopDong = true,
                            CanViewHopDong = true,
                            CanCreateHopDong = true,
                            CanEditHopDong = true,
                            CanDeleteHopDong = false,
                            CanPrintHopDong = true,

                            CanManageHoaDon = true,
                            CanViewHoaDon = true,
                            CanCreateHoaDon = true,
                            CanEditHoaDon = true,
                            CanDeleteHoaDon = false,
                            CanPrintHoaDon = true,

                            CanManageUser = true,
                            CanViewUser = true,
                            CanCreateUser = true,
                            CanEditUser = true,
                            CanDeleteUser = false,

                            CanViewBaoCao = true,
                            CanViewThongKe = true,
                            CanExportBaoCao = true,

                            CanDatCho = true,
                            CanViewDatCho = true,

                            CanManageCart = true,
                            CanViewCart = true,
                            CanCheckout = true,

                            CanManageHinhAnhXe = true,
                            CanViewHinhAnhXe = true,
                            CanUploadHinhAnhXe = true,
                            CanEditHinhAnhXe = true,
                            CanDeleteHinhAnhXe = true,
                        }
                    ); context.SaveChanges();
                }
            }
        }
    }
}
