using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Movie_Site_Management_System.Data.Enums;

namespace Movie_Site_Management_System.Models
{
    public class Payment
    {
        public long PaymentId { get; set; }

        [Required]
        public long BookingId { get; set; }

        [Required]
        public int PaymentMethodId { get; set; }

        [Required]
        public PaymentStatus Status { get; set; } = PaymentStatus.Pending;

        [Column(TypeName = "decimal(10,2)")]
        public decimal Amount { get; set; }

        [MaxLength(32)]
        public string InvoiceNo { get; set; } = default!;

        [MaxLength(64)]
        public string? ProviderTxnId { get; set; }
        [MaxLength(64)]
        public string? ProviderRef { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? PaidAt { get; set; }

        // NAV
        public Booking Booking { get; set; } = default!;
        public PaymentMethod PaymentMethod { get; set; } = default!;
    }
}
