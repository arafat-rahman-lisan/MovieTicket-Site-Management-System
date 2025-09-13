namespace Movie_Site_Management_System.ViewModels.Movies
{
    public class ShowTimeChipVM
    {
        public long ShowId { get; set; }
        public TimeOnly Start { get; set; }
        public string HallName { get; set; } = "";
    }
}
