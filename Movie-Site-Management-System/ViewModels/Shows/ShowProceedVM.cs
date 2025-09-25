using System;
using System.Collections.Generic;

namespace Movie_Site_Management_System.ViewModels.Shows
{
    public class ShowProceedVM
    {
        public long ShowId { get; set; }

        // Non-null defaults to satisfy CS8618
        public string MovieTitle { get; set; } = string.Empty;
        public string TheatreName { get; set; } = string.Empty;
        public string HallName { get; set; } = string.Empty;

        // Match your project (you use DateOnly/TimeOnly elsewhere)
        public DateOnly Date { get; set; }
        public TimeOnly StartTime { get; set; }

        // Never null to the view
        public List<ShowSeatCellVM> Seats { get; set; } = new();

        public decimal Total { get; set; }
    }
}
