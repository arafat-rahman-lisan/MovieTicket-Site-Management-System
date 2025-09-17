using System.ComponentModel.DataAnnotations;

namespace Movie_Site_Management_System.Models
{
    public class Hall
    {
        public long HallId { get; set; }

        // FK -> Theatre
        [Required(ErrorMessage = "Theatre is required.")]
        public long TheatreId { get; set; }

        [Required(ErrorMessage = "Hall name is required.")]
        [MaxLength(60, ErrorMessage = "Hall name cannot exceed 60 characters.")]
        public string Name { get; set; } = default!;

        [Required(ErrorMessage = "Capacity is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Capacity must be greater than zero.")]
        public int Capacity { get; set; }

        [Required(ErrorMessage = "Seatmap version is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Seatmap version must be at least 1.")]
        public int SeatmapVersion { get; set; } = 1;

        [Required(ErrorMessage = "Active status must be selected.")]
        public bool IsActive { get; set; } = true;

        // RELATIONS
        // Hall (N) -> (1) Theatre
        public Theatre Theatre { get; set; } = default!;

        // Hall (1) -> (N) Seat
        public ICollection<Seat> Seats { get; set; } = new List<Seat>();

        // Hall (1) -> (N) HallSlot
        public ICollection<HallSlot> HallSlots { get; set; } = new List<HallSlot>();
    }
}
