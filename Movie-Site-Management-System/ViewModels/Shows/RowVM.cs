using Movie_Site_Management_System.ViewModels.Seats;

namespace Movie_Site_Management_System.ViewModels.Shows
{
    public class RowVM
    {
        public string Row { get; set; } = "";
        public List<SeatVM> Seats { get; set; } = new();
    }
}
