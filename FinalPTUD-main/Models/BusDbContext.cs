
using Microsoft.EntityFrameworkCore;

namespace BusTicketBooking.Models
{
    public class BusDbContext : DbContext
    {
        public BusDbContext(DbContextOptions<BusDbContext> options) : base(options) { }

        public DbSet<User> Users => Set<User>();
        public DbSet<City> Cities => Set<City>();
        public DbSet<Route> Routes => Set<Route>();
        public DbSet<Trip> Trips => Set<Trip>();
        public DbSet<Ticket> Tickets => Set<Ticket>();
        public DbSet<Payment> Payments => Set<Payment>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            modelBuilder.Entity<Route>()
                .HasMany(r => r.Trips)
                .WithOne(t => t.Route!)
                .HasForeignKey(t => t.RouteId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Trip>()
                .HasMany(t => t.Tickets!)
                .WithOne(k => k.Trip!)
                .HasForeignKey(k => k.TripId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<User>()
                .HasMany(u => u.Tickets!)
                .WithOne(t => t.User!)
                .HasForeignKey(t => t.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // City - Route (1-n cho Departure)
            modelBuilder.Entity<Route>()
                .HasOne(r => r.DepartureCity)
                .WithMany(c => c.DepartureRoutes)
                .HasForeignKey(r => r.DepartureCityId)
                .OnDelete(DeleteBehavior.Restrict);

            // City - Route (1-n cho Destination)
            modelBuilder.Entity<Route>()
                .HasOne(r => r.DestinationCity)
                .WithMany(c => c.DestinationRoutes)
                .HasForeignKey(r => r.DestinationCityId)
                .OnDelete(DeleteBehavior.Restrict);

            // ⚡ Fix decimal warnings
            modelBuilder.Entity<Trip>()
                .Property(t => t.Price)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Payment>()
                .Property(p => p.Amount)
                .HasPrecision(18, 2);

            // ✅ Seed danh sách thành phố Việt Nam
            modelBuilder.Entity<City>().HasData(
                new City { CityId = 1, CityName = "Hà Nội" },
                new City { CityId = 2, CityName = "Hồ Chí Minh" },
                new City { CityId = 3, CityName = "Đà Nẵng" },
                new City { CityId = 4, CityName = "Hải Phòng" },
                new City { CityId = 5, CityName = "Cần Thơ" },
                new City { CityId = 6, CityName = "Nha Trang" },
                new City { CityId = 7, CityName = "Huế" },
                new City { CityId = 8, CityName = "Vinh" },
                new City { CityId = 9, CityName = "Đà Lạt" },
                new City { CityId = 10, CityName = "Quy Nhơn" },
                new City { CityId = 11, CityName = "Thanh Hóa" },
                new City { CityId = 12, CityName = "Cà Mau" },
                new City { CityId = 13, CityName = "Thái Nguyên" },
                new City { CityId = 14, CityName = "Buôn Ma Thuột" },
                new City { CityId = 15, CityName = "Phan Thiết" },
                new City { CityId = 16, CityName = "Vũng Tàu" },
                new City { CityId = 17, CityName = "Long Xuyên" },
                new City { CityId = 18, CityName = "Rạch Giá" },
                new City { CityId = 19, CityName = "Bạc Liêu" },
                new City { CityId = 20, CityName = "Sóc Trăng" }
            );
        }
    }
}
