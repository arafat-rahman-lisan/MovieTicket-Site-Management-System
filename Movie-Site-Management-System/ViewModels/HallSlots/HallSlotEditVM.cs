using System.ComponentModel.DataAnnotations;

namespace Movie_Site_Management_System.ViewModels.HallSlots
{
    public class HallSlotEditVM
    {
        public long? HallSlotId { get; set; }

        [Required]
        [Display(Name = "Hall")]
        public long HallId { get; set; }

        [Required, Range(1, 50, ErrorMessage = "Slot number must be 1-50.")]
        [Display(Name = "Slot #")]
        public int SlotNumber { get; set; }

        [Required]
        [DataType(DataType.Time)]
        [Display(Name = "Start")]
        public TimeOnly StartTime { get; set; }

        [Required]
        [DataType(DataType.Time)]
        [Display(Name = "End")]
        public TimeOnly EndTime { get; set; }

        [Display(Name = "Active")]
        public bool IsActive { get; set; }
    }
}
