using System.Collections.Generic;

namespace Movie_Site_Management_System.ViewModels.Movies
{
    public sealed class ShowTicketsIndexVM
    {
        public List<MovieScheduleVM> Items { get; set; } = new();
        public long? SelectedTheatreId { get; set; }
    }
}
