
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BusTicketBooking.Models;

namespace BusTicketBooking.Controllers
{
    [Authorize(Roles = "Admin")]
    public class RoutesController : Controller
    {
        private readonly BusDbContext _db;

        public RoutesController(BusDbContext db) => _db = db;

        public IActionResult Index() =>
            View(_db.Routes
                .Include(r => r.DepartureCity)
                .Include(r => r.DestinationCity)
                .ToList());
        public IActionResult Create()
        {
            ViewBag.Cities = _db.Cities.ToList();
            return View();
        }

        [HttpPost]
        public IActionResult Create(BusTicketBooking.Models.Route route)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Cities = _db.Cities.ToList();
                return View(route);
            }

            _db.Routes.Add(route);
            _db.SaveChanges();
            return RedirectToAction(nameof(Index));
        }


        public IActionResult Edit(int id)
        {
            var route = _db.Routes.Find(id);
            if (route == null) return NotFound();

            ViewBag.Cities = _db.Cities.ToList();
            return View(route);
        }

        [HttpPost]
        public IActionResult Edit(BusTicketBooking.Models.Route route)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Cities = _db.Cities.ToList();
                return View(route);
            }

            _db.Routes.Update(route);
            _db.SaveChanges();
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Delete(int id)
        {
            var route = _db.Routes
                 .Include(r => r.DepartureCity)
                 .Include(r => r.DestinationCity)
                 .FirstOrDefault(r => r.RouteId == id);

            if (route == null) return NotFound();
            return View(route);
        }

        [HttpPost, ActionName("Delete")]
        public IActionResult DeleteConfirmed(int id)
        {
            var route = _db.Routes.Find(id);
            if (route != null)
            {
                _db.Routes.Remove(route);
                _db.SaveChanges();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
