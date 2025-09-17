namespace Movie_Site_Management_System.ViewModels.Seats
{
    public class SeatVM
    {
        public long ShowSeatId { get; set; }
        public long SeatId { get; set; }
        public string Label { get; set; } = "";
        public decimal Price { get; set; }
        public short SeatTypeId { get; set; }
        public string SeatType { get; set; } = "";
        public bool IsDisabled { get; set; }
        public bool IsBooked { get; set; }
        public bool IsHeld { get; set; }
    }
}
