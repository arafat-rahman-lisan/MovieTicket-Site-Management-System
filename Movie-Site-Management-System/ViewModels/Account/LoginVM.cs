using System.ComponentModel.DataAnnotations;

namespace Movie_Site_Management_System.ViewModels.Account
{
    public class LoginVM
    {
        [Required, Display(Name = "Email address"), EmailAddress]
        public string EmailAddress { get; set; } = "";

        [Required, DataType(DataType.Password)]
        public string Password { get; set; } = "";

        [Display(Name = "Remember me")]
        public bool RememberMe { get; set; }

        public string? ReturnUrl { get; set; }
    }
}
