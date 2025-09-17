using System;
using System.Collections.Generic;

namespace Movie_Site_Management_System.ViewModels.Shows
{
    public class ShowDateBlockVM
    {
        public DateOnly Date { get; set; }
        public List<ShowtimeItemVM> Times { get; set; } = new();
    }
}
