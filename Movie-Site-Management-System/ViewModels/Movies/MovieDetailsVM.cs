namespace Movie_Site_Management_System.ViewModels.Movies
{
    public class MovieDetailsVM
    {
        // Basic info
        public long MovieId { get; set; }
        public string Title { get; set; } = "";
        public string? Synopsis { get; set; }
        public int RuntimeMinutes { get; set; }
        public string? RatingCertificate { get; set; }
        public string? PosterUrl { get; set; }
        public string? SmallPoster { get; set; }
        public string? BigPoster { get; set; }
        public decimal? Imdb { get; set; }
        public string Year { get; set; } = "";
        public string Genre { get; set; } = "—";

        // Showtime info
        public long? SelectedTheatreId { get; set; }
        public string? SelectedTheatreName { get; set; }
        public IReadOnlyList<TheatreOptionVM> TheatreOptions { get; set; } = new List<TheatreOptionVM>();
        public IReadOnlyList<ShowDateBlockVM> DateBlocks { get; set; } = new List<ShowDateBlockVM>();
    }
}
