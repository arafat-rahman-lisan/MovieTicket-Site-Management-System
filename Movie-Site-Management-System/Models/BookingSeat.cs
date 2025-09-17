using System.ComponentModel.DataAnnotations.Schema;

namespace Movie_Site_Management_System.Models
{
    public class BookingSeat
    {
        // Composite PK (BookingId, SeatId) configured in AppDbContext
        public long BookingId { get; set; }
        public long SeatId { get; set; }

        // ✅ REQUIRED now (DB is NOT NULL)
        public long ShowSeatId { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal UnitPrice { get; set; }

        // Navigations
        public Booking Booking { get; set; } = default!;
        public Seat Seat { get; set; } = default!;
        public ShowSeat ShowSeat { get; set; } = default!;
    }
}
