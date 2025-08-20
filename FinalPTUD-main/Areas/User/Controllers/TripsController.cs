using BusTicketBooking.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace BusTicketBooking.Areas.User.Controllers
{
    [Area("User")]
    public class TripsController : Controller
    {
        private readonly BusDbContext _db;

        public TripsController(BusDbContext db) => _db = db;

        // Danh sách chuyến đi cho người dùng
        public IActionResult Index(string from, string to, string date)
        {
            var trips = _db.Trips
                .Include(t => t.Route)
                    .ThenInclude(r => r!.DepartureCity)
                .Include(t => t.Route)
                    .ThenInclude(r => r!.DestinationCity)
                .Include(t => t.Tickets) // ✅ load Tickets để tính chỗ trống
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(from))
            {
                var keyword = from.Trim();
                trips = trips.Where(t =>
                        t.Route != null &&
                        t.Route.DepartureCity != null &&
                        EF.Functions.Like(t.Route.DepartureCity.CityName, $"%{keyword}%"));
            }

            if (!string.IsNullOrWhiteSpace(to))
            {
                var keyword = to.Trim();
                trips = trips.Where(t =>
                        t.Route != null &&
                        t.Route.DestinationCity != null &&
                        EF.Functions.Like(t.Route.DestinationCity.CityName, $"%{keyword}%"));
            }

            if (!string.IsNullOrWhiteSpace(date))
            {
                if (DateTime.TryParseExact(date, "dd/MM/yyyy",
                        CultureInfo.InvariantCulture, DateTimeStyles.None, out var selectedDate))
                {
                    trips = trips.Where(t => t.DepartureTime >= selectedDate
                                           && t.DepartureTime < selectedDate.AddDays(1));
                }
            }

            // Truyền giá trị đã chọn về view
            ViewBag.From = from;
            ViewBag.To = to;
            ViewBag.Date = date ?? DateTime.Now.ToString("dd/MM/yyyy");

            return View(trips.ToList());
        }



    }
}
