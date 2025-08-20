using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BusTicketBooking.Models;

namespace BusTicketBooking.Controllers
{
    public class CitiesController : Controller
    {
        private readonly BusDbContext _db;
        public CitiesController(BusDbContext db) => _db = db;

        [HttpGet]
        public IActionResult GetAllCities(string keyword)
        {
            var cities = _db.Cities
                .Where(c => string.IsNullOrEmpty(keyword) || c.CityName.Contains(keyword))
                .Select(c => new { c.CityName })
                .OrderBy(c => c.CityName)
                .ToList();

            return Json(cities);
        }
    }
}
