using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Movie_Site_Management_System.ViewModels.Halls
{
    public class HallFormVM
    {
        public long? HallId { get; set; }

        [Required(ErrorMessage = "Hall name is required.")]
        [MaxLength(60, ErrorMessage = "Hall name cannot exceed 60 characters.")]
        public string Name { get; set; } = default!;

        [Required(ErrorMessage = "Capacity is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Capacity must be greater than zero.")]
        public int Capacity { get; set; } = 100;

        [Required(ErrorMessage = "Seatmap version is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Seatmap version must be at least 1.")]
        public int SeatmapVersion { get; set; } = 1;

        [Required(ErrorMessage = "Active status must be selected.")]
        public bool IsActive { get; set; } = true;

        [Display(Name = "Theatre")]
        [Required(ErrorMessage = "Theatre is required.")]
        public long TheatreId { get; set; }

        // For dropdown
        public IEnumerable<SelectListItem> Theatres { get; set; } = Enumerable.Empty<SelectListItem>();
    }
}
