
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BusTicketBooking.Models
{
    public class Trip
    {
        [Key]
        public int TripId { get; set; }

        [Required]
        public int RouteId { get; set; }
        [ForeignKey(nameof(RouteId))]
        public Route? Route { get; set; }

        [Required, MaxLength(100)]
        public string BusName { get; set; } = string.Empty;

        [Required]
        public DateTime DepartureTime { get; set; }

        [Required]
        public DateTime ArrivalTime { get; set; }

        [Range(0, 999999)]
        public decimal Price { get; set; }

        public string? ImagePath { get; set; }

        public ICollection<Ticket>? Tickets { get; set; }
    }
}
