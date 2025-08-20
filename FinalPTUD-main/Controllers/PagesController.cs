using Microsoft.AspNetCore.Mvc;

namespace BusTicketBooking.Controllers
{
    public class PagesController : Controller
    {
        public IActionResult About()
        {
            ViewData["Title"] = "Về chúng tôi";
            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Title"] = "Liên hệ";
            return View();
        }
    }
}
