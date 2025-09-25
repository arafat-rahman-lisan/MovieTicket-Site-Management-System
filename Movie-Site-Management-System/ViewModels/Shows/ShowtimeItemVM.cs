using System;

namespace Movie_Site_Management_System.ViewModels.Shows
{
    public class ShowtimeItemVM
    {
        public long ShowId { get; set; }

        // Display labels
        public string Theatre { get; set; } = string.Empty;   // e.g., "Bashundhara Cineplex"
        public string Hall { get; set; } = string.Empty;      // e.g., "Hall A"

        // Optional IDs (handy if you need links or grouping keys later)
        public long? TheatreId { get; set; }
        public long? HallId { get; set; }

        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }
    }
}
