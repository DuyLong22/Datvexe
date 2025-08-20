using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BusTicketBooking.Models;

namespace BusTicketBooking.Controllers
{
    public class HomeController : Controller
    {
        private readonly BusDbContext _db;

        public HomeController(BusDbContext db) => _db = db;

        public IActionResult Index(string from, string to, DateTime? date)
        {
            var trips = _db.Trips
                .Include(t => t.Route)
                    .ThenInclude(r => r!.DepartureCity)
                .Include(t => t.Route)
                    .ThenInclude(r => r!.DestinationCity)
                .AsQueryable();

            // Lọc theo điểm đi
            if (!string.IsNullOrWhiteSpace(from))
            {
                var keyword = from.Trim().ToLower();
                trips = trips.Where(t => t!.Route!.DepartureCity.CityName.ToLower().Contains(keyword));
            }

            // Lọc theo điểm đến
            if (!string.IsNullOrWhiteSpace(to))
            {
                var keyword = to.Trim().ToLower();
                trips = trips.Where(t => t!.Route!.DestinationCity.CityName.ToLower().Contains(keyword));
            }

            // Lọc theo ngày
            if (date.HasValue)
            {
                trips = trips.Where(t => t.DepartureTime.Date == date.Value.Date);
            }

            return View(trips.ToList());
        }
    }
}
