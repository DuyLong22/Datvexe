using System.ComponentModel.DataAnnotations;

namespace BusTicketBooking.Models
{
    public class City
    {
        [Key]
        public int CityId { get; set; }

        [Required, MaxLength(100)]
        public string CityName { get; set; } = string.Empty;

        // Quan hệ ngược với Route
        public ICollection<Route>? DepartureRoutes { get; set; }
        public ICollection<Route>? DestinationRoutes { get; set; }
    }
}
