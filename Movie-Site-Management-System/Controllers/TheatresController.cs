using Movie_Site_Management_System.Data;
using Movie_Site_Management_System.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

// 🔐
using Microsoft.AspNetCore.Authorization;
using Movie_Site_Management_System.Data.Identity;

namespace Movie_Site_Management_System.Controllers
{
    // 🔐 Entire controller is Admin-only (except Locations endpoint below)
    [Authorize(Roles = Roles.Admin)]
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
        public IActionResult Create() => View();

        // POST: Theatres/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,Address,City,Lat,Lng,IsActive")] Theatre theatre)
        {
            if (!ModelState.IsValid) return View(theatre);

            theatre.CreatedAt = DateTime.UtcNow;

            _db.Theatres.Add(theatre);
            await _db.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // GET: Theatres/Details/5
        // Shows + Halls (with counts) for the selected date.
        public async Task<IActionResult> Details(long id, DateOnly? date)
        {
            var theatre = await _db.Theatres
                .Include(t => t.Halls) // for Delete screen & quick checks
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.TheatreId == id);

            if (theatre == null) return NotFound();

            // ---- Date handling
            var targetDate = date ?? DateOnly.FromDateTime(DateTime.UtcNow.Date);
            ViewBag.Date = targetDate;

            // ---- Shows for that theatre & date (null-safe)
            var shows = await _db.Shows
                .AsNoTracking()
                .Include(s => s.Movie)
                .Include(s => s.HallSlot!).ThenInclude(hs => hs.Hall!)
                .Where(s =>
                    s.ShowDate == targetDate &&
                    s.HallSlot != null &&
                    s.HallSlot.Hall != null &&
                    s.HallSlot.Hall.TheatreId == id)
                .OrderBy(s => s.HallSlot!.HallId)
                .ThenBy(s => s.HallSlot!.SlotNumber)
                .ToListAsync();

            ViewBag.Shows = shows;

            // ---- Halls table with live counts (Seats, Slots, Shows)
            var halls = await _db.Halls
                .AsNoTracking()
                .Where(h => h.TheatreId == id)
                .Select(h => new
                {
                    h.HallId,
                    h.Name,
                    h.Capacity,
                    h.SeatmapVersion,
                    h.IsActive,
                    SeatCount = _db.Seats.Count(s => s.HallId == h.HallId),
                    SlotCount = _db.HallSlots.Count(sl => sl.HallId == h.HallId),
                    ShowCount = (
                        from sh in _db.Shows
                        join hs in _db.HallSlots on sh.HallSlotId equals hs.HallSlotId
                        where hs.HallId == h.HallId
                        select sh
                    ).Count()
                })
                .OrderBy(h => h.Name)
                .ToListAsync();

            ViewBag.Halls = halls;

            return View(theatre);
        }

        // GET: Theatres/Edit/5
        public async Task<IActionResult> Edit(long id)
        {
            var theatre = await _db.Theatres.FindAsync(id);
            if (theatre == null) return NotFound();

            return View(theatre);
        }

        // POST: Theatres/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, [Bind("TheatreId,Name,Address,City,Lat,Lng,IsActive,CreatedAt")] Theatre theatre)
        {
            if (id != theatre.TheatreId) return NotFound();
            if (!ModelState.IsValid) return View(theatre);

            try
            {
                _db.Update(theatre);
                await _db.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                var exists = await _db.Theatres.AnyAsync(e => e.TheatreId == id);
                if (!exists) return NotFound();
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

            if (theatre == null) return NotFound();

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
                var tAgain = await _db.Theatres.AsNoTracking().FirstOrDefaultAsync(t => t.TheatreId == id);
                if (tAgain == null) return NotFound();

                ViewBag.HallCount = await _db.Halls.CountAsync(h => h.TheatreId == id);
                ModelState.AddModelError(string.Empty, "Cannot delete: this theatre has halls. Delete or reassign halls first.");
                return View("Delete", tAgain);
            }

            var theatre = await _db.Theatres.FindAsync(id);
            if (theatre == null) return NotFound();

            try
            {
                _db.Theatres.Remove(theatre);
                await _db.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException)
            {
                var tAgain = await _db.Theatres.AsNoTracking().FirstOrDefaultAsync(t => t.TheatreId == id);
                ViewBag.HallCount = await _db.Halls.CountAsync(h => h.TheatreId == id);
                ModelState.AddModelError(string.Empty, "Delete failed due to related data. Remove dependents and try again.");
                return View("Delete", tAgain!);
            }
        }

        // GET /theatres/locations
        // 🌐 Keep this public so anonymous users can open the location picker on Movies page.
        [HttpGet]
        [AllowAnonymous]
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
