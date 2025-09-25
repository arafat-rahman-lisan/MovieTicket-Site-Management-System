using System.ComponentModel.DataAnnotations;

namespace Movie_Site_Management_System.ViewModels.Account
{
    public class ResetPasswordVM
    {
        [Required]
        public string Id { get; set; } = default!;

        // Displayed read-only in the form
        public string Email { get; set; } = string.Empty;

        [Required, DataType(DataType.Password), MinLength(6)]
        [Display(Name = "New Password")]
        public string NewPassword { get; set; } = default!;

        [Required, DataType(DataType.Password), Compare(nameof(NewPassword))]
        [Display(Name = "Confirm Password")]
        public string ConfirmPassword { get; set; } = default!;
    }
}
