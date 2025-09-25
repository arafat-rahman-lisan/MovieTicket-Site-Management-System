using System;
using System.ComponentModel.DataAnnotations;

namespace Movie_Site_Management_System.ViewModels.Account
{
    public class EditUserVM
    {
        [Required]
        public string Id { get; set; } = default!;

        [Required, EmailAddress]
        public string Email { get; set; } = default!; // read-only in UI

        [Required]
        public string Role { get; set; } = default!;
        [Required, EmailAddress]
        public bool LockoutEnabled { get; set; }
        public DateTimeOffset? LockoutEnd { get; set; }
        public string? FullName { get; internal set; }
    }
}
