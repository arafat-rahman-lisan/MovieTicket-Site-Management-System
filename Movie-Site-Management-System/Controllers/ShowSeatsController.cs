using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Movie_Site_Management_System.Data;

namespace Movie_Site_Management_System.Controllers
{
    public class ShowSeatsController : Controller
    {
        private readonly AppDbContext _db;
        public ShowSeatsController(AppDbContext db) => _db = db;

        // GET /showseats/map/{showId}
        public async Task<IActionResult> Map(long showId)
        {
            var show = await _db.Shows
                .AsNoTracking()
                .Include(s => s.HallSlot)
                    .ThenInclude(hs => hs.Hall)
                .Include(s => s.Movie)
                .FirstOrDefaultAsync(s => s.ShowId == showId);

            if (show == null) return NotFound();

            var seatInfos = await _db.ShowSeats
                .AsNoTracking()
                .Where(ss => ss.ShowId == showId)
                .Include(ss => ss.Seat) // RowLabel, SeatNumber, SeatTypeId
                .Select(ss => new
                {
                    ss.Seat.SeatId,
                    ss.Seat.RowLabel,
                    ss.Seat.SeatNumber,
                    ss.Seat.SeatTypeId,
                    ss.Status,
                    ss.PriceAtBooking
                })
                .OrderBy(x => x.RowLabel).ThenBy(x => x.SeatNumber)
                .ToListAsync();

            // You can return a View or JSON depending on front-end
            return View(seatInfos);
            // return Json(seatInfos);
        }
    }
}
