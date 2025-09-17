using System.Collections.Generic;

namespace Movie_Site_Management_System.ViewModels.Movies
{
    public class MoviesIndexVM
    {
        public List<MovieIndexItemVM> Movies { get; set; } = new();
        public string Search { get; set; } = string.Empty;
    }
}
