using System.ComponentModel.DataAnnotations;

namespace Movie_Site_Management_System.Data.Enums
{
    public enum MovieStatus
    {
        [Display(Name = "— Select —")] Unknown = 0,
        [Display(Name = "Coming Soon")] ComingSoon = 1,
        [Display(Name = "Now Showing")] NowShowing = 2,
        [Display(Name = "Archived")] Archived = 3
    }
}
