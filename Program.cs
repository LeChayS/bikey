using bikey.Repository;
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
    pattern: "{controller=Admin}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapRazorPages();

app.Run();