using Movie_Site_Management_System.ViewModels.Shows;

namespace Movie_Site_Management_System.ViewModels.Movies
{
    public sealed class MovieScheduleVM
    {
        public long MovieId { get; set; }
        public string Title { get; set; } = "";
        public string Genre { get; set; } = "";
        public string Year { get; set; } = "";
        public int? RuntimeMinutes { get; set; }
        public decimal? Imdb { get; set; }
        public string? BigPoster { get; set; }
        public string? PosterUrl { get; set; }

        public List<TheatreOptionVM> TheatreOptions { get; set; } = new();
        public long? SelectedTheatreId { get; set; }

        // Reuse your existing grouping
        public List<ShowtimeBlockVM> DateBlocks { get; set; } = new();
    }
}
