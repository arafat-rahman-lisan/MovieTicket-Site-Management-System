using System.ComponentModel.DataAnnotations.Schema;

namespace Movie_Site_Management_System.Models
{
    public class BookingSeat
    {
        // COMPOSITE KEY: BookingId + SeatId (configured in DbContext)
        public long BookingId { get; set; }
        public long SeatId { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal UnitPrice { get; set; }  // snapshot from SeatType.BasePrice

        // RELATIONS
        // BookingSeat (N) -> (1) Booking
        public Booking Booking { get; set; } = default!;

        // BookingSeat (N) -> (1) Seat
        public Seat Seat { get; set; } = default!;
    }
}
