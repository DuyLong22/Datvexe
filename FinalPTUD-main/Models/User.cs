
using System.ComponentModel.DataAnnotations;

namespace BusTicketBooking.Models
{
    public class User
    {
        [Key]
        public int UserId { get; set; }

        [Required, MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required, MaxLength(100)]
        public string Email { get; set; } = string.Empty;

        [Required, MaxLength(255)]
        public string PasswordHash { get; set; } = string.Empty;

        [Required, MaxLength(20)]
        public string Role { get; set; } = "User"; // Admin / User

        [MaxLength(20)]
        public string? Phone { get; set; }

        public ICollection<Ticket>? Tickets { get; set; }
    }
}
