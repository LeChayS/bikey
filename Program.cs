using bikey.Repository;
using bikey.Services;
using bikey.Common;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.AccessDeniedPath = "/AccessDenied";
    });
builder.Services.AddAuthorization();

// AutoMapper setup
builder.Services.AddAutoMapper(typeof(MappingProfile));

// Register UserService
builder.Services.AddScoped<IUserService, UserService>();

// Register Domain Services
builder.Services.AddScoped<IXeService, XeService>();
builder.Services.AddScoped<IHopDongService, HopDongService>();
builder.Services.AddScoped<IHoaDonService, HoaDonService>();
builder.Services.AddScoped<ILoaiXeService, LoaiXeService>();
builder.Services.AddScoped<INguoiDungService, NguoiDungService>();
builder.Services.AddScoped<ITrangChuService, TrangChuService>();
builder.Services.AddScoped<IDatXeService, DatXeService>();
builder.Services.AddScoped<IOnlineUserService, OnlineUserService>();

// Cấu hình Entity Framework với SQL Server - tối ưu performance
builder.Services.AddDbContext<BikeyDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("BikeyConnection"),
        sqlServerOptionsAction: sqlOptions =>
        {
            sqlOptions.EnableRetryOnFailure(
                maxRetryCount: 3,
                maxRetryDelay: TimeSpan.FromSeconds(30),
                errorNumbersToAdd: null);
            sqlOptions.CommandTimeout(120); // 2 phút timeout
        })
    .EnableSensitiveDataLogging(false)
    .EnableDetailedErrors(false)
);

var app = builder.Build();

app.UseRouting();

app.UseHttpsRedirection();
app.UseStaticFiles();

SeedData.EnsurePopulated(app);

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=TrangChu}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapRazorPages();

app.Run();