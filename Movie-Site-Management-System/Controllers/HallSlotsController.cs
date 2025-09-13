using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Movie_Site_Management_System.Data;

namespace Movie_Site_Management_System.Controllers
{
    /// <summary>
    /// Browse slots per hall and inspect one slot with its show for a date.
    /// Routes:
    ///   GET /hallslots?hallId=1
    ///   GET /hallslots/details/{id}?date=YYYY-MM-DD
    /// </summary>
    public class HallSlotsController : Controller
    {
        private readonly AppDbContext _db;
        public HallSlotsController(AppDbContext db) => _db = db;

        // GET /hallslots?hallId=1
        public async Task<IActionResult> Index(long? hallId)
        {
            var q = _db.HallSlots.AsNoTracking()
                .Include(hs => hs.Hall).ThenInclude(h => h.Theatre)
                .AsQueryable();

            if (hallId.HasValue) q = q.Where(hs => hs.HallId == hallId.Value);

            var list = await q
                .OrderBy(hs => hs.Hall.Theatre.Name)
                .ThenBy(hs => hs.Hall.Name)
                .ThenBy(hs => hs.SlotNumber)
                .ToListAsync();

            var halls = await _db.Halls.Include(h => h.Theatre).AsNoTracking()
                .OrderBy(h => h.Theatre.Name).ThenBy(h => h.Name).ToListAsync();

            ViewBag.Halls = new SelectList(
                halls.Select(h => new { h.HallId, Label = $"{h.Theatre.Name} / {h.Name}" }),
                "HallId", "Label", hallId
            );

            return View(list);
        }

        // GET /hallslots/details/7?date=2025-09-12
        // Shows one slot plus (if exists) the unique show for that date.
        public async Task<IActionResult> Details(long id, DateOnly? date)
        {
            var slot = await _db.HallSlots
                .Include(hs => hs.Hall).ThenInclude(h => h.Theatre)
                .AsNoTracking()
                .FirstOrDefaultAsync(hs => hs.HallSlotId == id);

            if (slot == null) return NotFound();

            var targetDate = date ?? DateOnly.FromDateTime(DateTime.UtcNow.Date);

            var show = await _db.Shows.AsNoTracking()
                .Include(s => s.Movie)
                .FirstOrDefaultAsync(s => s.HallSlotId == id && s.ShowDate == targetDate);

            ViewBag.Date = targetDate;
            ViewBag.Show = show; // can be null
            return View(slot);
        }
    }
}
