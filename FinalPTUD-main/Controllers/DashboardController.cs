
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BusTicketBooking.Models;

namespace BusTicketBooking.Controllers
{
    [Authorize(Roles="Admin")]
    public class DashboardController : Controller
    {
        private readonly BusDbContext _db;
        public DashboardController(BusDbContext db) => _db = db;

        public IActionResult Index()
        {
            ViewBag.TotalTrips = _db.Trips.Count();
            ViewBag.TotalTickets = _db.Tickets.Count();
            ViewBag.TotalUsers = _db.Users.Count();
            ViewBag.Revenue = _db.Tickets
                    .Where(t => t.Status == "Confirmed")
                    .Join(_db.Trips, t => t.TripId, tr => tr.TripId, (t, tr) => tr.Price)
                    .AsEnumerable() 
                    .DefaultIfEmpty(0)
                    .Sum();
            return View();
        }

        // returns revenue by month (current year)
        public IActionResult RevenueData()
        {
            var year = DateTime.Now.Year;

            var revenueByMonth = _db.Tickets
                .Where(t => t.Status == "Confirmed" && t.BookingDate.Year == year)
                .Join(_db.Trips,
                      t => t.TripId,
                      tr => tr.TripId,
                      (t, tr) => new { t.BookingDate, tr.Price })
                .GroupBy(x => x.BookingDate.Month)
                .Select(g => new
                {
                    month = g.Key,
                    revenue = g.Sum(x => x.Price)
                })
                .ToList();

            var data = Enumerable.Range(1, 12)
                .Select(m => new
                {
                    month = m,
                    revenue = revenueByMonth.FirstOrDefault(r => r.month == m)?.revenue ?? 0
                });

            return Json(data);
        }

    }
}
