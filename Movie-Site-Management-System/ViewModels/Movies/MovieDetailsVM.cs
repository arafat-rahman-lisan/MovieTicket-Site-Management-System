using Movie_Site_Management_System.ViewModels.Shows;
using System;
using System.Collections.Generic;

namespace Movie_Site_Management_System.ViewModels.Movies
{
    public class MovieDetailsVM
    {
        public long MovieId { get; set; }
        public string Title { get; set; } = "";
        public string Genre { get; set; } = "—";
        public string Year { get; set; } = "";
        public int RuntimeMinutes { get; set; }
        public string? RatingCertificate { get; set; }
        public decimal? Imdb { get; set; }
        public string? Synopsis { get; set; }

        // Posters
        public string? BigPoster { get; set; }
        public string? PosterUrl { get; set; }

        // Theatre picker
        public List<TheatreOptionVM> TheatreOptions { get; set; } = new();
        public long? SelectedTheatreId { get; set; }

        // Showtimes
        public List<ShowtimeBlockVM> DateBlocks { get; set; } = new();
    }
}
