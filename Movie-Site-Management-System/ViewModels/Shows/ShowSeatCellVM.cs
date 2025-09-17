using System.Collections.Generic;
using Movie_Site_Management_System.Data.Enums;

namespace Movie_Site_Management_System.ViewModels.Shows
{
    
    public class ShowSeatCellVM
    {
        public long SeatId { get; set; }       // ← snapshot id: ShowSeatId
        public string RowLabel { get; set; } = "?";
        public int SeatNumber { get; set; }
        public decimal Price { get; set; }
        public ShowSeatStatus Status { get; set; }
    }
}
