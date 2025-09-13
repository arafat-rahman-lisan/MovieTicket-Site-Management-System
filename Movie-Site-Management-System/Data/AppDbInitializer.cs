using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Movie_Site_Management_System.Data.Enums;
using Movie_Site_Management_System.Models;

// Enums namespace (adjust if needed)

namespace Movie_Site_Management_System.Data
{
    public static class AppDbInitializer
    {
        public static async Task SeedAsync(IApplicationBuilder app)
        {
            using var scope = app.ApplicationServices.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            // Ensure DB & migrations
            await context.Database.MigrateAsync();

            // Seed order matters
            await SeedSeatTypesAsync(context);
            await SeedMoviesAsync(context);

            // Fill poster paths for already-seeded movies (non-destructive)
            await SeedMoviePostersAsync(context);

            await SeedTheatresHallsSeatsAsync(context);
            await SeedHallSlotsAsync(context);
            await SeedShowsAsync(context);
            await SeedShowSeatsSnapshotAsync(context); // optional but useful
        }

        // ---------- SeatTypes ----------
        private static async Task SeedSeatTypesAsync(AppDbContext context)
        {
            if (await context.SeatTypes.AnyAsync()) return;

            var seatTypes = new List<SeatType>
            {
                new SeatType { Name = "Regular",  Description = "Standard seating",    BasePrice = 450m },
                new SeatType { Name = "Premium",  Description = "Better view/legroom", BasePrice = 650m },
                new SeatType { Name = "VIP",      Description = "Recliner prime rows", BasePrice = 900m }
            };

            await context.SeatTypes.AddRangeAsync(seatTypes);
            await context.SaveChangesAsync();
        }

        // ---------- Movies ----------
        private static async Task SeedMoviesAsync(AppDbContext context)
        {
            if (await context.Movies.AnyAsync()) return;

            var movies = new List<Movie>
            {
                new Movie { Title = "The Boys",             ImdbRating = 9.3m, ReleaseDate = Year(2022), Genre = MovieGenre.Action,    Status = MovieStatus.Upcoming,   RuntimeMinutes = 120 },
                new Movie { Title = "Money Heist",          ImdbRating = 9.9m, ReleaseDate = Year(2020), Genre = MovieGenre.Thriller,  Status = MovieStatus.NowShowing, RuntimeMinutes = 120 },
                new Movie { Title = "John Wick",            ImdbRating = 9.8m, ReleaseDate = Year(2022), Genre = MovieGenre.Action,    Status = MovieStatus.NowShowing, RuntimeMinutes = 120 },
                new Movie { Title = "Ant Man",              ImdbRating = 8.9m, ReleaseDate = Year(2017), Genre = MovieGenre.SciFi,     Status = MovieStatus.NowShowing, RuntimeMinutes = 120 },
                new Movie { Title = "Avengers",             ImdbRating = 9.9m, ReleaseDate = Year(2012), Genre = MovieGenre.Superhero, Status = MovieStatus.NowShowing, RuntimeMinutes = 120 },
                new Movie { Title = "Moon Knight",          ImdbRating = 7.3m, ReleaseDate = Year(2022), Genre = MovieGenre.Superhero, Status = MovieStatus.Upcoming,   RuntimeMinutes = 120 },
                new Movie { Title = "Kota Factory",         ImdbRating = 9.6m, ReleaseDate = Year(2020), Genre = MovieGenre.Drama,     Status = MovieStatus.Upcoming,   RuntimeMinutes = 120 },
                new Movie { Title = "Collage Romance",      ImdbRating = 7.9m, ReleaseDate = Year(2021), Genre = MovieGenre.Romance,   Status = MovieStatus.Upcoming,   RuntimeMinutes = 120 },
                new Movie { Title = "Thor Love Of Thunder", ImdbRating = 8.8m, ReleaseDate = Year(2022), Genre = MovieGenre.Superhero, Status = MovieStatus.NowShowing, RuntimeMinutes = 120 },
                new Movie { Title = "Uncharted",            ImdbRating = 9.8m, ReleaseDate = Year(2022), Genre = MovieGenre.Adventure, Status = MovieStatus.NowShowing, RuntimeMinutes = 120 },
                new Movie { Title = "Eesho",                ImdbRating = 8.2m, ReleaseDate = Year(2022), Genre = MovieGenre.Thriller,  Status = MovieStatus.NowShowing, RuntimeMinutes = 120 },
                new Movie { Title = "Top Gun",              ImdbRating = 8.0m, ReleaseDate = Year(2022), Genre = MovieGenre.Action,    Status = MovieStatus.NowShowing, RuntimeMinutes = 120 },
                new Movie { Title = "Jurassic World",       ImdbRating = 8.0m, ReleaseDate = Year(2022), Genre = MovieGenre.SciFi,     Status = MovieStatus.NowShowing, RuntimeMinutes = 120 },
                new Movie { Title = "Eternals",             ImdbRating = 9.0m, ReleaseDate = Year(2022), Genre = MovieGenre.Superhero, Status = MovieStatus.Upcoming,   RuntimeMinutes = 120 },
                new Movie { Title = "Spider Man",           ImdbRating = 9.9m, ReleaseDate = Year(2020), Genre = MovieGenre.Superhero, Status = MovieStatus.NowShowing, RuntimeMinutes = 120 },
            };

            await context.Movies.AddRangeAsync(movies);
            await context.SaveChangesAsync();

            static DateTime Year(int y) => new DateTime(y, 1, 1);
        }

        // ----------- Posters (non-destructive patch) -----------
        private static async Task SeedMoviePostersAsync(AppDbContext context)
        {
            // Title -> (SmallPosterPath, BigPosterPath)
            var posters = new Dictionary<string, (string sp, string bp)>(StringComparer.OrdinalIgnoreCase)
            {
                { "The Boys",                 ("/img/the boys.jpg",                 "/img/the boys1.jpg") },
                { "Money Heist",              ("/img/money heist.jpg",              "/img/money heist1.jpg") },
                { "John Wick",                ("/img/Jhon Wick.jpg",                "/img/jhon wick1.webp") },
                { "Ant Man",                  ("/img/ant man.jpg",                  "/img/ant man1.jpg") },
                { "Avengers",                 ("/img/avengers.jpg",                 "/img/avengers1.jpg") },
                { "Moon Knight",              ("/img/moon knight.jpg",              "/img/moon knight1.webp") },
                { "Kota Factory",             ("/img/kota factory.jpg",             "/img/kota factory1.webp") },
                { "Collage Romance",          ("/img/collage romance.jpg",          "/img/collage romance1.jpg") },
                { "Thor Love Of Thunder",     ("/img/thor love of thunder.jpg",     "/img/thor love of thunder1.avif") },
                { "Uncharted",                ("/img/uncharted.webp",               "/img/uncharted1.jpg") },
                { "Eesho",                    ("/img/eesho.jpeg",                   "/img/eesho1.jpg") },
                { "Top Gun",                  ("/img/topgun.jpg",                   "/img/topgun1.jpg") },
                { "Jurassic World",           ("/img/jurassic world.jpg",           "/img/jurassic world1.jpg") },
                { "Eternals",                 ("/img/eternals.jpg",                 "/img/eternals1.webp") },
                { "Spider Man",               ("/img/spiderman.jpg",                "/img/spiderman1.jpg") },
            };

            const string fallbackSmall = "/img/placeholder-small.jpg";
            const string fallbackBig = "/img/placeholder-big.jpg";

            var movies = await context.Movies.ToListAsync();

            foreach (var m in movies)
            {
                var hasSmall = !string.IsNullOrWhiteSpace(m.SmallPosterPath);
                var hasBig = !string.IsNullOrWhiteSpace(m.BigPosterPath);
                if (hasSmall && hasBig) continue;

                if (!string.IsNullOrWhiteSpace(m.Title) && posters.TryGetValue(m.Title, out var p))
                {
                    if (!hasSmall) m.SmallPosterPath = p.sp;
                    if (!hasBig) m.BigPosterPath = p.bp;
                }
                else
                {
                    if (!hasSmall) m.SmallPosterPath = fallbackSmall;
                    if (!hasBig) m.BigPosterPath = fallbackBig;
                }
            }

            await context.SaveChangesAsync();
        }

        // ---------- Theatres, Halls, Seats ----------
        private static async Task SeedTheatresHallsSeatsAsync(AppDbContext context)
        {
            if (await context.Theatres.AnyAsync()) return;

            var now = DateTime.UtcNow;

            var t1 = new Theatre { Name = "Star Cineplex Bashundhara", City = "Dhaka", IsActive = true, CreatedAt = now, Address = "Bashundhara City, Panthapath", Lat = 23.751200m, Lng = 90.390600m };
            var t2 = new Theatre { Name = "Star Cineplex Mirpur", City = "Dhaka", IsActive = true, CreatedAt = now, Address = "Mirpur DOHS", Lat = 23.829500m, Lng = 90.365800m };

            var halls = new List<Hall>
            {
                new Hall { Theatre = t1, Name = "Hall A", Capacity = 25, SeatmapVersion = 1, IsActive = true },
                new Hall { Theatre = t1, Name = "Hall B", Capacity = 25, SeatmapVersion = 1, IsActive = true },
                new Hall { Theatre = t2, Name = "Hall 1", Capacity = 25, SeatmapVersion = 1, IsActive = true },
            };

            await context.Theatres.AddRangeAsync(t1, t2);
            await context.Halls.AddRangeAsync(halls);
            await context.SaveChangesAsync();

            // Build seats for each hall (A=Regular, B=Premium, C=VIP)
            var regular = await context.SeatTypes.FirstAsync(s => s.Name == "Regular");
            var premium = await context.SeatTypes.FirstAsync(s => s.Name == "Premium");
            var vip = await context.SeatTypes.FirstAsync(s => s.Name == "VIP");

            var allSeats = new List<Seat>();
            foreach (var hall in halls)
            {
                allSeats.AddRange(BuildRow(hall.HallId, "A", 1, 10, regular.SeatTypeId));
                allSeats.AddRange(BuildRow(hall.HallId, "B", 1, 10, premium.SeatTypeId));
                allSeats.AddRange(BuildRow(hall.HallId, "C", 1, 5, vip.SeatTypeId));
            }

            await context.Seats.AddRangeAsync(allSeats);
            await context.SaveChangesAsync();

            static IEnumerable<Seat> BuildRow(long hallId, string rowLabel, int from, int to, short seatTypeId)
            {
                var list = new List<Seat>();
                var y = rowLabel switch { "A" => 1, "B" => 2, "C" => 3, _ => 0 };
                for (int n = from; n <= to; n++)
                {
                    list.Add(new Seat
                    {
                        HallId = hallId,
                        RowLabel = rowLabel,
                        SeatNumber = n,
                        SeatTypeId = seatTypeId,
                        PosX = n,
                        PosY = y,
                        IsDisabled = false
                    });
                }
                return list;
            }
        }

        // ---------- HallSlots ----------
        private static async Task SeedHallSlotsAsync(AppDbContext context)
        {
            if (await context.HallSlots.AnyAsync()) return;

            // 3 slots per hall
            var halls = await context.Halls.AsNoTracking().ToListAsync();
            var slots = new List<HallSlot>();

            foreach (var h in halls)
            {
                slots.Add(new HallSlot
                {
                    HallId = h.HallId,
                    SlotNumber = 1,
                    StartTime = new TimeOnly(12, 00),
                    EndTime = new TimeOnly(14, 30),
                    IsActive = true
                });
                slots.Add(new HallSlot
                {
                    HallId = h.HallId,
                    SlotNumber = 2,
                    StartTime = new TimeOnly(16, 00),
                    EndTime = new TimeOnly(18, 30),
                    IsActive = true
                });
                slots.Add(new HallSlot
                {
                    HallId = h.HallId,
                    SlotNumber = 3,
                    StartTime = new TimeOnly(20, 00),
                    EndTime = new TimeOnly(22, 30),
                    IsActive = true
                });
            }

            await context.HallSlots.AddRangeAsync(slots);
            await context.SaveChangesAsync();
        }

        // ---------- Shows (unique per (HallSlotId, ShowDate)) ----------
        private static async Task SeedShowsAsync(AppDbContext context)
        {
            if (await context.Shows.AnyAsync()) return;

            var today = DateOnly.FromDateTime(DateTime.UtcNow.Date);

            var hallSlots = await context.HallSlots
                .AsNoTracking()
                .OrderBy(hs => hs.HallId)
                .ThenBy(hs => hs.SlotNumber)
                .ToListAsync();

            var pick = await context.Movies
                .Where(m => m.Status == MovieStatus.NowShowing)
                .OrderByDescending(m => m.ImdbRating)
                .Take(6)
                .ToListAsync();

            if (pick.Count == 0 || hallSlots.Count == 0) return;

            int idx = 0;
            var shows = new List<Show>();

            foreach (var hs in hallSlots)
            {
                var movie = pick[idx % pick.Count];
                idx++;

                shows.Add(new Show
                {
                    MovieId = movie.MovieId,
                    HallSlotId = hs.HallSlotId,
                    ShowDate = today,
                    Language = "English",
                    IsActive = true,
                    IsCancelled = false,
                    CreatedAt = DateTime.UtcNow
                });
            }

            await context.Shows.AddRangeAsync(shows);
            await context.SaveChangesAsync();
        }

        // ---------- ShowSeats snapshot ----------
        private static async Task SeedShowSeatsSnapshotAsync(AppDbContext context)
        {
            if (await context.ShowSeats.AnyAsync()) return;

            var shows = await context.Shows.AsNoTracking().ToListAsync();
            if (shows.Count == 0) return;

            // Snapshot price per seat type at time of seeding
            var seatTypePrice = await context.SeatTypes
                .Select(st => new { st.SeatTypeId, st.BasePrice })
                .ToDictionaryAsync(x => x.SeatTypeId, x => x.BasePrice);

            // Map HallSlot -> Hall
            var hallIdByHallSlot = await context.HallSlots
                .Select(hs => new { hs.HallSlotId, hs.HallId })
                .ToDictionaryAsync(x => x.HallSlotId, x => x.HallId);

            // Seats by hall
            var seatsByHall = await context.Seats
                .GroupBy(s => s.HallId)
                .ToDictionaryAsync(g => g.Key, g => g.Select(s => new { s.SeatId, s.SeatTypeId }).ToList());

            var showSeats = new List<ShowSeat>();

            foreach (var show in shows)
            {
                var hallId = hallIdByHallSlot[show.HallSlotId];
                var seats = seatsByHall[hallId];

                foreach (var seat in seats)
                {
                    showSeats.Add(new ShowSeat
                    {
                        ShowId = show.ShowId,
                        SeatId = seat.SeatId,

                        // If ShowSeat.Status is a string, use: Status = "Available",
                        Status = ShowSeatStatus.Available,

                        HoldExpiresAt = null,
                        PriceAtBooking = seatTypePrice.TryGetValue(seat.SeatTypeId, out var p) ? p : null
                    });
                }
            }

            await context.ShowSeats.AddRangeAsync(showSeats);
            await context.SaveChangesAsync();
        }
    }
}
