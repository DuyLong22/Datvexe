namespace BusTicketBooking.Models
{
    public class TicketGroupViewModel
    {
        public int TicketId { get; set; }
        public Trip Trip { get; set; } = null!;
        public List<string> Code { get; set; } = new List<string>();
        public List<int> SeatNumber { get; set; } = new List<int>(); // ghế là int
        public decimal Price { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime BookingDate { get; set; }
    }
}
