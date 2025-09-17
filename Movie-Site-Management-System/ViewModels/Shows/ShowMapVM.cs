using System.Collections.Generic;

namespace Movie_Site_Management_System.ViewModels.Shows
{
    /// <summary>
    /// Seat map VM that works with both:
    ///  - a flat list (Seats) for simple Razor grouping (your current Map.cshtml)
    ///  - optional row/seat structure (Rows) if you need it elsewhere
    /// </summary>
    public class ShowMapVM
    {
        public long ShowId { get; set; }
        public string MovieTitle { get; set; } = "";
        public string TheatreName { get; set; } = "";
        public string HallName { get; set; } = "";
        public DateOnly Date { get; set; }
        public TimeOnly StartTime { get; set; }

        // 👉 Your current Map.cshtml expects this:
        public List<ShowSeatCellVM> Seats { get; set; } = new();

        // Keep Rows for any older code that might already use it (safe to keep both)
        public List<RowVM> Rows { get; set; } = new();
    }
}
