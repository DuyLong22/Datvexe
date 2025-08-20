
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BusTicketBooking.Models
{
    public class Payment
    {
        [Key]
        public int PaymentId { get; set; }

        [Required]
        public int TicketId { get; set; }
        [ForeignKey(nameof(TicketId))]
        public Ticket? Ticket { get; set; }

        [Required]
        [MaxLength(50)]
        public string PaymentMethod { get; set; } = "Cash";

        public decimal Amount { get; set; }

        public DateTime PaymentDate { get; set; } = DateTime.Now;
    }
}
