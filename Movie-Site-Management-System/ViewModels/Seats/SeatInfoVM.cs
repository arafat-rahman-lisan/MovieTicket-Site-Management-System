using Movie_Site_Management_System.Data.Enums;

public class SeatInfoVM
{
    public long SeatId { get; set; }
    public string RowLabel { get; set; } = string.Empty;
    public int SeatNumber { get; set; }
    public short SeatTypeId { get; set; }
    public ShowSeatStatus Status { get; set; }
    public decimal Price { get; set; }
}