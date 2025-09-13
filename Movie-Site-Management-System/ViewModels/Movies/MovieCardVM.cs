namespace Movie_Site_Management_System.ViewModels.Movies
{
    public class MovieCardVM
    {
        public long Id { get; set; }
        public string Title { get; set; } = "";
        public decimal Imdb { get; set; }          // non-null for display
        public string Year { get; set; } = "";
        public string Genre { get; set; } = "—";
        public string SmallPoster { get; set; } = "";
        public string BigPoster { get; set; } = "";
    }
}
