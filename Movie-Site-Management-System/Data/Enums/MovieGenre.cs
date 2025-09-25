using System.ComponentModel.DataAnnotations;

namespace Movie_Site_Management_System.Data.Enums
{
    public enum MovieGenre
    {
        [Display(Name = "— Select —")] Unknown = 0,

        [Display(Name = "Action")] Action = 1,
        [Display(Name = "Adventure")] Adventure = 2,
        [Display(Name = "Animation")] Animation = 3,
        [Display(Name = "Comedy")] Comedy = 4,
        [Display(Name = "Crime")] Crime = 5,
        [Display(Name = "Drama")] Drama = 6,
        [Display(Name = "Fantasy")] Fantasy = 7,
        [Display(Name = "Horror")] Horror = 8,
        [Display(Name = "Mystery")] Mystery = 9,
        [Display(Name = "Romance")] Romance = 10,
        [Display(Name = "Sci-Fi")] SciFi = 11,
        [Display(Name = "Thriller")] Thriller = 12,
        [Display(Name = "War")] War = 13,
        [Display(Name = "Documentary")] Documentary = 14,
        [Display(Name = "Superhero")] Superhero = 15,
        [Display(Name = "Other")] Other = 99
    }
}
