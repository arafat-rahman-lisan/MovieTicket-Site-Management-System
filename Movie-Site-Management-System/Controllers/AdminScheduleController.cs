using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Movie_Site_Management_System.Data;
using Movie_Site_Management_System.Data.Enums;
using Movie_Site_Management_System.Data.Identity;
using Movie_Site_Management_System.Models;
using Movie_Site_Management_System.ViewModels.Schedule;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Movie_Site_Management_System.Controllers
{
    [Authorize(Roles = Roles.Admin)]
    public class AdminScheduleController : Controller
    {
        private readonly AppDbContext _db;
        public AdminScheduleController(AppDbContext db) => _db = db;

        // GET: /AdminSchedule
        [HttpGet]
        public async Task<IActionResult> Index(DateOnly? date = null)
        {
            var movies = await _db.Movies
                .AsNoTracking()
                .OrderByDescending(m => m.Status)
                .ThenByDescending(m => m.ImdbRating ?? 0m) // nullable-safe
                .ThenBy(m => m.Title)
                .Select(m => new { m.MovieId, m.Title })
                .ToListAsync();

            ViewBag.MovieOptions = movies;

            return View(new ScheduleWizardVM
            {
                Date = date ?? DateOnly.FromDateTime(DateTime.UtcNow.Date),
                // Language left empty intentionally — admin must type it
            });
        }

        // GET: /AdminSchedule/Data?date=yyyy-MM-dd
        // Returns Theatres → Halls → HallSlots for the day, and marks occupied slots.
        [HttpGet]
        public async Task<IActionResult> Data(string date)
        {
            if (!DateOnly.TryParse(date, out var d))
                return BadRequest("Invalid date.");

            var theatreTree = await _db.Theatres
                .Include(t => t.Halls)
                    .ThenInclude(h => h.HallSlots)
                .AsNoTracking()
                .Select(t => new ScheduleTheatreDTO
                {
                    TheatreId = t.TheatreId,
                    TheatreName = t.Name,
                    Halls = t.Halls
                        .OrderBy(h => h.Name)
                        .Select(h => new ScheduleHallDTO
                        {
                            HallId = h.HallId,
                            HallName = h.Name,
                            HallSlots = h.HallSlots
                                .OrderBy(hs => hs.StartTime)
                                .Select(hs => new ScheduleHallSlotDTO
                                {
                                    HallSlotId = hs.HallSlotId,
                                    Start = hs.StartTime.ToString(@"hh\:mm"),
                                    End = hs.EndTime.ToString(@"hh\:mm"),
                                    IsOccupied = false
                                })
                                .ToList()
                        })
                        .ToList()
                })
                .ToListAsync();

            var occupied = await _db.Shows
                .AsNoTracking()
                .Where(s => s.ShowDate == d)
                .Select(s => s.HallSlotId)
                .Distinct()
                .ToListAsync();

            var occ = occupied.ToHashSet();
            foreach (var th in theatreTree)
                foreach (var h in th.Halls)
                    foreach (var slot in h.HallSlots)
                        slot.IsOccupied = occ.Contains(slot.HallSlotId);

            return Json(new ScheduleDataDTO
            {
                Theatres = theatreTree,
                OccupiedHallSlotIds = occupied,
                Date = d.ToString("yyyy-MM-dd")
            });
        }

        // POST: /AdminSchedule/Create
        // Bulk-create shows for (Date, selections of MovieId×HallSlotId) and snapshot ShowSeats.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ScheduleWizardVM vm)
        {
            // Basic model checks
            if (!ModelState.IsValid || vm.MovieIds == null || vm.MovieIds.Count == 0)
                return BadRequest("Please select a date and at least one movie.");

            if (vm.Selections == null || vm.Selections.Count == 0)
                return BadRequest("No selections were provided.");

            // Language is REQUIRED (no default)
            var language = vm.Language?.Trim();
            if (string.IsNullOrWhiteSpace(language))
            {
                TempData["Error"] = "Please enter a Language before creating the schedule.";
                return RedirectToAction(nameof(Index), new { date = vm.Date.ToString("yyyy-MM-dd") });
            }

            var date = vm.Date;

            // Server-side guard: prevent assigning the SAME HallSlot to more than one movie
            var dupes = vm.Selections
                .GroupBy(s => s.HallSlotId)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key)
                .ToList();
            if (dupes.Count > 0)
                return BadRequest("The same hall slot is selected for multiple movies. Each hall slot can have only one movie.");

            // Also avoid clashes with already-existing Shows on that date
            var selectedHallSlotIds = vm.Selections.Select(s => s.HallSlotId).Distinct().ToList();
            var alreadyOccupied = await _db.Shows
                .AsNoTracking()
                .Where(s => s.ShowDate == date && selectedHallSlotIds.Contains(s.HallSlotId))
                .Select(s => new { s.HallSlotId })
                .Distinct()
                .ToListAsync();

            var occupiedSet = alreadyOccupied.Select(x => x.HallSlotId).ToHashSet();

            using var tx = await _db.Database.BeginTransactionAsync();
            int createdCount = 0;

            foreach (var sel in vm.Selections)
            {
                // sanity: selection must belong to chosen movies list
                if (!vm.MovieIds.Contains(sel.MovieId))
                    continue;

                // skip if occupied
                if (occupiedSet.Contains(sel.HallSlotId))
                    continue;

                var hallSlot = await _db.HallSlots
                    .Include(hs => hs.Hall)
                        .ThenInclude(h => h.Seats)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(hs => hs.HallSlotId == sel.HallSlotId);

                if (hallSlot == null)
                    continue;

                var show = new Show
                {
                    MovieId = sel.MovieId,
                    HallSlotId = sel.HallSlotId,
                    ShowDate = date,
                    Language = language!, // ← REQUIRED, no default; will match DB NOT NULL
                    IsActive = true
                };

                _db.Shows.Add(show);
                await _db.SaveChangesAsync();

                // Snapshot ShowSeats (matching your ShowsController logic)
                var seatPairs = await _db.Seats
                    .AsNoTracking()
                    .Where(seat => seat.HallId == hallSlot.HallId)
                    .Select(seat => new { seat.SeatId, seat.SeatTypeId })
                    .ToListAsync();

                var seatTypeIds = seatPairs.Select(p => p.SeatTypeId).Distinct().ToList();
                var typePrices = await _db.SeatTypes
                    .AsNoTracking()
                    .Where(st => seatTypeIds.Contains(st.SeatTypeId))
                    .ToDictionaryAsync(st => st.SeatTypeId, st => st.BasePrice);

                var showSeats = seatPairs.Select(p => new ShowSeat
                {
                    ShowId = show.ShowId,
                    SeatId = p.SeatId,
                    SeatTypeId = p.SeatTypeId,
                    Price = typePrices.TryGetValue(p.SeatTypeId, out var price) ? price : 0m,
                    Status = ShowSeatStatus.Available
                }).ToList();

                if (showSeats.Count > 0)
                {
                    _db.ShowSeats.AddRange(showSeats);
                    await _db.SaveChangesAsync();
                }

                createdCount++;
            }

            await tx.CommitAsync();

            TempData["Success"] = $"{createdCount} show(s) created for {date:yyyy-MM-dd}.";
            return RedirectToAction(nameof(Index), new { date = date.ToString("yyyy-MM-dd") });
        }
    }
}
