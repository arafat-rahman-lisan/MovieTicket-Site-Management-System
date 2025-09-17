using System.ComponentModel.DataAnnotations;

namespace Movie_Site_Management_System.ViewModels.Account
{
    public class RegisterVM
    {
        [Required, MaxLength(120), Display(Name = "Full name")]
        public string FullName { get; set; } = "";

        [Required, Display(Name = "Email address"), EmailAddress]
        public string EmailAddress { get; set; } = "";

        [Required, DataType(DataType.Password), MinLength(6)]
        public string Password { get; set; } = "";

        [Required, DataType(DataType.Password), Display(Name = "Confirm password"), Compare("Password")]
        public string ConfirmPassword { get; set; } = "";
    }
}
