using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Movie_Site_Management_System.Models
{
    public class SeatType
    {
        public short SeatTypeId { get; set; }

        [Required(ErrorMessage = "Seat type name is required.")]
        [MaxLength(40, ErrorMessage = "Seat type name cannot exceed 40 characters.")]
        public string Name { get; set; } = default!; // Premium / Regular

        [Required(ErrorMessage = "Description is required.")]
        [MaxLength(120, ErrorMessage = "Description cannot exceed 120 characters.")]
        public string Description { get; set; } = default!;

        [Required(ErrorMessage = "Base price is required.")]
        [Range(0.01, 9999999999.99, ErrorMessage = "Base price must be greater than zero.")]
        [Column(TypeName = "decimal(10,2)")]
        public decimal BasePrice { get; set; }

        // RELATIONS
        // SeatType (1) -> (N) Seat
        public ICollection<Seat> Seats { get; set; } = new List<Seat>();
    }
}
