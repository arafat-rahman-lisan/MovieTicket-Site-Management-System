using Movie_Site_Management_System.Data.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace Movie_Site_Management_System.Models
{

    public class Booking
    {
        public long BookingId { get; set; }

        // FK -> Show
        public long ShowId { get; set; }

        public long? CustomerId { get; set; } // optional, future
        public BookingStatus Status { get; set; } = BookingStatus.CREATED;
        public int TicketQuantity { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal TotalAmount { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ExpiresAt { get; set; }

        // RELATIONS
        // Booking (N) -> (1) Show
        public Show Show { get; set; } = default!;

        // Booking (1) -> (N) BookingSeat
        public ICollection<BookingSeat> BookingSeats { get; set; } = new List<BookingSeat>();
    }
}
