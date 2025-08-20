
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BusTicketBooking.Models
{
    public class Ticket
    {
        [Key]
        public int TicketId { get; set; }

        [Required]
        public int TripId { get; set; }
        [ForeignKey(nameof(TripId))]
        public Trip? Trip { get; set; }

        public int? ReturnTripId { get; set; }  // chuyến về (nullable)
        [ForeignKey(nameof(ReturnTripId))]
        public Trip? ReturnTrip { get; set; }

        [Required]
        public int UserId { get; set; }
        [ForeignKey(nameof(UserId))]
        public User? User { get; set; }


        [Range(0, 999999)]
        public decimal Price { get; set; } = 0;

        public int? ReturnSeatNumber { get; set; }  // ghế chiều về (nullable)

        public ICollection<Payment>? Payments { get; set; }

        public DateTime? ExpireAt { get; set; } // thời điểm hết hạn giữ ghế

        // Thêm TransactionCode
        public string TransactionCode { get; set; } = string.Empty;

        public DateTime DepartureDate { get; set; }
        public DateTime? ReturnDate { get; set; }  // có thể null nếu đi 1 chiều
        public DateTime BookingDate { get; set; } = DateTime.Now;

        [MaxLength(6)]
        public string Code { get; set; } = string.Empty; // Mã vé 6 ký tự

        [MaxLength(20)]
        public string Status { get; set; } = "Pending"; // Pending, Confirmed, Cancelled

        // Quan hệ 1-n với ghế
        public ICollection<TicketSeat> SeatNumber { get; set; } = new List<TicketSeat>();
    }

    public class TicketSeat
    {
        [Key]
        public int Id { get; set; }

        public int TicketId { get; set; }
        public Ticket Ticket { get; set; }

        public int SeatNumber { get; set; }
    }
}
