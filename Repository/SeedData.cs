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

                if (!context.Xe.Any())
                {
                    var maLoaiXeMay = context.LoaiXe
                        .Where(loaiXe => loaiXe.TenLoaiXe == "Xe Máy")
                        .Select(loaiXe => loaiXe.MaLoaiXe)
                        .First();

                    context.Xe.AddRange(
                        new Xe
                        {
                            TenXe = "Honda Air Blade 160",
                            BienSoXe = "59A3-16001",
                            HangXe = "Honda",
                            DongXe = "Air Blade 160",
                            TrangThai = "Sẵn sàng",
                            GiaThue = 180000,
                            GiaTriXe = 55000000,
                            MaLoaiXe = maLoaiXeMay
                        },
                        new Xe
                        {
                            TenXe = "Honda Vision",
                            BienSoXe = "59A3-14002",
                            HangXe = "Honda",
                            DongXe = "Vision",
                            TrangThai = "Sẵn sàng",
                            GiaThue = 140000,
                            GiaTriXe = 38000000,
                            MaLoaiXe = maLoaiXeMay
                        },
                        new Xe
                        {
                            TenXe = "Yamaha Janus",
                            BienSoXe = "59A3-15003",
                            HangXe = "Yamaha",
                            DongXe = "Janus",
                            TrangThai = "Sẵn sàng",
                            GiaThue = 150000,
                            GiaTriXe = 36000000,
                            MaLoaiXe = maLoaiXeMay
                        },
                        new Xe
                        {
                            TenXe = "Honda Wave Alpha",
                            BienSoXe = "59A3-11004",
                            HangXe = "Honda",
                            DongXe = "Wave Alpha",
                            TrangThai = "Sẵn sàng",
                            GiaThue = 110000,
                            GiaTriXe = 21000000,
                            MaLoaiXe = maLoaiXeMay
                        },
                        new Xe
                        {
                            TenXe = "Honda Future 125",
                            BienSoXe = "59A3-13005",
                            HangXe = "Honda",
                            DongXe = "Future 125",
                            TrangThai = "Sẵn sàng",
                            GiaThue = 130000,
                            GiaTriXe = 34000000,
                            MaLoaiXe = maLoaiXeMay
                        },
                        new Xe
                        {
                            TenXe = "Yamaha Exciter 155",
                            BienSoXe = "59A3-22006",
                            HangXe = "Yamaha",
                            DongXe = "Exciter 155",
                            TrangThai = "Sẵn sàng",
                            GiaThue = 220000,
                            GiaTriXe = 52000000,
                            MaLoaiXe = maLoaiXeMay
                        }
                    ); context.SaveChanges();

                    var maXeExciter = context.Xe.First(x => x.TenXe == "Yamaha Exciter 155").MaXe;
                    var maXeFuture = context.Xe.First(x => x.TenXe == "Honda Future 125").MaXe;
                    var maXeWave = context.Xe.First(x => x.TenXe == "Honda Wave Alpha").MaXe;
                    var maXeJanus = context.Xe.First(x => x.TenXe == "Yamaha Janus").MaXe;
                    var maXeVision = context.Xe.First(x => x.TenXe == "Honda Vision").MaXe;
                    var maXeAirBlade = context.Xe.First(x => x.TenXe == "Honda Air Blade 160").MaXe;

                    context.HinhAnhXe.AddRange(
                        new HinhAnhXe  {
                            MaXe = maXeExciter,
                            TenFile = "images/xe/37beb51a3c4444069dede013b2f638ad.webp",
                            MoTa = null,
                            ThuTu = 1,
                            LaAnhChinh = true,
                            NgayThem = DateTime.Now
                            },
                        new HinhAnhXe  {
                            MaXe = maXeExciter,
                            TenFile = "images/xe/3e0e9dc813de411a875de386cf757a3a.webp",
                            MoTa = null,
                            ThuTu = 2,
                            LaAnhChinh = false,
                            NgayThem = DateTime.Now
                        },
                        new HinhAnhXe  {
                            MaXe = maXeExciter,
                            TenFile = "images/xe/2197958b3180408689f7d78d055b244e.webp",
                            MoTa = null,
                            ThuTu = 3,
                            LaAnhChinh = false,
                            NgayThem = DateTime.Now
                        },
                        new HinhAnhXe  {
                            MaXe = maXeExciter,
                            TenFile = "images/xe/7f591f22e2ba4e6cbc336930748578e6.webp",
                            MoTa = null,
                            ThuTu = 4,
                            LaAnhChinh = false,
                            NgayThem = DateTime.Now
                        },
                        new HinhAnhXe  {
                            MaXe = maXeExciter,
                            TenFile = "images/xe/9e24e421f27344a2975036bf4fdac125.webp",
                            MoTa = null,
                            ThuTu = 5,
                            LaAnhChinh = false,
                            NgayThem = DateTime.Now
                        },
                        new HinhAnhXe  {
                            MaXe = maXeFuture,
                            TenFile = "images/xe/068f8bd28b064a5a919871ed0364c2c4.png",
                            MoTa = null,
                            ThuTu = 1,
                            LaAnhChinh = true,
                            NgayThem = DateTime.Now
                        },
                        new HinhAnhXe  {
                            MaXe = maXeFuture,
                            TenFile = "images/xe/f5d571da5a134603b2ec81236347fa42.png",
                            MoTa = null,
                            ThuTu = 2,
                            LaAnhChinh = false,
                            NgayThem = DateTime.Now
                        },
                        new HinhAnhXe  {
                            MaXe = maXeFuture,
                            TenFile = "images/xe/f3fdda1e10f34922a801d731cbc7c978.png",
                            MoTa = null,
                            ThuTu = 3,
                            LaAnhChinh = false,
                            NgayThem = DateTime.Now
                        },
                        new HinhAnhXe  {
                            MaXe = maXeFuture,
                            TenFile = "images/xe/b086d8e2073941cba7a683897a00ed43.png",
                            MoTa = null,
                            ThuTu = 4,
                            LaAnhChinh = false,
                            NgayThem = DateTime.Now
                        },
                        new HinhAnhXe  {
                            MaXe = maXeFuture,
                            TenFile = "images/xe/964a399bd8f04d6ea5c973915b949994.png",
                            MoTa = null,
                            ThuTu = 5,
                            LaAnhChinh = false,
                            NgayThem = DateTime.Now
                        },
                        new HinhAnhXe  {
                            MaXe = maXeWave,
                            TenFile = "images/xe/e1ae71f6cfc04cdcb1abb80eabaf99eb.png",
                            MoTa = null,
                            ThuTu = 1,
                            LaAnhChinh = true,
                            NgayThem = DateTime.Now
                        },
                        new HinhAnhXe  {
                            MaXe = maXeWave,
                            TenFile = "images/xe/f8b456cb3b524b9cbe90ac9efc6cdf43.png",
                            MoTa = null,
                            ThuTu = 2,
                            LaAnhChinh = false,
                            NgayThem = DateTime.Now
                        },
                        new HinhAnhXe  {
                            MaXe = maXeWave,
                            TenFile = "images/xe/7dae6b42fb5c48628e093a0d94bf82e1.png",
                            MoTa = null,
                            ThuTu = 3,
                            LaAnhChinh = false,
                            NgayThem = DateTime.Now
                        },
                        new HinhAnhXe  {
                            MaXe = maXeWave,
                            TenFile = "images/xe/37eaab19ce6644bdb95687259827f561.png",
                            MoTa = null,
                            ThuTu = 4,
                            LaAnhChinh = false,
                            NgayThem = DateTime.Now
                        },
                        new HinhAnhXe  {
                            MaXe = maXeWave,
                            TenFile = "images/xe/443e0aa1ab5d47008f81f922df0460f7.png",
                            MoTa = null,
                            ThuTu = 5,
                            LaAnhChinh = false,
                            NgayThem = DateTime.Now
                        },
                        new HinhAnhXe  {
                            MaXe = maXeJanus,
                            TenFile = "images/xe/780bb94832ea4364a8112661d35730fc.jpg",
                            MoTa = null,
                            ThuTu = 1,
                            LaAnhChinh = true,
                            NgayThem = DateTime.Now
                        },
                        new HinhAnhXe  {
                            MaXe = maXeJanus,
                            TenFile = "images/xe/d0a86e8a8d364732934ddabfd2c77d6c.jpg",
                            MoTa = null,
                            ThuTu = 2,
                            LaAnhChinh = false,
                            NgayThem = DateTime.Now
                        },
                        new HinhAnhXe  {
                            MaXe = maXeJanus,
                            TenFile = "images/xe/caec7ef65acf4f22b0f1266cd5823a8e.jpg",
                            MoTa = null,
                            ThuTu = 3,
                            LaAnhChinh = false,
                            NgayThem = DateTime.Now
                        },
                        new HinhAnhXe  {
                            MaXe = maXeJanus,
                            TenFile = "images/xe/d3f54271ab0f4cc8aeac9654d6ad78e6.jpg",
                            MoTa = null,
                            ThuTu = 4,
                            LaAnhChinh = false,
                            NgayThem = DateTime.Now
                        },
                        new HinhAnhXe  {
                            MaXe = maXeVision,
                            TenFile = "images/xe/4d5a7b70646c498f85ade5e4cc956841.png",
                            MoTa = null,
                            ThuTu = 1,
                            LaAnhChinh = true,
                            NgayThem = DateTime.Now
                        },
                        new HinhAnhXe  {
                            MaXe = maXeVision,
                            TenFile = "images/xe/864dfa25e2b046b38f78a7f8d707bf78.png",
                            MoTa = null,
                            ThuTu = 2,
                            LaAnhChinh = false,
                            NgayThem = DateTime.Now
                        },
                        new HinhAnhXe  {
                            MaXe = maXeVision,
                            TenFile = "images/xe/d280c62228f7485f883e81e4f5b21c10.png",
                            MoTa = null,
                            ThuTu = 3,
                            LaAnhChinh = false,
                            NgayThem = DateTime.Now
                        },
                        new HinhAnhXe  {
                            MaXe = maXeVision,
                            TenFile = "images/xe/1b591a1c0bfc41a897ac88d6979cdd37.png",
                            MoTa = null,
                            ThuTu = 4,
                            LaAnhChinh = false,
                            NgayThem = DateTime.Now
                        },
                        new HinhAnhXe  {
                            MaXe = maXeVision,
                            TenFile = "images/xe/6839abc7c69a442499c4da3375588a18.png",
                            MoTa = null,
                            ThuTu = 5,
                            LaAnhChinh = false,
                            NgayThem = DateTime.Now
                        },
                        new HinhAnhXe  {
                            MaXe = maXeAirBlade,
                            TenFile = "images/xe/93db7e5239fc4ac885dc61cc111cceea.png",
                            MoTa = null,
                            ThuTu = 1,
                            LaAnhChinh = true,
                            NgayThem = DateTime.Now
                        },
                        new HinhAnhXe  {
                            MaXe = maXeAirBlade,
                            TenFile = "images/xe/1e3076365e2f43dca5347b04f9001225.png",
                            MoTa = null,
                            ThuTu = 2,
                            LaAnhChinh = false,
                            NgayThem = DateTime.Now
                        },
                        new HinhAnhXe  {
                            MaXe = maXeAirBlade,
                            TenFile = "images/xe/e1a528a1447e4444ad63d9a579b4898e.png",
                            MoTa = null,
                            ThuTu = 3,
                            LaAnhChinh = false,
                            NgayThem = DateTime.Now
                        },
                        new HinhAnhXe  {
                            MaXe = maXeAirBlade,
                            TenFile = "images/xe/31a63df98f794e53bf3502877846d52f.png",
                            MoTa = null,
                            ThuTu = 4,
                            LaAnhChinh = false,
                            NgayThem = DateTime.Now
                        },
                        new HinhAnhXe  {
                            MaXe = maXeAirBlade,
                            TenFile = "images/xe/30b957ae394c4d2fbc63d280ec9647b0.png",
                            MoTa = null,
                            ThuTu = 5,
                            LaAnhChinh = false,
                            NgayThem = DateTime.Now
                        }
                    ); context.SaveChanges();
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
                            IsSystemAccount = true,
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

                            CanViewXe = true,
                            CanCreateXe = true,
                            CanEditXe = true,
                            CanDeleteXe = true,

                            CanViewLoaiXe = true,
                            CanCreateLoaiXe = true,
                            CanEditLoaiXe = true,
                            CanDeleteLoaiXe = true,

                            CanViewHopDong = true,
                            CanProcessBooking = true,
                            CanReturnVehicle = true,

                            CanViewHoaDon = true,

                            CanViewUser = true,
                            CanCreateUser = true,
                            CanEditUser = true,
                            CanDeleteUser = true,

                            CanViewBaoCao = true,
                            CanViewThongKe = true,
                            CanExportBaoCao = true,

                            CanDatCho = true,
                            CanViewDatCho = true
                        },
                        new PhanQuyen
                        {
                            UserId = 3,

                            CanViewXe = true,
                            CanCreateXe = true,
                            CanEditXe = true,
                            CanDeleteXe = true,

                            CanViewLoaiXe = true,
                            CanCreateLoaiXe = true,
                            CanEditLoaiXe = true,
                            CanDeleteLoaiXe = false,

                            CanViewHopDong = true,
                            CanProcessBooking = true,
                            CanReturnVehicle = true,

                            CanViewHoaDon = true,

                            CanViewUser = true,
                            CanCreateUser = true,
                            CanEditUser = true,
                            CanDeleteUser = false,

                            CanViewBaoCao = true,
                            CanViewThongKe = true,
                            CanExportBaoCao = true,

                            CanDatCho = true,
                            CanViewDatCho = true
                        }
                    ); context.SaveChanges();
                }
            }
        }
    }
}
