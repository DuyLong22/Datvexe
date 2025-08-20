using BusTicketBooking.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;




namespace BusTicketBooking.Controllers
{
    public enum TicketStatus
    {
        Pending,
        Confirmed,
        Cancelled
    }

    [Authorize]
    public class TicketsController : Controller
    {
        private readonly BusDbContext _db;

        public TicketsController(BusDbContext db) => _db = db;

        // GET: Book page
        public IActionResult Book(int id)
        {
            var trip = _db.Trips
                .Include(t => t.Route)
                    .ThenInclude(r => r!.DepartureCity)
                .Include(t => t.Route)
                    .ThenInclude(r => r!.DestinationCity)
                .Include(t => t!.Tickets!.Where(ticket => ticket.Status != TicketStatus.Cancelled.ToString()))
                .FirstOrDefault(t => t.TripId == id);

            if (trip == null) return NotFound();
            return View(trip);
        }

        private string GenerateTicketCode(int length = 6)
        {
            // Dùng Guid để đảm bảo uniqueness
            return Guid.NewGuid().ToString("N").Substring(0, length).ToUpper();
        }

        // GET: Lấy danh sách ghế đã đặt
        [HttpGet]
        public IActionResult GetBookedSeats(int tripId)
        {
            var now = DateTime.Now;

            var seats = _db.Tickets
                .Where(t => t.TripId == tripId &&
                       (t.Status == "Confirmed" || (t.Status == "Pending" && t.ExpireAt > now)))
                .Select(t => new {
                    SeatNumber = t.SeatNumber,
                    Status = t.Status,
                    ExpireAt = t.ExpireAt
                })
                .ToList();

            return Json(seats);
        }


        // POST: Đặt vé
        [HttpPost]
        public IActionResult Book(int id, int[] seatNumbers, string paymentMethod = "VNPAY")
        {
            var trip = _db.Trips.Find(id);
            if (trip == null) return NotFound();

            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userIdClaim == null) return Unauthorized();
            int userId = int.Parse(userIdClaim);

            if (seatNumbers.Length > 5)
            {
                TempData["Alert"] = "Bạn chỉ có thể đặt tối đa 5 vé trong một lần.";
                return RedirectToAction(nameof(Book), new { id });
            }

            using var transaction = _db.Database.BeginTransaction();

            // Kiểm tra ghế đã được đặt (tránh race condition)
            var takenSeats = _db.Tickets
                .Where(t => t.TripId == id && t.Status != TicketStatus.Cancelled.ToString())
                .SelectMany(t => t.SeatNumber.Select(s => s.SeatNumber)) // lấy tất cả số ghế
                .Where(s => seatNumbers.Contains(s))
                .ToList();



            if (takenSeats.Any())
            {
                TempData["Alert"] = $"Ghế {string.Join(", ", takenSeats)} đã được đặt. Vui lòng chọn ghế khác.";
                return RedirectToAction(nameof(Book), new { id });
            }

            string ticketCode = GenerateTicketCode();

            var tickets = seatNumbers.Select(seatNumber => new Ticket
            {
                TripId = id,
                UserId = userId,
                Price = trip.Price,
                Status = TicketStatus.Pending.ToString(),
                BookingDate = DateTime.Now,
                ExpireAt = DateTime.Now.AddMinutes(10), // giữ tối đa 10 phút
                Code = ticketCode,
                SeatNumber = new List<TicketSeat>
                {
                    new TicketSeat { SeatNumber = seatNumber }
                }
            }).ToList();

            _db.Tickets.AddRange(tickets);
            _db.SaveChanges();

            transaction.Commit();

            var ticketIdsQuery = string.Join(",", tickets.Select(t => t.TicketId));

            if (tickets.Count == 1)
            {
                return RedirectToAction("Pay", new { ticketId = tickets[0].TicketId });
            }
            else
            {
                return Redirect($"/Tickets/PayMultiple?ticketIds={ticketIdsQuery}");
            }
        }

        // GET: Thanh toán nhiều vé
        [HttpGet]
        public IActionResult PayMultiple([FromQuery] string ticketIds)
        {
            if (string.IsNullOrEmpty(ticketIds)) return NotFound();

            var ids = ticketIds.Split(',').Select(int.Parse).ToList();
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            int userId = int.Parse(userIdClaim);

            var tickets = _db.Tickets
                .Where(t => ids.Contains(t.TicketId) && t.UserId == userId)
                .Include(t => t.Trip)
                    .ThenInclude(tr => tr.Route)
                        .ThenInclude(r => r.DepartureCity)
                .Include(t => t.Trip)
                    .ThenInclude(tr => tr.Route)
                        .ThenInclude(r => r.DestinationCity)
                .Include(t => t.User)
                .ToList();

            if (!tickets.Any()) return Unauthorized(); // user không sở hữu vé
            return View(tickets);
        }

        [HttpPost]
        public IActionResult PayMultipleConfirm(string ticketIds, string paymentMethod)
        {
            if (string.IsNullOrEmpty(ticketIds)) return NotFound();

            var ids = ticketIds.Split(',').Select(int.Parse).ToList();
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            int userId = int.Parse(userIdClaim);

            var tickets = _db.Tickets.Where(t => ids.Contains(t.TicketId) && t.UserId == userId).ToList();
            if (!tickets.Any()) return Unauthorized();

            foreach (var ticket in tickets)
            {
                ticket.Status = TicketStatus.Confirmed.ToString();
                ticket.ExpireAt = null; // Xóa thời gian hết hạn giữ ghế
                var payment = new Payment
                {
                    TicketId = ticket.TicketId,
                    Amount = ticket.Price,
                    PaymentMethod = string.IsNullOrEmpty(paymentMethod) ? "Cash" : paymentMethod,
                    PaymentDate = DateTime.Now
                };
                _db.Payments.Add(payment);
            }

            _db.SaveChanges();
            TempData["Alert"] = "Thanh toán thành công!";
            return RedirectToAction(nameof(MyTickets));
        }

        // GET: Trang thanh toán
        public IActionResult Pay(int ticketId)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            int userId = int.Parse(userIdClaim);

            var ticket = _db.Tickets
                .Include(t => t.User)
                .Include(t => t.Trip)
                    .ThenInclude(tr => tr.Route)
                        .ThenInclude(r => r.DepartureCity)
                .Include(t => t.Trip)
                    .ThenInclude(tr => tr.Route)
                        .ThenInclude(r => r.DestinationCity)
                .FirstOrDefault(t => t.TicketId == ticketId && t.UserId == userId);

            if (ticket == null) return Unauthorized();
            return View(ticket);
        }

        // POST: Xác nhận thanh toán
        [HttpPost]
        public IActionResult PayConfirm(int ticketId, string paymentMethod)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            int userId = int.Parse(userIdClaim);

            var ticket = _db.Tickets.FirstOrDefault(t => t.TicketId == ticketId && t.UserId == userId);
            if (ticket == null) return Unauthorized();

            var payment = new Payment
            {
                TicketId = ticket.TicketId,
                Amount = ticket.Price,
                PaymentMethod = string.IsNullOrEmpty(paymentMethod) ? "Cash" : paymentMethod,
                PaymentDate = DateTime.Now
            };

            _db.Payments.Add(payment);
            ticket.Status = TicketStatus.Confirmed.ToString();
            ticket.ExpireAt = null; // Xóa thời gian hết hạn giữ ghế

            try
            {
                _db.SaveChanges();
                TempData["Alert"] = "Thanh toán thành công!";
            }
            catch (Exception ex)
            {
                TempData["Alert"] = "Thanh toán thất bại: " + ex.InnerException?.Message ?? ex.Message;
            }

            return RedirectToAction(nameof(MyTickets));
        }

        // GET: Vé của user
        public IActionResult MyTickets()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userIdClaim == null) return Unauthorized();
            int userId = int.Parse(userIdClaim);

            var tickets = _db.Tickets
                .Where(t => t.UserId == userId && t.Status != "Cancelled") // bỏ vé hủy
                .Include(t => t.Trip)
                    .ThenInclude(tr => tr.Route)
                        .ThenInclude(r => r.DepartureCity)
                .Include(t => t.Trip)
                    .ThenInclude(tr => tr.Route)
                        .ThenInclude(r => r.DestinationCity)
                .OrderByDescending(t => t.BookingDate)
                .ToList();

            // Gộp vé theo chuyến
            var grouped = tickets
    .GroupBy(t => t.TripId)
    .Select(g => new TicketGroupViewModel
    {
        Trip = g.First().Trip!,
        Code = g.Select(x => x.Code).ToList(),
        SeatNumber = g.SelectMany(x => x.SeatNumber.Select(s => s.SeatNumber)).ToList(), // ghép tất cả ghế
        Price = g.Sum(x => x.Price),
        Status = g.All(x => x.Status == "Confirmed") ? "Confirmed" :
                 g.All(x => x.Status == "Pending") ? "Pending" : "Mixed",
        BookingDate = g.Min(x => x.BookingDate)
    })
    .OrderByDescending(x => x.BookingDate)
    .ToList();

            return View(grouped);
        }


        // GET: Quản lý vé (Admin)
        [Authorize(Roles = "Admin")]
        public IActionResult Manage()
        {
            var tickets = _db.Tickets
                .Include(t => t.User)
                .Include(t => t.Trip)
                    .ThenInclude(tr => tr.Route)
                        .ThenInclude(r => r.DepartureCity)
                .Include(t => t.Trip)
                    .ThenInclude(tr => tr.Route)
                        .ThenInclude(r => r.DestinationCity)
                .OrderByDescending(t => t.BookingDate)
                .ToList();

            return View(tickets);
        }

        // POST: Cập nhật trạng thái vé (Admin)
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public IActionResult UpdateStatus(int id, string status)
        {
            var ticket = _db.Tickets.Find(id);
            if (ticket == null) return NotFound();

            if (!Enum.TryParse<TicketStatus>(status, out var parsedStatus))
                return BadRequest("Trạng thái không hợp lệ.");

            ticket.Status = parsedStatus.ToString();
            _db.SaveChanges();

            return RedirectToAction(nameof(Manage));
        }
    }
}
