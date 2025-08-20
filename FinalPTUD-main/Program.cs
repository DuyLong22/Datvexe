
using BusTicketBooking.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddHostedService<TicketCleanupService>();

builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<BusDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Auth/Login";
        options.AccessDeniedPath = "/Auth/AccessDenied";
    });

builder.Services.AddAuthorization();

var app = builder.Build();

// Seed database sample data
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<BusDbContext>();
    db.Database.EnsureCreated();

    if (!db.Users.Any())
    {
        // admin / 123
        string Hash(string s)
        {
            using var sha = System.Security.Cryptography.SHA256.Create();
            var bytes = System.Text.Encoding.UTF8.GetBytes(s);
            var hash = sha.ComputeHash(bytes);
            return BitConverter.ToString(hash).Replace("-", "").ToLower();
        }

        db.Users.Add(new User
        {
            Name = "Admin",
            Email = "admin@local",
            PasswordHash = Hash("123"),
            Role = "Admin",
            Phone = "0000000000"
        });
        db.Users.Add(new User
        {
            Name = "Người dùng",
            Email = "user@local",
            PasswordHash = Hash("123"),
            Role = "User",
            Phone = "0900000000"
        });
        db.SaveChanges();
    }

    if (!db.Cities.Any())
    {
        var hcm = new City { CityName = "TP.HCM" };
        var vt = new City { CityName = "Vũng Tàu" };
        var dl = new City { CityName = "Đà Lạt" };

        db.Cities.AddRange(hcm, vt, dl);
        db.SaveChanges();

        if (!db.Routes.Any())
        {
            var r1 = new BusTicketBooking.Models.Route { DepartureCityId = hcm.CityId, DestinationCityId = vt.CityId, Distance = 100 };
            var r2 = new BusTicketBooking.Models.Route { DepartureCityId = hcm.CityId, DestinationCityId = dl.CityId, Distance = 320 };
            db.Routes.AddRange(r1, r2);
            db.SaveChanges();

            db.Trips.Add(new Trip
            {
                RouteId = r1.RouteId,
                BusName = "Phuong Trang 45 chỗ",
                DepartureTime = DateTime.Now.AddDays(1).AddHours(8),
                ArrivalTime = DateTime.Now.AddDays(1).AddHours(10),
                Price = 150000
            });
            db.Trips.Add(new Trip
            {
                RouteId = r2.RouteId,
                BusName = "Thanh Buoi Limousine",
                DepartureTime = DateTime.Now.AddDays(2).AddHours(7),
                ArrivalTime = DateTime.Now.AddDays(2).AddHours(12),
                Price = 350000
            });
            db.SaveChanges();
        }
    }

    // Configure pipeline
    if (!app.Environment.IsDevelopment())
    {
        app.UseExceptionHandler("/Home/Error");
        app.UseHsts();
    }

    app.UseHttpsRedirection();
    app.UseStaticFiles();

    app.UseRouting();

    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllerRoute(
        name: "areas",
        pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}"); 

    app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}");

    app.Run();
}