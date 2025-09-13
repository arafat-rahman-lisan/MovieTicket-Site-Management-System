using Movie_Site_Management_System.Data;
using Movie_Site_Management_System.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Movie_Site_Management_System.Controllers
{
    public class TheatresController : Controller
    {
        private readonly AppDbContext _db;
        public TheatresController(AppDbContext db) => _db = db;

        // GET /theatres
        public async Task<IActionResult> Index()
        {
            var theatres = await _db.Theatres
                .AsNoTracking()
                .OrderBy(t => t.Name)
                .ToListAsync();

            return View(theatres);
        }

        // GET: Theatres/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Theatres/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,Address,City,Lat,Lng,IsActive")] Theatre theatre)
        {
            if (!ModelState.IsValid)
            {
                return View(theatre);
            }

            theatre.CreatedAt = DateTime.UtcNow;

            _db.Theatres.Add(theatre);
            await _db.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // GET: Theatres/Details/5
        public async Task<IActionResult> Details(long id, DateOnly? date)
        {
            var theatre = await _db.Theatres
                .Include(t => t.Halls)
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.TheatreId == id);

            if (theatre == null)
                return NotFound();

            var targetDate = date ?? DateOnly.FromDateTime(DateTime.UtcNow.Date);

            var shows = await _db.Shows.AsNoTracking()
                .Include(s => s.Movie)
                .Include(s => s.HallSlot).ThenInclude(hs => hs.Hall)
                .Where(s => s.HallSlot.Hall.TheatreId == id && s.ShowDate == targetDate)
                .OrderBy(s => s.HallSlot.HallId)
                .ThenBy(s => s.HallSlot.SlotNumber)
                .ToListAsync();

            ViewBag.Date = targetDate;
            ViewBag.Shows = shows;

            return View(theatre);
        }

        // GET: Theatres/Edit/5
        public async Task<IActionResult> Edit(long id)
        {
            var theatre = await _db.Theatres.FindAsync(id);
            if (theatre == null)
                return NotFound();

            return View(theatre);
        }

        // POST: Theatres/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, [Bind("TheatreId,Name,Address,City,Lat,Lng,IsActive,CreatedAt")] Theatre theatre)
        {
            if (id != theatre.TheatreId)
                return NotFound();

            if (!ModelState.IsValid)
                return View(theatre);

            try
            {
                _db.Update(theatre);
                await _db.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _db.Theatres.AnyAsync(e => e.TheatreId == id))
                    return NotFound();
                else
                    throw;
            }

            return RedirectToAction(nameof(Index));
        }


        // GET: Theatres/Delete/5
        public async Task<IActionResult> Delete(long id)
        {
            var theatre = await _db.Theatres
                .Include(t => t.Halls)
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.TheatreId == id);

            if (theatre == null)
                return NotFound();

            ViewBag.HallCount = theatre.Halls?.Count ?? 0;
            return View(theatre);
        }

        // POST: Theatres/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            // Business rule: don’t allow delete if any Halls exist for this Theatre.
            var hasHalls = await _db.Halls.AnyAsync(h => h.TheatreId == id);
            if (hasHalls)
            {
                // Reload entity to show on the page again
                var tAgain = await _db.Theatres.AsNoTracking().FirstOrDefaultAsync(t => t.TheatreId == id);
                if (tAgain == null) return NotFound();

                ViewBag.HallCount = await _db.Halls.CountAsync(h => h.TheatreId == id);
                ModelState.AddModelError(string.Empty, "Cannot delete: this theatre has halls. Delete or reassign halls first.");
                return View("Delete", tAgain);
            }

            var theatre = await _db.Theatres.FindAsync(id);
            if (theatre == null)
                return NotFound();

            try
            {
                _db.Theatres.Remove(theatre);
                await _db.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException)
            {
                // If there are FK constraints we didn’t account for
                var tAgain = await _db.Theatres.AsNoTracking().FirstOrDefaultAsync(t => t.TheatreId == id);
                ViewBag.HallCount = await _db.Halls.CountAsync(h => h.TheatreId == id);
                ModelState.AddModelError(string.Empty, "Delete failed due to related data. Remove dependents and try again.");
                return View("Delete", tAgain!);
            }
        }


        // GET /theatres/locations
        [HttpGet]
        public async Task<IActionResult> Locations()
        {
            var items = await _db.Theatres
                .AsNoTracking()
                .OrderBy(t => t.Name)
                .Select(t => new
                {
                    id = t.TheatreId,
                    name = t.Name,
                    address = t.Address ?? ""
                })
                .ToListAsync();

            return Json(items);
        }
    }
}
