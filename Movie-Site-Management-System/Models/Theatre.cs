using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Movie_Site_Management_System.Models
{
    public class Theatre
    {
        public long TheatreId { get; set; }

        [Required(ErrorMessage = "Theatre name is required.")]
        [MaxLength(120)]
        public string Name { get; set; } = default!;

        [Required(ErrorMessage = "Address is required.")]
        [MaxLength(255)]
        public string Address { get; set; } = default!;

        [Required(ErrorMessage = "City is required.")]
        [MaxLength(80)]
        public string City { get; set; } = default!;

        [Required(ErrorMessage = "Latitude is required.")]
        [Range(-90, 90, ErrorMessage = "Latitude must be between -90 and 90.")]
        public decimal Lat { get; set; }   // decimal(9,6)

        [Required(ErrorMessage = "Longitude is required.")]
        [Range(-180, 180, ErrorMessage = "Longitude must be between -180 and 180.")]
        public decimal Lng { get; set; }   // decimal(9,6)

        [Required(ErrorMessage = "Active status must be selected.")]
        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // RELATIONS
        // Theatre (1) -> (N) Hall
        public ICollection<Hall> Halls { get; set; } = new List<Hall>();
    }
}
