using Movie_Site_Management_System.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Movie_Site_Management_System.ViewModels.Shows
{
    public class ShowtimeBlockVM
    {
        public DateOnly Date { get; set; }
        public List<ShowtimeItemVM> Items { get; set; } = new();

        /// <summary>
        /// Build blocks grouped by date. If theatreId is provided, filter to that theatre.
        /// Only pass present/future shows to this method (controller handles time filtering).
        /// </summary>
        public static List<ShowtimeBlockVM> BuildFrom(IEnumerable<Show> shows, long? theatreId)
        {
            if (shows == null) return new List<ShowtimeBlockVM>();

            // Theatre filter only if provided (used by Schedule page)
            var filtered = shows.Where(s =>
            {
                if (!theatreId.HasValue) return true;

                var hall = s.HallSlot?.Hall;
                return hall != null && hall.TheatreId == theatreId.Value;
            });

            // Group by date
            var byDate = filtered
                .GroupBy(s => s.ShowDate)
                .OrderBy(g => g.Key);

            var result = new List<ShowtimeBlockVM>();

            foreach (var g in byDate)
            {
                var items = g
                    .OrderBy(s => s.HallSlot?.StartTime)
                    .Select(s => new ShowtimeItemVM
                    {
                        ShowId = s.ShowId,
                        TheatreId = s.HallSlot?.Hall?.Theatre?.TheatreId,
                        Theatre = s.HallSlot?.Hall?.Theatre?.Name ?? "",
                        HallId = s.HallSlot?.Hall?.HallId,
                        Hall = s.HallSlot?.Hall?.Name ?? "",
                        StartTime = s.HallSlot?.StartTime ?? default,
                        EndTime = s.HallSlot?.EndTime ?? default
                    })
                    .ToList();

                result.Add(new ShowtimeBlockVM
                {
                    Date = g.Key,
                    Items = items
                });
            }

            return result;
        }
    }
}
