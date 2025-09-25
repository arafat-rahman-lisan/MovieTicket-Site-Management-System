using System.ComponentModel.DataAnnotations;

namespace Movie_Site_Management_System.ViewModels.Account
{
    public class ChangePasswordVM
    {
        [Required, DataType(DataType.Password), Display(Name = "Current password")]
        public string CurrentPassword { get; set; } = "";

        [Required, DataType(DataType.Password), MinLength(6), Display(Name = "New password")]
        public string NewPassword { get; set; } = "";

        [Required, DataType(DataType.Password), Compare(nameof(NewPassword)), Display(Name = "Confirm new password")]
        public string ConfirmPassword { get; set; } = "";
    }
}
