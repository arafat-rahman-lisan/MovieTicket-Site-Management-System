using System.ComponentModel.DataAnnotations;

namespace Movie_Site_Management_System.ViewModels.Account
{
    public class CreateUserVM
    {
        [Required, EmailAddress]
        public string Email { get; set; } = default!;

        [Required, DataType(DataType.Password), MinLength(6)]
        public string Password { get; set; } = default!;

        [Required, DataType(DataType.Password), Compare(nameof(Password))]
        public string ConfirmPassword { get; set; } = default!;

        [Required]
        public string Role { get; set; } = default!;
        public string? FullName { get; internal set; }
    }
}
