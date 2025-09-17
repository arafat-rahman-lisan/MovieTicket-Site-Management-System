using System;
using System.Collections.Generic;
using System.Linq;
using Movie_Site_Management_System.Models;

namespace Movie_Site_Management_System.ViewModels.Shows
{
    public class ShowtimeBlockVM
    {
        public DateOnly Date { get; set; }
        public List<ShowtimeItemVM> Items { get; set; } = new();

        public static List<ShowtimeBlockVM> BuildFrom(IEnumerable<Show> shows, long? theatreId)
        {
            if (shows == null) return new List<ShowtimeBlockVM>();

            var filtered = theatreId.HasValue
                ? shows.Where(s => s.HallSlot?.Hall?.TheatreId == theatreId.Value)
                : shows;

            return filtered
                .GroupBy(s => s.ShowDate) // ShowDate is DateOnly
                .Select(g => new ShowtimeBlockVM
                {
                    Date = g.Key,
                    Items = g.Select(s => new ShowtimeItemVM
                    {
                        ShowId = s.ShowId,
                        Hall = s.HallSlot?.Hall?.Name ?? string.Empty,
                        StartTime = s.HallSlot?.StartTime ?? default(TimeOnly),
                        EndTime = s.HallSlot?.EndTime ?? default(TimeOnly)
                    }).ToList()
                })
                .OrderBy(b => b.Date)
                .ToList();
        }
    }
}