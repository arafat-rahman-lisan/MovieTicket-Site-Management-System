namespace Movie_Site_Management_System.ViewModels.Movies
{
    public class ShowDateBlockVM
    {
        public DateOnly Date { get; set; }
        public IReadOnlyList<ShowTimeChipVM> Times { get; set; } = new List<ShowTimeChipVM>();
    }
}
