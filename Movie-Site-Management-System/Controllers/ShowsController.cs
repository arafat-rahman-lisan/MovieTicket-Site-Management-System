using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Movie_Site_Management_System.Data;
using Movie_Site_Management_System.Data.Enums;
using Movie_Site_Management_System.Models;
using Movie_Site_Management_System.Data.Identity;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Movie_Site_Management_System.Controllers
{
    [Authorize(Roles = Roles.Admin)]
    public class ShowsController : Controller
    {
        private readonly AppDbContext _db;
        public ShowsController(AppDbContext db) => _db = db;

        // GET /shows?theatreId=1&date=YYYY-MM-DD
        public async Task<IActionResult> Index(long? theatreId, DateOnly? date)
        {
            var targetDate = date ?? DateOnly.FromDateTime(DateTime.UtcNow.Date);

            var q = _db.Shows
                .AsNoTracking()
#pragma warning disable CS8602
                .Include(s => s.Movie)
                .Include(s => s.HallSlot).ThenInclude(hs => hs.Hall).ThenInclude(h => h.Theatre)
#pragma warning restore CS8602
                .Where(s => s.ShowDate == targetDate);

            if (theatreId.HasValue)
            {
                q = q.Where(s =>
                    s.HallSlot != null &&
                    s.HallSlot.Hall != null &&
                    s.HallSlot.Hall.TheatreId == theatreId.Value);
            }

            var list = await q
                .OrderBy(s => s.HallSlot != null && s.HallSlot.Hall != null ? s.HallSlot.Hall.TheatreId : 0L)
                .ThenBy(s => s.HallSlot != null ? s.HallSlot.HallId : 0L)
                .ThenBy(s => s.HallSlot != null ? s.HallSlot.SlotNumber : 0)
                .ToListAsync();

            ViewBag.Theatres = new SelectList(
                await _db.Theatres.AsNoTracking().OrderBy(t => t.Name).ToListAsync(),
                "TheatreId", "Name", theatreId
            );
            ViewBag.Date = targetDate;

            return View(list);
        }

        // GET /shows/create
        public async Task<IActionResult> Create(long? hallSlotId, DateOnly? date)
        {
            await BuildDropDowns(selectedMovieId: null, selectedHallSlotId: hallSlotId);

            var model = new Show
            {
                HallSlotId = hallSlotId ?? 0,
                ShowDate = date ?? DateOnly.FromDateTime(DateTime.UtcNow.Date),
                IsActive = true
            };
            return View(model);
        }

        // POST /shows/create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("MovieId,HallSlotId,ShowDate,Language,IsActive")] Show show)
        {
            // UNIQUE: (HallSlotId, ShowDate)
            var exists = await _db.Shows.AnyAsync(s => s.HallSlotId == show.HallSlotId && s.ShowDate == show.ShowDate);
            if (exists)
                ModelState.AddModelError(string.Empty, "A show already exists for this Hall Slot on that date.");

            if (!ModelState.IsValid)
            {
                await BuildDropDowns(show.MovieId, show.HallSlotId);
                return View(show);
            }

            show.CreatedAt = DateTime.UtcNow;
            show.IsCancelled = false;

            await using var tx = await _db.Database.BeginTransactionAsync();
            try
            {
                _db.Shows.Add(show);
                await _db.SaveChangesAsync(); // show.ShowId available

                // hallId for this slot
                var hallId = await _db.HallSlots
                    .Where(hs => hs.HallSlotId == show.HallSlotId)
                    .Select(hs => hs.HallId)
                    .FirstAsync();

                // enabled seats in hall + their types
                var seatPairs = await _db.Seats
                    .AsNoTracking()
                    .Where(seat => seat.HallId == hallId && seat.IsDisabled == false)
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

                await tx.CommitAsync();
            }
            catch
            {
                await tx.RollbackAsync();
                ModelState.AddModelError(string.Empty, "Failed to create show. Please try again.");
                await BuildDropDowns(show.MovieId, show.HallSlotId);
                return View(show);
            }

            return RedirectToAction(nameof(Index), new { date = show.ShowDate });
        }

        // GET /shows/details/5
        public async Task<IActionResult> Details(long id)
        {
#pragma warning disable CS8602
            var show = await _db.Shows
                .Include(s => s.Movie)
                .Include(s => s.HallSlot).ThenInclude(hs => hs.Hall).ThenInclude(h => h.Theatre)
                .Include(s => s.ShowSeats).ThenInclude(ss => ss.Seat).ThenInclude(se => se.SeatType)
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.ShowId == id);
#pragma warning restore CS8602
            if (show == null) return NotFound();
            return View(show);
        }

        // GET /shows/edit/5
        public async Task<IActionResult> Edit(long id)
        {
#pragma warning disable CS8602
            var show = await _db.Shows
                .Include(s => s.HallSlot).ThenInclude(hs => hs.Hall).ThenInclude(h => h.Theatre)
                .Include(s => s.Movie)
                .FirstOrDefaultAsync(s => s.ShowId == id);
#pragma warning restore CS8602

            if (show == null) return NotFound();

            await BuildDropDowns(show.MovieId, show.HallSlotId);
            return View(show);
        }

        // POST /shows/edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, [Bind("ShowId,MovieId,HallSlotId,ShowDate,Language,IsActive,IsCancelled,CreatedAt")] Show show)
        {
            if (id != show.ShowId) return NotFound();

            var exists = await _db.Shows.AnyAsync(s =>
                s.ShowId != show.ShowId &&
                s.HallSlotId == show.HallSlotId &&
                s.ShowDate == show.ShowDate);

            if (exists)
                ModelState.AddModelError(string.Empty, "A show already exists for this Hall Slot on that date.");

            if (!ModelState.IsValid)
            {
                await BuildDropDowns(show.MovieId, show.HallSlotId);
                return View(show);
            }

            try
            {
                _db.Entry(show).State = EntityState.Modified;
                await _db.SaveChangesAsync();
                return RedirectToAction(nameof(Details), new { id = show.ShowId });
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _db.Shows.AnyAsync(s => s.ShowId == show.ShowId)) return NotFound();
                throw;
            }
        }

        // GET /shows/delete/5
        public async Task<IActionResult> Delete(long id)
        {
#pragma warning disable CS8602
            var show = await _db.Shows
                .Include(s => s.Movie)
                .Include(s => s.HallSlot).ThenInclude(hs => hs.Hall).ThenInclude(h => h.Theatre)
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.ShowId == id);
#pragma warning restore CS8602
            if (show == null) return NotFound();
            return View(show);
        }

        // POST /shows/delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            // Load show and dependent aggregates
            var show = await _db.Shows
                .Include(s => s.Bookings)
                    .ThenInclude(b => b.Payments)
                .Include(s => s.Bookings)
                    .ThenInclude(b => b.BookingSeats)
                .FirstOrDefaultAsync(s => s.ShowId == id);

            if (show == null) return NotFound();

            // Block only if there are paid/success bookings
            var hasPaid = show.Bookings != null && show.Bookings.Any(b =>
                b.Payments.Any(p => p.Status == PaymentStatus.Success || p.Status == PaymentStatus.Paid));

            if (hasPaid)
            {
                TempData["Error"] = "Cannot delete this show because there are paid bookings.";
                return RedirectToAction(nameof(Details), new { id });
            }

            await using var tx = await _db.Database.BeginTransactionAsync();

            try
            {
                // Remove unpaid/abandoned/cancelled bookings + children
                if (show.Bookings != null && show.Bookings.Any())
                {
                    var bookingIds = show.Bookings.Select(b => b.BookingId).ToList();

                    var bookingSeats = _db.BookingSeats.Where(bs => bookingIds.Contains(bs.BookingId));
                    var payments = _db.Payments.Where(p => bookingIds.Contains(p.BookingId));

                    _db.BookingSeats.RemoveRange(bookingSeats);
                    _db.Payments.RemoveRange(payments);
                    _db.Bookings.RemoveRange(show.Bookings);
                }

                // Remove ShowSeats snapshot
                var showSeats = _db.ShowSeats.Where(ss => ss.ShowId == id);
                _db.ShowSeats.RemoveRange(showSeats);

                // Remove the Show
                _db.Shows.Remove(show);

                await _db.SaveChangesAsync();
                await tx.CommitAsync();

                TempData["Success"] = "Show deleted.";
                return RedirectToAction(nameof(Index), new { date = show.ShowDate });
            }
            catch
            {
                await tx.RollbackAsync();
                TempData["Error"] = "Delete failed. Please try again.";
                return RedirectToAction(nameof(Details), new { id });
            }
        }

        private async Task BuildDropDowns(long? selectedMovieId = null, long? selectedHallSlotId = null)
        {
            // ComingSoon + NowShowing
            ViewBag.Movies = new SelectList(
                await _db.Movies.AsNoTracking()
                    .Where(m => m.Status == MovieStatus.NowShowing || m.Status == MovieStatus.ComingSoon)
                    .OrderByDescending(m => m.ImdbRating)
                    .Select(m => new { m.MovieId, m.Title })
                    .ToListAsync(),
                "MovieId", "Title", selectedMovieId
            );

            var hallSlots = await _db.HallSlots
                .Include(hs => hs.Hall).ThenInclude(h => h.Theatre)
                .AsNoTracking()
                .OrderBy(hs => hs.Hall != null && hs.Hall.Theatre != null ? hs.Hall.Theatre.Name : "")
                .ThenBy(hs => hs.Hall != null ? hs.Hall.Name : "")
                .ThenBy(hs => hs.SlotNumber)
                .ToListAsync();

            var slotOptions = hallSlots.Select(hs => new
            {
                hs.HallSlotId,
                Label = $"{(hs.Hall?.Theatre?.Name ?? "Theatre?")} / {(hs.Hall?.Name ?? "Hall?")} / S{hs.SlotNumber} ({hs.StartTime:hh\\:mm}-{hs.EndTime:hh\\:mm})"
            }).ToList();

            ViewBag.HallSlots = new SelectList(slotOptions, "HallSlotId", "Label", selectedHallSlotId);
        }
    }
}
