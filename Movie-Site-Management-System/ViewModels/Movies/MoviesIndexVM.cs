using System.Collections.Generic;

namespace Movie_Site_Management_System.ViewModels.Movies
{
    public class MoviesIndexVM
    {
        public List<MovieCardVM> Movies { get; set; } = new();
        public string? Search { get; set; }
    }
}
