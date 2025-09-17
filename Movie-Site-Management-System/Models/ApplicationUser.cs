using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Movie_Site_Management_System.Models
{
    /// <summary>
    /// Extend IdentityUser with domain fields.
    /// </summary>
    public class ApplicationUser : IdentityUser
    {
        [Display(Name = "Full name")]
        [MaxLength(120)]
        public string? FullName { get; set; }
    }
}
