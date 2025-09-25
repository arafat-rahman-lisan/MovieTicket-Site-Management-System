using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Movie_Site_Management_System.Data;
using Movie_Site_Management_System.Data.Enums;
using Movie_Site_Management_System.Data.Identity;
using Movie_Site_Management_System.ViewModels.Shows;
using System.Globalization;

namespace Movie_Site_Management_System.Controllers
{
    [Authorize(Roles = Roles.Admin)] // Map/Proceed opened below with [AllowAnonymous]
    public class ShowSeatsController : Controller
    {
        private readonly AppDbContext _db;
        public ShowSeatsController(AppDbContext db) => _db = db;

        private static DateTime UtcNow() => DateTime.UtcNow;

        private async Task<int> ReleaseExpiredHolds(long showId)
        {
            var now = UtcNow();
            var expired = await _db.ShowSeats
                .Where(ss => ss.ShowId == showId
                          && ss.Status == ShowSeatStatus.Held
                          && ss.HoldUntil != null
                          && ss.HoldUntil < now)
                .ToListAsync();

            foreach (var ss in expired)
            {
                ss.Status = ShowSeatStatus.Available;
                ss.HoldUntil = null;
            }
            if (expired.Count > 0) await _db.SaveChangesAsync();
            return expired.Count;
        }

        private async Task<bool> TryAcquireHold(long showId, IReadOnlyCollection<long> showSeatIds, TimeSpan ttl)
        {
            var now = UtcNow();
            await using var tx = await _db.Database.BeginTransactionAsync();

            var rows = await _db.ShowSeats
                .Where(ss => ss.ShowId == showId && showSeatIds.Contains(ss.ShowSeatId))
                .ToListAsync();

            if (rows.Count != showSeatIds.Count)
            {
                await tx.RollbackAsync();
                return false;
            }

            foreach (var ss in rows)
            {
                var canTake =
                    ss.Status == ShowSeatStatus.Available ||
                    (ss.Status == ShowSeatStatus.Held && ss.HoldUntil != null && ss.HoldUntil < now);

                if (!canTake)
                {
                    await tx.RollbackAsync();
                    return false;
                }
            }

            foreach (var ss in rows)
            {
                ss.Status = ShowSeatStatus.Held;
                ss.HoldUntil = now.Add(ttl);
            }

            try
            {
                await _db.SaveChangesAsync();
                await tx.CommitAsync();
                return true;
            }
            catch
            {
                await tx.RollbackAsync();
                return false;
            }
        }

        private static List<long> ParseIds(string? csv) =>
            (csv ?? "")
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(s => long.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out var id) ? id : 0)
            .Where(id => id > 0)
            .Distinct()
            .ToList();

        // ------------------ MAP (seat grid) ------------------
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Map(long showId)
        {
            await ReleaseExpiredHolds(showId);

            var show = await _db.Shows
                .AsNoTracking()
                .Include(s => s.Movie)
                .Include(s => s.HallSlot)!.ThenInclude(hs => hs!.Hall)!.ThenInclude(h => h!.Theatre)
                .FirstOrDefaultAsync(s => s.ShowId == showId);
            if (show == null) return NotFound();

            var showSeats = await _db.ShowSeats
                .AsNoTracking()
                .Include(ss => ss.Seat)
                .Where(ss => ss.ShowId == showId)
                .ToListAsync();

            var seatCells = showSeats
                .Select(ss => new ShowSeatCellVM
                {
                    SeatId = ss.ShowSeatId, // snapshot id (ShowSeatId)
                    RowLabel = ss.Seat?.RowLabel ?? "?",
                    SeatNumber = ss.Seat?.SeatNumber ?? 0,
                    Price = ss.Price,
                    Status = ss.Status,
                    IsDisabled = ss.Seat?.IsDisabled ?? false
                })
                .OrderBy(x => x.RowLabel).ThenBy(x => x.SeatNumber)
                .ToList();

            var vm = new ShowMapVM
            {
                ShowId = show.ShowId,
                MovieTitle = show.Movie?.Title ?? "Movie",
                TheatreName = show.HallSlot?.Hall?.Theatre?.Name ?? "Theatre",
                HallName = show.HallSlot?.Hall?.Name ?? "Hall",
                Date = show.ShowDate,
                StartTime = show.HallSlot?.StartTime ?? default,
                Seats = seatCells
            };

            return View(vm);
        }

        // ------------------ PROCEED (PRG pattern) ------------------

        // POST: acquire hold, then redirect to GET with seatIds in the query
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Proceed(long showId, string seatIds)
        {
            if (string.IsNullOrWhiteSpace(seatIds))
            {
                TempData["Error"] = "No seats selected.";
                return RedirectToAction(nameof(Map), new { showId });
            }

            var idSet = ParseIds(seatIds);
            if (idSet.Count == 0)
            {
                TempData["Error"] = "No valid seats selected.";
                return RedirectToAction(nameof(Map), new { showId });
            }

            await ReleaseExpiredHolds(showId);

            var acquired = await TryAcquireHold(showId, idSet, TimeSpan.FromMinutes(2));
            if (!acquired)
            {
                TempData["Error"] = "Some seats are no longer available. Please select again.";
                return RedirectToAction(nameof(Map), new { showId });
            }

            // PRG: keep normalized CSV
            var csv = string.Join(",", idSet);
            return RedirectToAction(nameof(Proceed), new { showId, seatIds = csv });
        }

        // GET: display summary; DO NOT acquire or modify holds here
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Proceed(long showId, string? seatIds, bool fromGet = true)
        {
            var ids = ParseIds(seatIds);
            if (showId <= 0 || ids.Count == 0)
                return RedirectToAction(nameof(Map), new { showId });

            var show = await _db.Shows
                .AsNoTracking()
                .Include(s => s.Movie)
                .Include(s => s.HallSlot)!.ThenInclude(hs => hs!.Hall)!.ThenInclude(h => h!.Theatre)
                .FirstOrDefaultAsync(s => s.ShowId == showId);
            if (show == null) return NotFound();

            var selected = await _db.ShowSeats
                .AsNoTracking()
                .Include(ss => ss.Seat)
                .Where(ss => ss.ShowId == showId && ids.Contains(ss.ShowSeatId))
                .OrderBy(s => s.Seat!.RowLabel).ThenBy(s => s.Seat!.SeatNumber)
                .ToListAsync();

            var lines = selected.Select(s => new ShowSeatCellVM
            {
                SeatId = s.ShowSeatId,
                RowLabel = s.Seat!.RowLabel ?? "?",
                SeatNumber = s.Seat!.SeatNumber,
                Price = s.Price,
                Status = s.Status,
                IsDisabled = s.Seat!.IsDisabled
            }).ToList();

            var vm = new ShowProceedVM
            {
                ShowId = showId,
                MovieTitle = show.Movie?.Title ?? string.Empty,
                TheatreName = show.HallSlot?.Hall?.Theatre?.Name ?? string.Empty,
                HallName = show.HallSlot?.Hall?.Name ?? string.Empty,
                Date = show.ShowDate,
                StartTime = show.HallSlot?.StartTime ?? default,
                Seats = lines,
                Total = lines.Sum(s => s.Price)
            };

            // countdown = min remaining HoldUntil for selected seats (fallback 120)
            var now = UtcNow();
            var remaining = selected
                .Where(s => s.HoldUntil != null && s.HoldUntil > now)
                .Select(s => (int)Math.Ceiling((s.HoldUntil!.Value - now).TotalSeconds))
                .DefaultIfEmpty(120)
                .Min();

            ViewBag.HoldSeconds = Math.Max(0, remaining);
            ViewBag.SeatIdsCsv = string.Join(",", ids);

            return View("Proceed", vm);
        }
    }
}
