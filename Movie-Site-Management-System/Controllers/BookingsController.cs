
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Movie_Site_Management_System.Data;
using Movie_Site_Management_System.Data.Enums;
using Movie_Site_Management_System.Models;

namespace Movie_Site_Management_System.Controllers
{
    public class BookingsController : Controller
    {
        private readonly AppDbContext _db;
        public BookingsController(AppDbContext db) => _db = db;

        // GET /bookings/start?showId=123
        public async Task<IActionResult> Start(long showId)
        {
            var show = await _db.Shows
                .Include(s => s.Movie)
                .Include(s => s.HallSlot).ThenInclude(hs => hs.Hall).ThenInclude(h => h.Theatre)
                .FirstOrDefaultAsync(s => s.ShowId == showId);
            if (show == null) return NotFound();

            // Show summary + link to seat map
            return View(show);
        }

        // POST /bookings/confirm
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Confirm(long showId, List<long> seatIds)
        {
            if (seatIds == null || seatIds.Count == 0)
            {
                ModelState.AddModelError(string.Empty, "Please select seats.");
                return RedirectToAction(nameof(Start), new { showId });
            }

            // Make sure all requested seats for this show are available
            var snapshot = await _db.ShowSeats
                .Where(ss => ss.ShowId == showId && seatIds.Contains(ss.SeatId))
                .ToListAsync();

            if (snapshot.Count != seatIds.Count || snapshot.Any(ss => ss.Status != ShowSeatStatus.Available))
            {
                TempData["Error"] = "One or more seats are no longer available.";
                return RedirectToAction(nameof(Start), new { showId });
            }

            // Create booking
            var booking = new Booking
            {
                ShowId = showId,
                CustomerId = null, // attach when you have auth
                Status = BookingStatus.Confirmed, // adjust to your enum/string
                TicketQuantity = seatIds.Count,
                TotalAmount = snapshot.Sum(s => s.PriceAtBooking ?? 0m),
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = null
            };
            _db.Bookings.Add(booking);

            // BookingSeats
            foreach (var ss in snapshot)
            {
                _db.BookingSeats.Add(new BookingSeat
                {
                    Booking = booking,
                    SeatId = ss.SeatId,
                    UnitPrice = ss.PriceAtBooking ?? 0m
                });

                // mark as booked
                ss.Status = ShowSeatStatus.Booked; // or "Booked" if string
            }

            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Details), new { id = booking.BookingId });
        }

        // GET /bookings/details/5
        public async Task<IActionResult> Details(long id)
        {
            var booking = await _db.Bookings
                .Include(b => b.Show).ThenInclude(s => s.Movie)
                .Include(b => b.BookingSeats).ThenInclude(bs => bs.Seat)
                .FirstOrDefaultAsync(b => b.BookingId == id);

            if (booking == null) return NotFound();
            return View(booking);
        }
    }
}
