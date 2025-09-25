using System.Collections.Generic;
using Movie_Site_Management_System.ViewModels.Shows;

namespace Movie_Site_Management_System.ViewModels.Movies
{
    public class MovieDetailsVM
    {
        public long MovieId { get; set; }

        // Basic meta
        public string? Title { get; set; }
        public string? Synopsis { get; set; }
        public string? Genre { get; set; }
        public string? Year { get; set; }
        public int RuntimeMinutes { get; set; }
        public string? RatingCertificate { get; set; }
        public decimal? Imdb { get; set; }

        // Posters
        public string? PosterUrl { get; set; }   // small/card poster
        public string? BigPoster { get; set; }   // hero background poster

        // Theatre selection (used by Details/Schedule/ShowTickets views)
        public List<TheatreOptionVM> TheatreOptions { get; set; } = new();
        public long? SelectedTheatreId { get; set; }

        // Showtimes grouped (Date -> items)
        public List<ShowtimeBlockVM> DateBlocks { get; set; } = new();
    }
}
