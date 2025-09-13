
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Movie_Site_Management_System.Data;
using Movie_Site_Management_System.ViewModels.Movies;

namespace Movie_Site_Management_System.Controllers
{
    public class MoviesController : Controller
    {
        private readonly AppDbContext _db;
        public MoviesController(AppDbContext db) => _db = db;

        // =======================
        // GET: /Movies?search=...
        // =======================
        public async Task<IActionResult> Index(string? search)
        {
            var q = _db.Movies.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim();
                q = q.Where(m =>
                    m.Title.Contains(s) ||
                    m.Genre.ToString().Contains(s) ||
                    m.ReleaseDate.HasValue && m.ReleaseDate.Value.Year.ToString().Contains(s)
                );
            }

            var list = await q
                .OrderByDescending(m => m.ImdbRating ?? 0m)
                .Select(m => new MovieCardVM
                {
                    Id = m.MovieId,
                    Title = m.Title,
                    Imdb = m.ImdbRating ?? 0m,
                    Year = m.ReleaseDate.HasValue ? m.ReleaseDate.Value.Year.ToString() : "",
                    Genre = m.Genre.ToString(),
                    SmallPoster = string.IsNullOrWhiteSpace(m.SmallPosterPath)
                        ? "/img/placeholder-small.jpg"
                        : m.SmallPosterPath!,
                    BigPoster = string.IsNullOrWhiteSpace(m.BigPosterPath)
                        ? "/img/placeholder-big.jpg"
                        : m.BigPosterPath!
                })
                .ToListAsync();

            return View(new MoviesIndexVM { Movies = list, Search = search });
        }

        // =======================
        // GET: /Movies/Details/5
        // =======================
        public async Task<IActionResult> Details(long id, long? theatreId)
        {
            var movie = await _db.Movies.AsNoTracking()
                .FirstOrDefaultAsync(m => m.MovieId == id);

            if (movie == null) return NotFound();

            var today = DateOnly.FromDateTime(DateTime.UtcNow.Date);

            // Theatre options
            var theatreOptions = await _db.Shows.AsNoTracking()
                .Where(s => s.MovieId == id && s.IsActive && !s.IsCancelled && s.ShowDate >= today)
                .Select(s => new
                {
                    s.HallSlot.Hall.Theatre.TheatreId,
                    s.HallSlot.Hall.Theatre.Name,
                    Location = s.HallSlot.Hall.Theatre.Address
                })
                .Distinct()
                .OrderBy(t => t.Name)
                .Select(t => new TheatreOptionVM
                {
                    TheatreId = t.TheatreId,
                    Name = t.Name,
                    Location = t.Location
                })
                .ToListAsync();

            var selectedTheatreId = theatreId ?? theatreOptions.FirstOrDefault()?.TheatreId;

            // Build showtimes query
            IQueryable<Models.Show> showsQ = _db.Shows.AsNoTracking()
                .Where(s => s.MovieId == id && s.IsActive && !s.IsCancelled && s.ShowDate >= today);

            if (selectedTheatreId.HasValue)
            {
                showsQ = showsQ.Where(s => s.HallSlot.Hall.TheatreId == selectedTheatreId.Value);
            }

            var shows = await showsQ
                .Include(s => s.HallSlot)
                    .ThenInclude(hs => hs.Hall)
                    .ThenInclude(h => h.Theatre)
                .ToListAsync();

            var dateBlocks = shows
                .GroupBy(s => s.ShowDate)
                .Select(g => new ShowDateBlockVM
                {
                    Date = g.Key,
                    Times = g.OrderBy(x => x.HallSlot.StartTime)
                             .Select(x => new ShowTimeChipVM
                             {
                                 ShowId = x.ShowId,
                                 Start = x.HallSlot.StartTime,
                                 HallName = x.HallSlot.Hall.Name
                             })
                             .ToList()
                })
                .OrderBy(b => b.Date)
                .ToList();

            // Map to VM
            var vm = new MovieDetailsVM
            {
                MovieId = movie.MovieId,
                Title = movie.Title,
                Synopsis = movie.Synopsis,
                RuntimeMinutes = movie.RuntimeMinutes,
                RatingCertificate = movie.RatingCertificate,
                PosterUrl = movie.PosterUrl,
                SmallPoster = movie.SmallPosterPath,
                BigPoster = movie.BigPosterPath,
                Imdb = movie.ImdbRating,
                Year = movie.ReleaseDate.HasValue ? movie.ReleaseDate.Value.Year.ToString() : "",
                Genre = movie.Genre.ToString(),

                SelectedTheatreId = selectedTheatreId,
                SelectedTheatreName = theatreOptions.FirstOrDefault(t => t.TheatreId == selectedTheatreId)?.Name,
                TheatreOptions = theatreOptions,
                DateBlocks = dateBlocks
            };

            return View(vm);
        }
    }
}
