using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Movie_Site_Management_System.Models
{
    public class Show
    {
        public long ShowId { get; set; }

        // FK -> Movie
        [Required(ErrorMessage = "Movie is required.")]
        public long MovieId { get; set; }

        // FK -> HallSlot
        [Required(ErrorMessage = "Hall slot is required.")]
        public long HallSlotId { get; set; }

        [Required(ErrorMessage = "Show date is required.")]
        public DateOnly ShowDate { get; set; }

        [Required(ErrorMessage = "Language is required.")]
        [MaxLength(30, ErrorMessage = "Language cannot exceed 30 characters.")]
        public string? Language { get; set; } = default!;

        [Required(ErrorMessage = "Active status must be set.")]
        public bool IsActive { get; set; } = true;

        [Required(ErrorMessage = "Cancelled status must be set.")]
        public bool IsCancelled { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // RELATIONS (nullable at runtime until EF loads them)
        public Movie? Movie { get; set; }
        public HallSlot? HallSlot { get; set; }
        public ICollection<ShowSeat> ShowSeats { get; set; } = new List<ShowSeat>();
        public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
        public ICollection<ShowNote> ShowNotes { get; set; } = new List<ShowNote>();

        // Convenience (not mapped) - null-safe so it never throws
        [NotMapped]
        public DateTime StartDateTime =>
            ShowDate.ToDateTime(HallSlot?.StartTime ?? TimeOnly.MinValue);

        [NotMapped]
        public DateTime EndDateTime =>
            ShowDate.ToDateTime(HallSlot?.EndTime ?? TimeOnly.MinValue);

        public DateOnly Date { get; internal set; }
    }
}
