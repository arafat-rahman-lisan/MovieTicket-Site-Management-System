using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Movie_Site_Management_System.Data;
using Movie_Site_Management_System.Data.Identity;
using Movie_Site_Management_System.Models;
using Movie_Site_Management_System.ViewModels.Movies;
using Movie_Site_Management_System.ViewModels.Shows;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Movie_Site_Management_System.Controllers
{
    public class MoviesController : Controller
    {
        private readonly AppDbContext _db;
        public MoviesController(AppDbContext db) => _db = db;

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
                q = q.Where(m => m.Title.Contains(s));
            }

            var movies = await q
                .OrderByDescending(m => m.ReleaseDate)
                .Select(m => new MovieIndexItemVM
                {
                    Id = m.MovieId,
                    Title = m.Title,
                    Year = m.ReleaseDate.HasValue ? m.ReleaseDate.Value.Year.ToString() : "",
                    Genre = m.Genre.ToString(),
                    SmallPoster = string.IsNullOrWhiteSpace(m.SmallPosterPath)
                        ? "/img/placeholder-small.jpg"
                        : m.SmallPosterPath!,
                    BigPoster = string.IsNullOrWhiteSpace(m.BigPosterPath)
                        ? "/img/placeholder-big.jpg"
                        : m.BigPosterPath!,
                    Imdb = m.ImdbRating ?? 0m
                })
                .ToListAsync();

            var vm = new MoviesIndexVM
            {
                Movies = movies,
                Search = search ?? ""
            };

            return View(vm);
        }

        // Login required -> ensures ReturnUrl brings user back here after login
        [Authorize]
        public async Task<IActionResult> Details(long? id, long? theatreId)
        {
            if (id == null) return NotFound();

#pragma warning disable CS8602
            var movie = await _db.Movies
                .Include(m => m.Shows)
                    .ThenInclude(s => s.HallSlot)
                        .ThenInclude(hs => hs.Hall)
                            .ThenInclude(h => h.Theatre)
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.MovieId == id);
#pragma warning restore CS8602

            if (movie == null) return NotFound();

            var shows = movie.Shows ?? new List<Show>();

            var theatreOptions = shows
                .Where(s => s.HallSlot?.Hall?.Theatre != null)
                .Select(s => new TheatreOptionVM
                {
                    TheatreId = s.HallSlot!.Hall!.Theatre!.TheatreId,
                    TheatreName = s.HallSlot!.Hall!.Theatre!.Name
                })
                .Distinct()
                .OrderBy(t => t.TheatreName)
                .ToList();

            var selectedTheatreId = theatreId ?? theatreOptions.FirstOrDefault()?.TheatreId;

            var vm = new MovieDetailsVM
            {
                MovieId = movie.MovieId,
                Title = movie.Title,
                Genre = movie.Genre.ToString(),
                Year = movie.ReleaseDate?.Year.ToString() ?? "",
                RuntimeMinutes = movie.RuntimeMinutes,
                RatingCertificate = movie.RatingCertificate,
                Imdb = movie.ImdbRating,
                Synopsis = movie.Synopsis,
                BigPoster = string.IsNullOrWhiteSpace(movie.BigPosterPath) ? "/img/placeholder-big.jpg" : movie.BigPosterPath,
                PosterUrl = string.IsNullOrWhiteSpace(movie.SmallPosterPath) ? "/img/placeholder-small.jpg" : movie.SmallPosterPath,
                TheatreOptions = theatreOptions,
                SelectedTheatreId = selectedTheatreId,
                DateBlocks = ShowtimeBlockVM.BuildFrom(shows, selectedTheatreId)
            };

            return View(vm);
        }

        // For now just redirects to Details; still requires login
        [Authorize]
        public IActionResult Ticket(long id, long? theatreId)
        {
            return RedirectToAction(nameof(Details), new { id, theatreId });
        }

        // ======================
        // Admin-only (optional)
        // ======================

        [Authorize(Roles = Roles.Admin)]
        public IActionResult Create() => View();

        [Authorize(Roles = Roles.Admin)]
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Movie movie)
        {
            if (!ModelState.IsValid) return View(movie);
            _db.Movies.Add(movie);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = Roles.Admin)]
        public async Task<IActionResult> Edit(long? id)
        {
            if (id == null) return NotFound();
            var m = await _db.Movies.FindAsync(id.Value);
            if (m == null) return NotFound();
            return View(m);
        }

        [Authorize(Roles = Roles.Admin)]
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, Movie movie)
        {
            if (id != movie.MovieId) return NotFound();
            if (!ModelState.IsValid) return View(movie);
            _db.Update(movie);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = Roles.Admin)]
        public async Task<IActionResult> Delete(long? id)
        {
            if (id == null) return NotFound();
            var m = await _db.Movies.AsNoTracking().FirstOrDefaultAsync(x => x.MovieId == id.Value);
            if (m == null) return NotFound();
            return View(m);
        }

        [Authorize(Roles = Roles.Admin)]
        [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            var m = await _db.Movies.FindAsync(id);
            if (m != null)
            {
                _db.Movies.Remove(m);
                await _db.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
