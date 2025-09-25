using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Movie_Site_Management_System.Data;
using Movie_Site_Management_System.Data.Identity;
using Movie_Site_Management_System.Data.Enums;
using Movie_Site_Management_System.Models;
using Movie_Site_Management_System.ViewModels.Movies;
using Movie_Site_Management_System.ViewModels.Shows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Movie_Site_Management_System.Controllers
{
    public class MoviesController : Controller
    {
        private readonly AppDbContext _db;
        public MoviesController(AppDbContext db) => _db = db;

        private static string SafePath(string? path, string fallback)
            => string.IsNullOrWhiteSpace(path) ? fallback : path!;

        // ======================
        // Public pages
        // ======================

        [AllowAnonymous]
        public async Task<IActionResult> Index(string? search)
        {
            IQueryable<Movie> q = _db.Movies.AsNoTracking();
            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim();
                q = q.Where(m => (m.Title ?? string.Empty).Contains(s));
            }

            var movies = await q
                .OrderByDescending(m => m.ReleaseDate)
                .Select(m => new MovieIndexItemVM
                {
                    Id = m.MovieId,
                    Title = m.Title ?? string.Empty,
                    Year = m.ReleaseDate.HasValue ? m.ReleaseDate.Value.Year.ToString() : "",
                    Genre = m.Genre.ToString(),
                    SmallPoster = SafePath(m.SmallPosterPath, "/img/placeholder-small.jpg"),
                    BigPoster = SafePath(m.BigPosterPath, "/img/placeholder-big.jpg"),
                    Imdb = m.ImdbRating ?? 0m
                })
                .ToListAsync();

            var today = DateOnly.FromDateTime(DateTime.UtcNow.Date);

            var movieMap = await _db.Movies.AsNoTracking()
                .Select(m => new
                {
                    m.MovieId,
                    m.Title,
                    m.ReleaseDate,
                    Imdb = m.ImdbRating ?? 0m,
                    Status = m.Status,
                    Genre = m.Genre
                })
                .ToDictionaryAsync(m => m.MovieId);

            var showList = await (
                from s in _db.Shows.AsNoTracking()
                join hs in _db.HallSlots.AsNoTracking()
                     on s.HallSlotId equals hs.HallSlotId into hsj
                from hs in hsj.DefaultIfEmpty()
                join h in _db.Halls.AsNoTracking()
                     on (hs != null ? hs.HallId : 0) equals h.HallId into hj
                from h in hj.DefaultIfEmpty()
                where s.ShowDate >= today
                select new
                {
                    s.MovieId,
                    TheatreId = h != null ? (long?)h.TheatreId : null,
                    s.ShowDate
                }
            ).ToListAsync();

            var data = movieMap.Values.Select(m => new
            {
                id = m.MovieId,
                title = m.Title,
                imdb = m.Imdb,
                status = m.Status.ToString(),
                genre = m.Genre.ToString(),
                releaseDate = m.ReleaseDate.HasValue ? m.ReleaseDate.Value.ToString("yyyy-MM-dd") : null,
                shows = showList
                    .Where(s => s.MovieId == m.MovieId && s.TheatreId.HasValue)
                    .Select(s => new
                    {
                        theatreId = s.TheatreId!.Value,
                        date = s.ShowDate.ToString("yyyy-MM-dd")
                    })
                    .Distinct()
                    .ToList()
            }).ToList();

            var json = System.Text.Json.JsonSerializer.Serialize(
                data,
                new System.Text.Json.JsonSerializerOptions
                {
                    PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase
                }
            );
            ViewBag.ClientMoviesJson = json;

            var vm = new MoviesIndexVM
            {
                Movies = movies,
                Search = search ?? ""
            };

            return View(vm);
        }

        // PUBLIC Details (with theatres/halls/slots)
        [Authorize]
        public async Task<IActionResult> Details(long? id, DateOnly? date)
        {
            if (id == null) return NotFound();

            IQueryable<Movie> q = _db.Movies;
#pragma warning disable CS8602
            q = q
                .Include(m => m.Shows)
                    .ThenInclude(s => s.HallSlot)
                        .ThenInclude(hs => hs.Hall)
                            .ThenInclude(h => h.Theatre);
#pragma warning restore CS8602

            var movie = await q.AsNoTracking()
                .FirstOrDefaultAsync(m => m.MovieId == id);
            if (movie == null) return NotFound();

            var nowLocal = DateTime.Now;
            var today = DateOnly.FromDateTime(nowLocal.Date);
            var nowTime = TimeOnly.FromDateTime(nowLocal);

            var shows = (movie.Shows ?? new List<Show>())
                .Where(s => s.IsActive && !s.IsCancelled)
                .Where(s => s.ShowDate > today
                         || (s.ShowDate == today && s.HallSlot != null && s.HallSlot.StartTime >= nowTime));

            if (date.HasValue)
                shows = shows.Where(s => s.ShowDate == date.Value);

            var orderedShows = shows
                .OrderBy(s => s.ShowDate)
                .ThenBy(s => s.HallSlot != null ? s.HallSlot.StartTime : default)
                .ToList();

            var theatreOptions = orderedShows
                .Where(s => s.HallSlot?.Hall?.Theatre != null)
                .Select(s => new TheatreOptionVM
                {
                    TheatreId = s.HallSlot!.Hall!.Theatre!.TheatreId,
                    Name = s.HallSlot!.Hall!.Theatre!.Name
                })
                .GroupBy(t => new { t.TheatreId, t.Name })
                .Select(g => g.First())
                .OrderBy(t => t.Name)
                .ToList();

            var vm = new MovieDetailsVM
            {
                MovieId = movie.MovieId,
                Title = movie.Title ?? string.Empty,
                Genre = movie.Genre.ToString(),
                Year = movie.ReleaseDate?.Year.ToString() ?? "",
                RuntimeMinutes = movie.RuntimeMinutes,
                RatingCertificate = movie.RatingCertificate,
                Imdb = movie.ImdbRating,
                Synopsis = movie.Synopsis,
                BigPoster = SafePath(movie.BigPosterPath, "/img/placeholder-big.jpg"),
                PosterUrl = SafePath(movie.SmallPosterPath, "/img/placeholder-small.jpg"),
                TheatreOptions = theatreOptions,
                SelectedTheatreId = null,
                DateBlocks = ShowtimeBlockVM.BuildFrom(orderedShows, null)
            };

            ViewBag.SelectedDate = date;
            return View(vm);
        }

        // Public: Schedule (single movie)
        [AllowAnonymous]
        public async Task<IActionResult> Schedule(long id, long? theatreId)
        {
            var movie = await _db.Movies
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.MovieId == id);
            if (movie == null) return NotFound();

            var theatreOptions = await _db.Theatres
                .AsNoTracking()
                .OrderBy(t => t.Name)
                .Select(t => new TheatreOptionVM
                {
                    TheatreId = t.TheatreId,
                    Name = t.Name
                })
                .ToListAsync();

            long? selectedTheatreId = theatreId ?? theatreOptions.FirstOrDefault()?.TheatreId;

            var nowLocal = DateTime.Now;
            var today = DateOnly.FromDateTime(nowLocal.Date);
            var nowTime = TimeOnly.FromDateTime(nowLocal);

            IQueryable<Show> sq = _db.Shows.AsNoTracking();
#pragma warning disable CS8602
            sq = sq
                .Include(s => s.HallSlot)
                    .ThenInclude(hs => hs.Hall)
                        .ThenInclude(h => h.Theatre);
#pragma warning restore CS8602

            var shows = await sq
                .Where(s => s.MovieId == id && s.IsActive && !s.IsCancelled)
                .Where(s => s.ShowDate > today
                         || (s.ShowDate == today && s.HallSlot != null && s.HallSlot.StartTime >= nowTime))
                .Where(s => !selectedTheatreId.HasValue
                         || (s.HallSlot != null
                          && s.HallSlot.Hall != null
                          && s.HallSlot.Hall.TheatreId == selectedTheatreId.Value))
                .OrderBy(s => s.ShowDate)
                .ThenBy(s => s.HallSlot != null ? s.HallSlot.StartTime : default)
                .ToListAsync();

            var vm = new MovieDetailsVM
            {
                MovieId = movie.MovieId,
                Title = movie.Title ?? string.Empty,
                Genre = movie.Genre.ToString(),
                Year = movie.ReleaseDate?.Year.ToString() ?? "",
                RuntimeMinutes = movie.RuntimeMinutes,
                RatingCertificate = movie.RatingCertificate,
                Imdb = movie.ImdbRating,
                Synopsis = movie.Synopsis,
                BigPoster = SafePath(movie.BigPosterPath, "/img/placeholder-big.jpg"),
                PosterUrl = SafePath(movie.SmallPosterPath, "/img/placeholder-small.jpg"),
                TheatreOptions = theatreOptions,
                SelectedTheatreId = selectedTheatreId,
                DateBlocks = ShowtimeBlockVM.BuildFrom(shows, selectedTheatreId)
            };

            return View(vm);
        }

        // Public: ShowTickets (all movies)
        [AllowAnonymous]
        public async Task<IActionResult> ShowTickets(long? theatreId)
        {
            var nowLocal = DateTime.Now;
            var today = DateOnly.FromDateTime(nowLocal.Date);
            var nowTime = TimeOnly.FromDateTime(nowLocal);

            var theatreOptions = await _db.Theatres
                .AsNoTracking()
                .OrderBy(t => t.Name)
                .Select(t => new TheatreOptionVM { TheatreId = t.TheatreId, Name = t.Name })
                .ToListAsync();

            long? selectedTheatreId = theatreId ?? theatreOptions.FirstOrDefault()?.TheatreId;

            IQueryable<Show> sq = _db.Shows.AsNoTracking();
#pragma warning disable CS8602
            sq = sq
                .Include(s => s.HallSlot)
                    .ThenInclude(hs => hs.Hall)
                        .ThenInclude(h => h.Theatre);
#pragma warning restore CS8602

            var upcomingShows = await sq
                .Where(s => s.IsActive && !s.IsCancelled)
                .Where(s => s.ShowDate > today
                         || (s.ShowDate == today && s.HallSlot != null && s.HallSlot.StartTime >= nowTime))
                .OrderBy(s => s.ShowDate)
                .ThenBy(s => s.HallSlot != null ? s.HallSlot.StartTime : default)
                .ToListAsync();

            var movieIdsWithShows = upcomingShows
                .Where(s => !selectedTheatreId.HasValue || (s.HallSlot?.Hall?.TheatreId == selectedTheatreId.Value))
                .Select(s => s.MovieId)
                .Distinct()
                .ToList();

            var movies = await _db.Movies
                .AsNoTracking()
                .Where(m => movieIdsWithShows.Contains(m.MovieId))
                .OrderBy(m => m.Title)
                .ToListAsync();

            var items = new List<MovieScheduleVM>();
            foreach (var m in movies)
            {
                var movieShows = upcomingShows
                    .Where(s => s.MovieId == m.MovieId)
                    .Where(s => !selectedTheatreId.HasValue || (s.HallSlot?.Hall?.TheatreId == selectedTheatreId.Value))
                    .ToList();

                if (movieShows.Count == 0) continue;

                items.Add(new MovieScheduleVM
                {
                    MovieId = m.MovieId,
                    Title = m.Title ?? string.Empty,
                    Genre = m.Genre.ToString(),
                    Year = m.ReleaseDate?.Year.ToString() ?? "",
                    RuntimeMinutes = m.RuntimeMinutes,
                    Imdb = m.ImdbRating,
                    BigPoster = SafePath(m.BigPosterPath, "/img/placeholder-big.jpg"),
                    PosterUrl = SafePath(m.SmallPosterPath, "/img/placeholder-small.jpg"),
                    TheatreOptions = theatreOptions,
                    SelectedTheatreId = selectedTheatreId,
                    DateBlocks = ShowtimeBlockVM.BuildFrom(movieShows, selectedTheatreId)
                });
            }

            var vm = new ShowTicketsIndexVM
            {
                Items = items,
                SelectedTheatreId = selectedTheatreId
            };

            return View(vm);
        }

        [Authorize]
        public IActionResult Ticket(long id, long? theatreId)
            => RedirectToAction(nameof(Schedule), new { id, theatreId });

        // ======================
        // Admin pages
        // ======================

        [Authorize(Roles = Roles.Admin)]
        public async Task<IActionResult> MovieManagement()
        {
            var movies = await _db.Movies.AsNoTracking()
                .OrderBy(m => m.Title)
                .ToListAsync();

            return View("MovieManagement", movies);
        }

        [Authorize(Roles = Roles.Admin)]
        public IActionResult AdminIndex()
            => RedirectToAction(nameof(MovieManagement));

        [Authorize(Roles = Roles.Admin)]
        public IActionResult Create() => View("MovieCrud/Create");

        [Authorize(Roles = Roles.Admin)]
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Movie movie)
        {
            if (!ModelState.IsValid) return View("MovieCrud/Create", movie);
            _db.Movies.Add(movie);
            await _db.SaveChangesAsync();
            TempData["Success"] = $"Movie \"{movie.Title}\" created.";
            return RedirectToAction(nameof(MovieManagement));
        }

        [Authorize(Roles = Roles.Admin)]
        public async Task<IActionResult> Edit(long? id)
        {
            if (id == null) return NotFound();
            var m = await _db.Movies.FindAsync(id.Value);
            if (m == null) return NotFound();
            return View("MovieCrud/Edit", m);
        }

        [Authorize(Roles = Roles.Admin)]
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, Movie movie)
        {
            if (id != movie.MovieId) return NotFound();
            if (!ModelState.IsValid) return View("MovieCrud/Edit", movie);
            _db.Update(movie);
            await _db.SaveChangesAsync();
            TempData["Success"] = $"Movie \"{movie.Title}\" updated.";
            return RedirectToAction(nameof(MovieManagement));
        }

        [Authorize(Roles = Roles.Admin)]
        public async Task<IActionResult> Delete(long? id)
        {
            if (id == null) return NotFound();

            var m = await _db.Movies
                .Include(x => x.Shows)
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.MovieId == id.Value);

            if (m == null) return NotFound();
            return View("MovieCrud/Delete", m);
        }

        [Authorize(Roles = Roles.Admin)]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            // Load movie with full show graph to make a safe decision
            var movie = await _db.Movies
                .Include(m => m.Shows)
                    .ThenInclude(s => s.Bookings)
                        .ThenInclude(b => b.Payments)
                .Include(m => m.Shows)
                    .ThenInclude(s => s.Bookings)
                        .ThenInclude(b => b.BookingSeats)
                .FirstOrDefaultAsync(m => m.MovieId == id);

            if (movie == null)
            {
                TempData["Error"] = "Movie not found.";
                return RedirectToAction(nameof(MovieManagement));
            }

            var shows = movie.Shows ?? new List<Show>();

            // If any ACTIVE (not-cancelled) show remains, block deletion
            var activeShowCount = shows.Count(s => !s.IsCancelled);
            if (activeShowCount > 0)
            {
                TempData["Error"] = $"Cannot delete \"{movie.Title}\" because {activeShowCount} active show(s) reference it.";
                return RedirectToAction(nameof(Delete), new { id = movie.MovieId });
            }

            // If any show has PAID/SUCCESS bookings, also block
            var hasPaid = shows.Any(s => (s.Bookings ?? new List<Booking>())
                .Any(b => (b.Payments ?? new List<Payment>())
                    .Any(p => p.Status == PaymentStatus.Success || p.Status == PaymentStatus.Paid)));

            if (hasPaid)
            {
                TempData["Error"] = $"Cannot delete \"{movie.Title}\" because some related bookings are paid.";
                return RedirectToAction(nameof(Delete), new { id = movie.MovieId });
            }

            // Safe to delete:
            // - Remaining shows are cancelled/orphan
            // - No paid bookings
            await using var tx = await _db.Database.BeginTransactionAsync();
            try
            {
                if (shows.Any())
                {
                    var showIds = shows.Select(s => s.ShowId).ToList();

                    // Delete dependent booking structures for these (cancelled) shows
                    var bookings = _db.Bookings.Where(b => showIds.Contains(b.ShowId));
                    var bookingIds = await bookings.Select(b => b.BookingId).ToListAsync();

                    var bookingSeats = _db.BookingSeats.Where(bs => bookingIds.Contains(bs.BookingId));
                    var payments = _db.Payments.Where(p => bookingIds.Contains(p.BookingId));
                    _db.BookingSeats.RemoveRange(bookingSeats);
                    _db.Payments.RemoveRange(payments);
                    _db.Bookings.RemoveRange(bookings);

                    // Remove ShowSeats snapshots
                    var showSeats = _db.ShowSeats.Where(ss => showIds.Contains(ss.ShowId));
                    _db.ShowSeats.RemoveRange(showSeats);

                    // Finally remove shows themselves
                    _db.Shows.RemoveRange(shows);
                }

                // Remove the movie
                _db.Movies.Remove(movie);

                await _db.SaveChangesAsync();
                await tx.CommitAsync();

                TempData["Success"] = $"Movie \"{movie.Title}\" deleted.";
                return RedirectToAction(nameof(MovieManagement));
            }
            catch
            {
                await tx.RollbackAsync();
                TempData["Error"] = "Delete failed. Please try again.";
                return RedirectToAction(nameof(Delete), new { id = movie.MovieId });
            }
        }

        [Authorize(Roles = Roles.Admin)]
        public async Task<IActionResult> DetailsAdmin(long? id)
        {
            if (id == null) return NotFound();
            var m = await _db.Movies.AsNoTracking().FirstOrDefaultAsync(x => x.MovieId == id.Value);
            if (m == null) return NotFound();
            return View("MovieCrud/Details", m);
        }
    }
}
