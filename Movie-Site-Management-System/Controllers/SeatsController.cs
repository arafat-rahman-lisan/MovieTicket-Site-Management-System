using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Movie_Site_Management_System.Data;
using Movie_Site_Management_System.Data.Identity;
using Movie_Site_Management_System.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Movie_Site_Management_System.Controllers
{
    /// <summary>
    /// Seat management per hall with CRUD and simple JSON endpoints for seat-map testing.
    /// Routes:
    ///   GET   /seats?hallId=1
    ///   GET   /seats/create?hallId=1
    ///   POST  /seats/create
    ///   GET   /seats/edit/{id}
    ///   POST  /seats/edit/{id}
    ///   GET   /seats/details/{id}
    ///   GET   /seats/delete/{id}
    ///   POST  /seats/delete/{id}
    ///   GET   /seats/grid/{hallId}     -> JSON
    ///   POST  /seats/toggle/{id}       -> quick toggle IsDisabled (redirect with alert)
    ///   POST  /seats/bulkgenerate      -> bulk create rows × seats
    /// </summary>
    [Authorize(Roles = Roles.Admin)]
    public class SeatsController : Controller
    {
        private readonly AppDbContext _db;
        public SeatsController(AppDbContext db) => _db = db;

        // ---------- helpers ----------
        private async Task PopulateDropdownsAsync(long? hallId = null, short? seatTypeId = null)
        {
            var halls = await _db.Halls
                .Include(h => h.Theatre)
                .AsNoTracking()
                .OrderBy(h => h.Theatre.Name).ThenBy(h => h.Name)
                .Select(h => new
                {
                    h.HallId,
                    Label = $"{h.Theatre.Name} / {h.Name}"
                })
                .ToListAsync();

            ViewBag.Halls = new SelectList(halls, "HallId", "Label", hallId);

            var seatTypes = await _db.SeatTypes
                .AsNoTracking()
                .OrderBy(s => s.Name)
                .ToListAsync();

            ViewBag.SeatTypes = new SelectList(seatTypes, "SeatTypeId", "Name", seatTypeId);
        }

        private async Task<bool> SeatExistsAsync(long hallId, string row, int num, long? excludeSeatId = null)
        {
            var q = _db.Seats.AsNoTracking().Where(s =>
                s.HallId == hallId &&
                s.RowLabel == row &&
                s.SeatNumber == num
            );

            if (excludeSeatId.HasValue)
                q = q.Where(s => s.SeatId != excludeSeatId.Value);

            return await q.AnyAsync();
        }

        // ---------- Index ----------
        public async Task<IActionResult> Index(long? hallId)
        {
            // Ensure both Halls and SeatTypes are in ViewBags for the bulk form.
            await PopulateDropdownsAsync(hallId);

            if (!hallId.HasValue)
                return View(Enumerable.Empty<Seat>());

            var list = await _db.Seats.AsNoTracking()
                .Where(s => s.HallId == hallId.Value)
                .Include(s => s.SeatType)
                .OrderBy(s => s.RowLabel).ThenBy(s => s.SeatNumber)
                .ToListAsync();

            return View(list);
        }

        // ---------- JSON grid ----------
        [HttpGet("/seats/grid/{hallId:long}")]
        public async Task<IActionResult> Grid(long hallId)
        {
            var data = await _db.Seats.AsNoTracking()
                .Where(s => s.HallId == hallId)
                .Select(s => new
                {
                    s.SeatId,
                    s.RowLabel,
                    s.SeatNumber,
                    s.PosX,
                    s.PosY,
                    SeatType = s.SeatType.Name,
                    s.IsDisabled
                })
                .OrderBy(s => s.RowLabel).ThenBy(s => s.SeatNumber)
                .ToListAsync();

            return Json(data);
        }

        // ---------- Details ----------
        public async Task<IActionResult> Details(long id)
        {
            var seat = await _db.Seats
                .Include(s => s.Hall).ThenInclude(h => h.Theatre)
                .Include(s => s.SeatType)
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.SeatId == id);

            if (seat == null) return NotFound();
            return View(seat);
        }

        // ---------- Create ----------
        [HttpGet]
        public async Task<IActionResult> Create(long? hallId)
        {
            await PopulateDropdownsAsync(hallId);
            return View(new Seat
            {
                HallId = hallId ?? 0,
                IsDisabled = false
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Seat seat)
        {
            if (await SeatExistsAsync(seat.HallId, seat.RowLabel, seat.SeatNumber))
                ModelState.AddModelError(string.Empty, "A seat with the same Row and Number already exists in this hall.");

            if (!ModelState.IsValid)
            {
                await PopulateDropdownsAsync(seat.HallId, seat.SeatTypeId);
                return View(seat);
            }

            _db.Seats.Add(seat);
            await _db.SaveChangesAsync();
            TempData["Success"] = "Seat created.";
            return RedirectToAction(nameof(Index), new { hallId = seat.HallId });
        }

        // ---------- Edit ----------
        [HttpGet]
        public async Task<IActionResult> Edit(long id)
        {
            var seat = await _db.Seats.AsNoTracking().FirstOrDefaultAsync(s => s.SeatId == id);
            if (seat == null) return NotFound();

            await PopulateDropdownsAsync(seat.HallId, seat.SeatTypeId);
            return View(seat);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, Seat seat)
        {
            if (id != seat.SeatId) return BadRequest();

            if (await SeatExistsAsync(seat.HallId, seat.RowLabel, seat.SeatNumber, excludeSeatId: id))
                ModelState.AddModelError(string.Empty, "A seat with the same Row and Number already exists in this hall.");

            if (!ModelState.IsValid)
            {
                await PopulateDropdownsAsync(seat.HallId, seat.SeatTypeId);
                return View(seat);
            }

            try
            {
                _db.Seats.Update(seat);
                await _db.SaveChangesAsync();
                TempData["Success"] = "Seat updated.";
                return RedirectToAction(nameof(Index), new { hallId = seat.HallId });
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _db.Seats.AnyAsync(s => s.SeatId == id))
                    return NotFound();
                throw;
            }
        }

        // ---------- Delete ----------
        [HttpGet]
        public async Task<IActionResult> Delete(long id)
        {
            var seat = await _db.Seats
                .Include(s => s.Hall).ThenInclude(h => h.Theatre)
                .Include(s => s.SeatType)
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.SeatId == id);

            if (seat == null) return NotFound();
            return View(seat);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            var seat = await _db.Seats
                .Include(s => s.ShowSeats)
                .Include(s => s.BookingSeats)
                .Include(s => s.SeatBlocks)
                .FirstOrDefaultAsync(s => s.SeatId == id);

            if (seat == null) return NotFound();

            // prevent deletion if referenced
            if ((seat.ShowSeats?.Any() ?? false) || (seat.BookingSeats?.Any() ?? false) || (seat.SeatBlocks?.Any() ?? false))
            {
                TempData["Error"] = "Cannot delete: this seat has related show/booking/block data.";
                return RedirectToAction(nameof(Details), new { id });
            }

            var hallId = seat.HallId;
            _db.Seats.Remove(seat);
            await _db.SaveChangesAsync();
            TempData["Success"] = "Seat deleted.";
            return RedirectToAction(nameof(Index), new { hallId });
        }

        // ---------- Quick toggle (redirect back with alert) ----------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Toggle(long id)
        {
            var seat = await _db.Seats.FirstOrDefaultAsync(s => s.SeatId == id);
            if (seat == null)
            {
                TempData["Error"] = "Seat not found.";
                return RedirectToAction(nameof(Index));
            }

            seat.IsDisabled = !seat.IsDisabled;
            await _db.SaveChangesAsync();

            TempData["Success"] = seat.IsDisabled
                ? $"Seat {seat.RowLabel}{seat.SeatNumber} disabled."
                : $"Seat {seat.RowLabel}{seat.SeatNumber} enabled.";

            return RedirectToAction(nameof(Index), new { hallId = seat.HallId });
        }

        // ---------- Bulk generator (rows × seats) ----------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BulkGenerate(long hallId, string rowsCsv, int seatsPerRow, short seatTypeId)
        {
            if (hallId <= 0 || string.IsNullOrWhiteSpace(rowsCsv) || seatsPerRow <= 0)
            {
                TempData["Error"] = "Provide hall, rows, and seats per row.";
                return RedirectToAction(nameof(Index), new { hallId });
            }

            var rows = rowsCsv.Split(',')
                              .Select(r => r.Trim())
                              .Where(r => !string.IsNullOrEmpty(r))
                              .Distinct()
                              .ToList();

            var existing = await _db.Seats.AsNoTracking()
                                 .Where(s => s.HallId == hallId)
                                 .ToListAsync();

            var toAdd = new List<Seat>();
            foreach (var r in rows)
            {
                for (int n = 1; n <= seatsPerRow; n++)
                {
                    if (existing.Any(e => e.RowLabel == r && e.SeatNumber == n)) continue;
                    toAdd.Add(new Seat
                    {
                        HallId = hallId,
                        RowLabel = r,
                        SeatNumber = n,
                        SeatTypeId = seatTypeId,
                        IsDisabled = false
                    });
                }
            }

            if (toAdd.Count == 0)
            {
                TempData["Error"] = "No new seats to add (all exist).";
                return RedirectToAction(nameof(Index), new { hallId });
            }

            _db.Seats.AddRange(toAdd);
            await _db.SaveChangesAsync();
            TempData["Success"] = $"Added {toAdd.Count} seats.";
            return RedirectToAction(nameof(Index), new { hallId });
        }
    }
}
