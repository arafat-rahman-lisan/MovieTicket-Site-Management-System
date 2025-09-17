using System.ComponentModel.DataAnnotations;

namespace Movie_Site_Management_System.ViewModels.Account
{
    public class ProfileVM
    {
        [Required, MaxLength(120), Display(Name = "Full name")]
        public string FullName { get; set; } = "";

        [Required, Display(Name = "Email address"), EmailAddress]
        public string EmailAddress { get; set; } = "";
    }
}
