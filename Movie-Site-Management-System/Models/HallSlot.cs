using System.ComponentModel.DataAnnotations;

namespace Movie_Site_Management_System.Models
{
    public class HallSlot
    {
        public long HallSlotId { get; set; }

        // FK -> Hall
        [Required(ErrorMessage = "Hall is required.")]
        public long HallId { get; set; }

        [Required(ErrorMessage = "Slot number is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Slot number must be at least 1.")]
        public int SlotNumber { get; set; } // 1,2,3...

        [Required(ErrorMessage = "Start time is required.")]
        public TimeOnly StartTime { get; set; }

        [Required(ErrorMessage = "End time is required.")]
        public TimeOnly EndTime { get; set; }

        [Required(ErrorMessage = "Active status must be selected.")]
        public bool IsActive { get; set; } = true;

        // RELATIONS
        // HallSlot (N) -> (1) Hall
        public Hall Hall { get; set; } = default!;

        // HallSlot (1) -> (N) Show
        public ICollection<Show> Shows { get; set; } = new List<Show>();
    }
}
