using Movie_Site_Management_System.Data.Enums;

namespace Movie_Site_Management_System.ViewModels.Shows
{
    public class ShowSeatCellVM
    {
        // NOTE: This is the ShowSeatId snapshot (kept as SeatId in your UI contract)
        public long SeatId { get; set; }

        public string RowLabel { get; set; } = "?";
        public int SeatNumber { get; set; }
        public decimal Price { get; set; }
        public ShowSeatStatus Status { get; set; }

        // Seat-level toggle (broken seat, etc.)
        public bool IsDisabled { get; set; }

        // NEW: time-based block (SeatBlocks overlap with show time)
        public bool IsBlocked { get; set; }
    }
}
