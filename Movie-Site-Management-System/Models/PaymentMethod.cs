using System.ComponentModel.DataAnnotations;

namespace Movie_Site_Management_System.Models
{
    public class PaymentMethod
    {
        public int PaymentMethodId { get; set; }

        [Required, MaxLength(50)]
        public string Name { get; set; } = default!;

        [MaxLength(20)]
        public string? CssClass { get; set; } // for button styling

        [MaxLength(200)]
        public string? LogoUrl { get; set; }

        // NAV
        public ICollection<Payment> Payments { get; set; } = new List<Payment>();
    }
}
