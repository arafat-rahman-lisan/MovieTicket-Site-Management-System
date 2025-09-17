namespace Movie_Site_Management_System.ViewModels.Theatres
{
    public class TheatreLocationVM
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        // Optional fields if you have them:
        public string? City { get; set; }
        public string? Area { get; set; }
    }
}
