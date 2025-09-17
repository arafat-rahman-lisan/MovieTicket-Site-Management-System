namespace Movie_Site_Management_System.ViewModels.Movies
{
    public class MovieIndexItemVM
    {
        public long Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Year { get; set; } = string.Empty;
        public string Genre { get; set; } = string.Empty;
        public string SmallPoster { get; set; } = string.Empty;
        public string BigPoster { get; set; } = string.Empty;
        public decimal Imdb { get; set; }
    }
}
