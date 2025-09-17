using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Movie_Site_Management_System.Data;
using Movie_Site_Management_System.Data.Enums;
using Movie_Site_Management_System.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

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
                // Use null-forgiving on navs to silence CS8602 in Include chains
                .Include(s => s.HallSlot!)
                    .ThenInclude(hs => hs.Hall!)
                        .ThenInclude(h => h.Theatre)
                .FirstOrDefaultAsync(s => s.ShowId == showId);

            if (show == null) return NotFound();
            return View(show);
        }

        // POST /bookings/confirm
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Confirm(long showId, List<long> seatIds)
        {
            if (seatIds == null || seatIds.Count == 0)
            {
                TempData["Error"] = "Please select seats.";
                return RedirectToAction(nameof(Start), new { showId });
            }

            seatIds = seatIds.Distinct().ToList();

            await using var tx = await _db.Database.BeginTransactionAsync();

            var snapshot = await _db.ShowSeats
                .Where(ss => ss.ShowId == showId && seatIds.Contains(ss.ShowSeatId))
                .ToListAsync();

            if (snapshot.Count != seatIds.Count || snapshot.Any(ss => ss.Status != ShowSeatStatus.Available))
            {
                await tx.RollbackAsync();
                TempData["Error"] = "One or more selected seats are no longer available.";
                return RedirectToAction(nameof(Start), new { showId });
            }

            long? customerId = null;
            var uid = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!string.IsNullOrWhiteSpace(uid) && long.TryParse(uid, out var parsed))
                customerId = parsed;
            // If you keep Identity string keys, consider changing Booking.CustomerId to string later.

            var booking = new Booking
            {
                ShowId = showId,
                CustomerId = customerId,
                Status = BookingStatus.Confirmed, // or Pending if adding payment step
                TicketQuantity = seatIds.Count,
                TotalAmount = snapshot.Sum(s => s.Price),
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = null
            };
            _db.Bookings.Add(booking);

            foreach (var ss in snapshot)
            {
                _db.BookingSeats.Add(new BookingSeat
                {
                    Booking = booking,
                    SeatId = ss.SeatId,  // physical seat id from snapshot
                    UnitPrice = ss.Price
                });

                ss.Status = ShowSeatStatus.Booked; // RowVersion on ShowSeat protects concurrency if configured
            }

            try
            {
                await _db.SaveChangesAsync();
                await tx.CommitAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                await tx.RollbackAsync();
                TempData["Error"] = "Another customer just booked one of those seats. Please choose different seats.";
                return RedirectToAction(nameof(Start), new { showId });
            }

            return RedirectToAction(nameof(Details), new { id = booking.BookingId });
        }

        // GET /bookings/details/5
        [HttpGet]
        public async Task<IActionResult> Details(long id)
        {
            var booking = await _db.Bookings
                .AsNoTracking()
                .Include(b => b.Show).ThenInclude(s => s.Movie)
                .Include(b => b.BookingSeats).ThenInclude(bs => bs.Seat) // Seat may be null; view should handle gracefully
                .FirstOrDefaultAsync(b => b.BookingId == id);

            if (booking == null) return NotFound();
            return View(booking);
        }
    }
}
