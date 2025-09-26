namespace Movie_Site_Management_System.ViewModels.Halls
{
    public class HallDeleteVM
    {
        public long Id { get; set; }
        public string HallName { get; set; } = "";
        public string TheatreName { get; set; } = "";

        public int Slots { get; set; }
        public int Seats { get; set; }
        public int Shows { get; set; }

        // Seats referenced anywhere (ShowSeats / BookingSeats / SeatBlocks)
        public int SeatLinks { get; set; }

        // We allow “delete seats with hall” only when those seats are not linked anywhere
        public bool CanDeleteSeatsSafely => Seats > 0 && SeatLinks == 0 && Shows == 0;
    }
}
