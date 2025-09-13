
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Movie_Site_Management_System.Data;
using Movie_Site_Management_System.Data.Enums;
using Movie_Site_Management_System.Models;

namespace Movie_Site_Management_System.Controllers
{
    public class ShowsController : Controller
    {
        private readonly AppDbContext _db;
        public ShowsController(AppDbContext db) => _db = db;

        // GET /shows?theatreId=1&date=2025-09-12
        public async Task<IActionResult> Index(long? theatreId, DateOnly? date)
        {
            var targetDate = date ?? DateOnly.FromDateTime(DateTime.UtcNow.Date);

            var q = _db.Shows
                .AsNoTracking()
                .Include(s => s.Movie)
                .Include(s => s.HallSlot)
                    .ThenInclude(hs => hs.Hall)
                        .ThenInclude(h => h.Theatre)
                .Where(s => s.ShowDate == targetDate);

            if (theatreId.HasValue)
                q = q.Where(s => s.HallSlot.Hall.TheatreId == theatreId.Value);

            var list = await q
                .OrderBy(s => s.HallSlot.Hall.TheatreId)
                .ThenBy(s => s.HallSlot.HallId)
                .ThenBy(s => s.HallSlot.SlotNumber)
                .ToListAsync();

            ViewBag.Theatres = new SelectList(
                await _db.Theatres.AsNoTracking().OrderBy(t => t.Name).ToListAsync(),
                "TheatreId", "Name", theatreId
            );
            ViewBag.Date = targetDate;

            return View(list);
        }

        // GET /shows/create
        // [Authorize(Roles="Admin")]
        public async Task<IActionResult> Create()
        {
            await BuildDropDowns();
            return View(new Show { ShowDate = DateOnly.FromDateTime(DateTime.UtcNow.Date) });
        }

        // POST /shows/create
        // [Authorize(Roles="Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("MovieId,HallSlotId,ShowDate,Language,IsActive")] Show show)
        {
            // Enforce unique (HallSlotId, ShowDate)
            var exists = await _db.Shows.AnyAsync(s => s.HallSlotId == show.HallSlotId && s.ShowDate == show.ShowDate);
            if (exists)
                ModelState.AddModelError(string.Empty, "A show already exists for this Hall Slot on that date.");

            if (!ModelState.IsValid)
            {
                await BuildDropDowns();
                return View(show);
            }

            show.CreatedAt = DateTime.UtcNow;
            show.IsCancelled = false;
            _db.Shows.Add(show);
            await _db.SaveChangesAsync();

            return RedirectToAction(nameof(Index), new { date = show.ShowDate });
        }

        private async Task BuildDropDowns()
        {
            ViewBag.Movies = new SelectList(
                await _db.Movies.AsNoTracking()
                    .Where(m => m.Status == MovieStatus.NowShowing || m.Status == MovieStatus.Upcoming)
                    .OrderByDescending(m => m.ImdbRating)
                    .ToListAsync(),
                "MovieId", "Title"
            );

            // Show HallSlots labeled as “Theatre / Hall / SlotNumber (Start-End)”
            var hallSlots = await _db.HallSlots
                .Include(hs => hs.Hall).ThenInclude(h => h.Theatre)
                .AsNoTracking()
                .OrderBy(hs => hs.Hall.Theatre.Name).ThenBy(hs => hs.Hall.Name).ThenBy(hs => hs.SlotNumber)
                .ToListAsync();

            var slotOptions = hallSlots.Select(hs => new
            {
                hs.HallSlotId,
                Label = $"{hs.Hall.Theatre.Name} / {hs.Hall.Name} / S{hs.SlotNumber} ({hs.StartTime:hh\\:mm}-{hs.EndTime:hh\\:mm})"
            }).ToList();

            ViewBag.HallSlots = new SelectList(slotOptions, "HallSlotId", "Label");
        }
    }
}
