using BusTicketBooking.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace BusTicketBooking.Controllers
{
    [Authorize]
    public class PaymentController : Controller
    {
        private readonly BusDbContext _db;
        public PaymentController(BusDbContext db) => _db = db;

        // GET: VNPAY thanh toán
        public IActionResult VnPay(int ticketId)
        {
            var ticket = _db.Tickets
                .Include(t => t.Trip)
                    .ThenInclude(tr => tr.Route)
                        .ThenInclude(r => r.DepartureCity)
                .Include(t => t.Trip)
                    .ThenInclude(tr => tr.Route)
                        .ThenInclude(r => r.DestinationCity)
                .FirstOrDefault(t => t.TicketId == ticketId);

            if (ticket == null) return NotFound();

            // Tạo URL thanh toán giả lập (có thể dùng VNPayHelper thật)
            string vnpUrl = $"/Payment/VnPayReturn?ticketId={ticketId}";
            ViewBag.VnPayUrl = vnpUrl;

            return View(ticket);
        }

        // GET: VNPAY return (sau khi thanh toán thành công)
        public IActionResult VnPayReturn(int ticketId)
        {
            var ticket = _db.Tickets.Find(ticketId);
            if (ticket == null) return NotFound();

            // Tạo Payment
            var payment = new Payment
            {
                TicketId = ticket.TicketId,
                Amount = ticket.Price,
                PaymentMethod = "VNPAY",
                PaymentDate = DateTime.Now
            };
            _db.Payments.Add(payment);
            ticket.Status = "Confirmed"; // Xác nhận vé
            _db.SaveChanges();

            TempData["Message"] = "Thanh toán online thành công!";
            return RedirectToAction("MyTickets", "Tickets");
        }
    }
}
