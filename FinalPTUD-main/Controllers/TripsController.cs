
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BusTicketBooking.Models;

namespace BusTicketBooking.Controllers
{
    [Authorize(Roles = "Admin")]
    public class TripsController : Controller
    {
        private readonly BusDbContext _db;
        private readonly IWebHostEnvironment _env;

        public TripsController(BusDbContext db, IWebHostEnvironment env)
        {
            _db = db;
            _env = env;
        }

        public IActionResult Index()
        {
            var trips = _db.Trips
                  .Include(t => t.Route)
                      .ThenInclude(r => r.DepartureCity)
                  .Include(t => t.Route)
                      .ThenInclude(r => r.DestinationCity)
                  .ToList();
            return View(trips);
        }

        public IActionResult Create()
        {
            ViewBag.Routes = _db.Routes
                                 .Include(r => r.DepartureCity)
                                 .Include(r => r.DestinationCity)
                                 .ToList(); // ✅ include các thành phố
            return View(new Trip { DepartureTime = DateTime.Now.AddDays(1), ArrivalTime = DateTime.Now.AddDays(1).AddHours(2) });
        }

        [HttpPost]
        public IActionResult Create(Trip trip, IFormFile? imageFile)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Routes = _db.Routes
                              .Include(r => r.DepartureCity)
                              .Include(r => r.DestinationCity)
                              .ToList(); // ✅ include lại để render dropdown khi lỗi
                return View(trip);
            }

            if (imageFile != null)
            {
                var uploads = Path.Combine(_env.WebRootPath, "images");
                Directory.CreateDirectory(uploads);
                var fileName = Guid.NewGuid().ToString("N") + Path.GetExtension(imageFile.FileName);
                var path = Path.Combine(uploads, fileName);
                using var fs = new FileStream(path, FileMode.Create);
                imageFile.CopyTo(fs);
                trip.ImagePath = "/images/" + fileName;
            }

            _db.Trips.Add(trip);
            _db.SaveChanges();
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Edit(int id)
        {
            var trip = _db.Trips.Find(id);
            if (trip == null) return NotFound();
            ViewBag.Routes = _db.Routes
                         .Include(r => r.DepartureCity)
                         .Include(r => r.DestinationCity)
                         .ToList(); // ✅ include để dropdown hiển thị tên tuyến

            return View(trip);
        }

        [HttpPost]
        public IActionResult Edit(int id, Trip trip, IFormFile? imageFile)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Routes = _db.Routes
                        .Include(r => r.DepartureCity)
                        .Include(r => r.DestinationCity)
                        .ToList();
                return View(trip);
            }

            var existingTrip = _db.Trips.Find(id);
            if (existingTrip == null) return NotFound();

            // Cập nhật các trường khớp với model của bạn
            existingTrip.RouteId = trip.RouteId;
            existingTrip.BusName = trip.BusName;
            existingTrip.DepartureTime = trip.DepartureTime;
            existingTrip.ArrivalTime = trip.ArrivalTime;
            existingTrip.Price = trip.Price;

            // Xử lý ảnh nếu có upload mới
            if (imageFile != null && imageFile.Length > 0)
            {
                var uploads = Path.Combine(_env.WebRootPath, "images");
                Directory.CreateDirectory(uploads);
                var fileName = Guid.NewGuid().ToString("N") + Path.GetExtension(imageFile.FileName);
                var path = Path.Combine(uploads, fileName);
                using var fs = new FileStream(path, FileMode.Create);
                imageFile.CopyTo(fs);
                existingTrip.ImagePath = "/images/" + fileName;
            }

            _db.SaveChanges();
            return RedirectToAction(nameof(Index));
        }


        public IActionResult Delete(int id)
        {
            var trip = _db.Trips.Include(t=>t.Route).FirstOrDefault(t=>t.TripId==id);
            if (trip == null) return NotFound();
            return View(trip);
        }

        [HttpPost, ActionName("Delete")]
        public IActionResult DeleteConfirmed(int id)
        {
            var trip = _db.Trips.Find(id);
            if (trip != null) { _db.Trips.Remove(trip); _db.SaveChanges(); }
            return RedirectToAction(nameof(Index));
        }
    }
}
