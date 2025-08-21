using BusTicketBooking.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Text;
using System.Linq;

namespace BusTicketBooking.Areas.User.Controllers
{
    [Area("User")]
    public class TripsController : Controller
    {
        private readonly BusDbContext _db;

        public TripsController(BusDbContext db) => _db = db;

        // Hàm normalize: bỏ dấu, ký tự đặc biệt, khoảng trắng
        private string NormalizeString(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return string.Empty;

            string normalized = input.Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder();

            foreach (var c in normalized)
            {
                var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != UnicodeCategory.NonSpacingMark && char.IsLetterOrDigit(c))
                    sb.Append(c);
            }

            return sb.ToString().Normalize(NormalizationForm.FormC).ToLower().Trim();
        }

        // Danh sách chuyến đi cho người dùng
        public IActionResult Index(string from, string to, string date)
        {
            var trips = _db.Trips
                .Include(t => t.Route)!.ThenInclude(r => r.DepartureCity)
                .Include(t => t.Route)!.ThenInclude(r => r.DestinationCity)
                .Include(t => t.Tickets)
                .AsQueryable();

            // Lọc theo ngày
            if (!string.IsNullOrWhiteSpace(date))
            {
                if (DateTime.TryParseExact(date,
                        new[] { "dd/MM/yyyy", "yyyy-MM-dd" },
                        CultureInfo.InvariantCulture,
                        DateTimeStyles.None,
                        out var selectedDate))
                {
                    trips = trips.Where(t => t.DepartureTime >= selectedDate
                                           && t.DepartureTime < selectedDate.AddDays(1));
                }
            }

            // Chuẩn hóa chuỗi: bỏ dấu + chỉ giữ chữ + số
            string NormalizeForSearch(string input)
            {
                string s = NormalizeString(input); // bỏ dấu + lowercase
                return new string(s.Where(c => char.IsLetterOrDigit(c) || char.IsWhiteSpace(c)).ToArray());
            }

            var normalizedFrom = NormalizeForSearch(from ?? "");
            var normalizedTo = NormalizeForSearch(to ?? "");

            var tripList = trips.AsEnumerable()
                .Where(t =>
                {
                    var dbFrom = NormalizeForSearch(t.Route?.DepartureCity?.CityName ?? "");
                    var dbTo = NormalizeForSearch(t.Route?.DestinationCity?.CityName ?? "");

                    return dbFrom.Contains(normalizedFrom) && dbTo.Contains(normalizedTo);
                })
                .ToList();

            ViewBag.From = from;
            ViewBag.To = to;
            ViewBag.Date = date ?? DateTime.Now.ToString("dd/MM/yyyy");

            return View(tripList);
        }
    }
}
