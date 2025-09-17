using System;

namespace Movie_Site_Management_System.ViewModels.Shows
{
    public class ShowtimeItemVM
    {
        public long ShowId { get; set; }
        public string Hall { get; set; } = string.Empty;
        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }
    }
}
