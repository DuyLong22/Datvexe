
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BusTicketBooking.Models
{
    public class Route
    {
        [Key]
        public int RouteId { get; set; }

        [Required]
        public int DepartureCityId { get; set; }

        [Required]
        public int DestinationCityId { get; set; }

        public double Distance { get; set; }

        // Navigation property
        [ForeignKey("DepartureCityId")]
        public City? DepartureCity { get; set; }

        [ForeignKey("DestinationCityId")]
        public City? DestinationCity { get; set; }

        public ICollection<Trip>? Trips { get; set; }
    }
}
