using Movie_Site_Management_System.Data.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace Movie_Site_Management_System.Models
{
    public class Booking
    {
        public long BookingId { get; set; }

        // FK -> Show
        public long ShowId { get; set; }

        // Legacy placeholder (not linked). Keep if you want backwards compatibility.
        public long? CustomerId { get; set; } // optional, legacy

        // NEW: FK -> AspNetUsers (ApplicationUser.Id, string)
        public string? UserId { get; set; }
        public ApplicationUser? User { get; set; }

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

        // Optional convenience nav (payments made for this booking)
        public ICollection<Payment> Payments { get; set; } = new List<Payment>();
    }
}
