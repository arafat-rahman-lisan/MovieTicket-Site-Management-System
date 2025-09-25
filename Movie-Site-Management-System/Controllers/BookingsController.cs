using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Movie_Site_Management_System.Data;
using Movie_Site_Management_System.Data.Enums;
using Movie_Site_Management_System.Models;
using System.Security.Claims;

namespace Movie_Site_Management_System.Controllers
{
    [Authorize] // customer must be logged in to start/confirm/view booking
    public class BookingsController : Controller
    {
        private readonly AppDbContext _db;
        public BookingsController(AppDbContext db) => _db = db;

        // GET /bookings/start?showId=123
        [HttpGet]
        public async Task<IActionResult> Start(long showId)
        {
            var show = await _db.Shows
                .AsNoTracking()
                .Include(s => s.Movie)
                .Include(s => s.HallSlot)!.ThenInclude(hs => hs!.Hall)!.ThenInclude(h => h!.Theatre)
                .FirstOrDefaultAsync(s => s.ShowId == showId);

            if (show == null) return NotFound();
            return View(show);
        }

        // POST /bookings/confirm
        // Accepts seat IDs as either CSV or List<long>; verifies holds, creates booking, redirects to /Payments/Invoice.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Confirm(long showId, string? seatIds, string? seatIdsCsv, [FromForm] List<long>? seatIdsList)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId)) return Challenge();

            // Normalize incoming IDs (prefer list; else CSV)
            IEnumerable<long> idsSource = (seatIdsList ?? new List<long>())
                .Where(x => x > 0);

            if (!idsSource.Any())
            {
                var csv = !string.IsNullOrWhiteSpace(seatIds) ? seatIds : seatIdsCsv;
                idsSource = (csv ?? "")
                    .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                    .Select(s => long.TryParse(s, out var id) ? id : 0)
                    .Where(id => id > 0);
            }

            var seatIdsNorm = idsSource.Distinct().ToList();

            if (seatIdsNorm.Count == 0)
            {
                TempData["Error"] = "Please select seats.";
                return RedirectToAction(nameof(Start), new { showId });
            }

            await using var tx = await _db.Database.BeginTransactionAsync();
            var now = DateTime.UtcNow;

            // 1) Defense-in-depth: free any expired holds for this show
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
            if (expired.Count > 0)
                await _db.SaveChangesAsync();

            // 2) Fetch selected seats and ensure HELD and not expired
            var snapshot = await _db.ShowSeats
                .Where(ss => ss.ShowId == showId && seatIdsNorm.Contains(ss.ShowSeatId))
                .Include(ss => ss.Seat)
                .ToListAsync();

            bool allFound = snapshot.Count == seatIdsNorm.Count;
            bool allHeldAndValid = allFound && snapshot.All(ss =>
                ss.Status == ShowSeatStatus.Held &&
                ss.HoldUntil != null &&
                ss.HoldUntil > now);

            if (!allHeldAndValid)
            {
                await tx.RollbackAsync();
                TempData["Error"] = "Your hold expired or seats changed. Please re-select.";
                return RedirectToAction("Map", "ShowSeats", new { showId });
            }

            // 3) Create booking in CREATED
            var booking = new Booking
            {
                ShowId = showId,
                UserId = userId,
                Status = BookingStatus.CREATED,
                TicketQuantity = snapshot.Count,
                TotalAmount = snapshot.Sum(s => s.Price),
                CreatedAt = now,
                ExpiresAt = null
            };
            _db.Bookings.Add(booking);

            // 4) Link seats (we keep ShowSeat rows as Held until payment)
            foreach (var ss in snapshot)
            {
                _db.BookingSeats.Add(new BookingSeat
                {
                    Booking = booking,
                    SeatId = ss.SeatId,
                    ShowSeatId = ss.ShowSeatId,
                    UnitPrice = ss.Price
                });
            }

            try
            {
                await _db.SaveChangesAsync();
                await tx.CommitAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                await tx.RollbackAsync();
                TempData["Error"] = "Another customer just selected one of those seats. Please try different seats.";
                return RedirectToAction(nameof(Start), new { showId });
            }
            catch
            {
                await tx.RollbackAsync();
                throw;
            }

            // 5) Go to Payments
            return RedirectToAction("Invoice", "Payments", new { bookingId = booking.BookingId });
        }

        // GET /bookings/details/{id}
        [HttpGet]
        public async Task<IActionResult> Details(long id)
        {
            var booking = await _db.Bookings
                .AsNoTracking()
                .Include(b => b.Show)!.ThenInclude(s => s.Movie)
                .Include(b => b.BookingSeats)!.ThenInclude(bs => bs.Seat)
                .FirstOrDefaultAsync(b => b.BookingId == id);

            if (booking == null) return NotFound();
            return View(booking);
        }
    }
}
