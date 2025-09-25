using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Movie_Site_Management_System.Data;
using Movie_Site_Management_System.Data.Identity;

namespace Movie_Site_Management_System.Controllers
{
    [Authorize(Roles = Roles.Admin)]
    public class SeatMapController : Controller
    {
        private readonly AppDbContext _db;
        public SeatMapController(AppDbContext db) => _db = db;

        // Step 1: choose a hall to edit
        [HttpGet]
        public async Task<IActionResult> Select()
        {
            var halls = await _db.Halls
                .Include(h => h.Theatre)
                .AsNoTracking()
                .OrderBy(h => h.Theatre.Name).ThenBy(h => h.Name)
                .Select(h => new { h.HallId, Label = $"{h.Theatre.Name} / {h.Name}" })
                .ToListAsync();

            ViewBag.Halls = new SelectList(halls, "HallId", "Label");
            return View();
        }

        // Step 2: seat map editor for selected hall
        [HttpGet]
        public async Task<IActionResult> Index(long hallId)
        {
            var hall = await _db.Halls
                .Include(h => h.Theatre)
                .Include(h => h.Seats).ThenInclude(s => s.SeatType)
                .AsNoTracking()
                .FirstOrDefaultAsync(h => h.HallId == hallId);

            if (hall == null) return NotFound();
            return View(hall);
        }

        // Toggle seat enable/disable (instant save via AJAX) — kept for convenience
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Toggle(long id)
        {
            var seat = await _db.Seats.FirstOrDefaultAsync(s => s.SeatId == id);
            if (seat == null) return NotFound();

            seat.IsDisabled = !seat.IsDisabled;
            await _db.SaveChangesAsync();

            return Ok(new { seat.SeatId, seat.IsDisabled });
        }

        // Batch "Save Layout" — sets IsDisabled for all seats in this hall from the submitted list
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Save(long hallId, string? disabledSeatIds)
        {
            var hall = await _db.Halls
                .Include(h => h.Seats)
                .FirstOrDefaultAsync(h => h.HallId == hallId);
            if (hall == null) return NotFound();

            var ids = new HashSet<long>();
            if (!string.IsNullOrWhiteSpace(disabledSeatIds))
            {
                foreach (var piece in disabledSeatIds.Split(',', StringSplitOptions.RemoveEmptyEntries))
                {
                    if (long.TryParse(piece.Trim(), out var sid))
                        ids.Add(sid);
                }
            }

            foreach (var seat in hall.Seats)
            {
                seat.IsDisabled = ids.Contains(seat.SeatId);
            }

            await _db.SaveChangesAsync();
            TempData["Success"] = "Seat layout saved.";
            return RedirectToAction(nameof(Index), new { hallId });
        }
    }
}
