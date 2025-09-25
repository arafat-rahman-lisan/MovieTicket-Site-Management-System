using System.ComponentModel.DataAnnotations;

namespace Movie_Site_Management_System.ViewModels.Schedule
{
    public class ScheduleWizardVM
    {
        [Required]
        public DateOnly Date { get; set; }

        [Required, MinLength(1)]
        public List<long> MovieIds { get; set; } = new();

        // NEW: Language is required (no default)
        [Required, StringLength(50, ErrorMessage = "Language must be 1–50 characters.")]
        public string Language { get; set; } = string.Empty;

        public List<ScheduleSelectionItem> Selections { get; set; } = new();
    }
}
