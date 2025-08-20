
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BusTicketBooking.Models;

namespace BusTicketBooking.Controllers
{
    [Authorize(Roles = "Admin")]
    public class UsersController : Controller
    {
        private readonly BusDbContext _db;

        public UsersController(BusDbContext db) => _db = db;

        public IActionResult Index(string? q)
        {
            var users = _db.Users.AsQueryable();
            if (!string.IsNullOrWhiteSpace(q))
                users = users.Where(u => u.Name.Contains(q) || u.Email.Contains(q));
            return View(users.OrderBy(u => u.UserId).ToList());
        }

        public IActionResult Edit(int id)
        {
            var user = _db.Users.Find(id);
            if (user == null) return NotFound();
            return View(user);
        }

        [HttpPost]
        public IActionResult Edit(User user)
        {
            _db.Users.Update(user);
            _db.SaveChanges();
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Delete(int id)
        {
            var user = _db.Users.Find(id);
            if (user == null) return NotFound();
            return View(user);
        }

        [HttpPost, ActionName("Delete")]
        public IActionResult DeleteConfirmed(int id)
        {
            var user = _db.Users.Find(id);
            if (user != null) { _db.Users.Remove(user); _db.SaveChanges(); }
            return RedirectToAction(nameof(Index));
        }
    }
}
