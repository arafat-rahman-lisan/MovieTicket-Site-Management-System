using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using Movie_Site_Management_System.Data;
using Movie_Site_Management_System.Data.Identity;
using Movie_Site_Management_System.ViewModels.Shows;

namespace Movie_Site_Management_System.Controllers
{
    // Keep the controller admin-only; we’ll open Map to the public so customers can book.
    [Authorize(Roles = Roles.Admin)]
    public class ShowSeatsController : Controller
    {
        private readonly AppDbContext _db;
        public ShowSeatsController(AppDbContext db) => _db = db;

        // GET /showseats/map/{showId}
        [HttpGet]
        [AllowAnonymous] // override controller-level authorize so customers can see the seat map
        public async Task<IActionResult> Map(long showId)
        {
            var show = await _db.Shows
                .AsNoTracking()
                .Include(s => s.Movie)
                .Include(s => s.HallSlot)!.ThenInclude(hs => hs.Hall)!.ThenInclude(h => h.Theatre)
                .FirstOrDefaultAsync(s => s.ShowId == showId);

            if (show == null) return NotFound();

            // Build the flat list your Map.cshtml expects
            var seatCells = await _db.ShowSeats
                .AsNoTracking()
                .Where(ss => ss.ShowId == showId)
                .Include(ss => ss.Seat!)      // for RowLabel / SeatNumber
                .Select(ss => new ShowSeatCellVM
                {
                    // IMPORTANT: post snapshot id for booking pipeline
                    SeatId = ss.ShowSeatId,           // ← snapshot id
                    RowLabel = ss.Seat!.RowLabel ?? "?",
                    SeatNumber = ss.Seat!.SeatNumber,
                    Price = ss.Price,
                    Status = ss.Status
                })
                .OrderBy(x => x.RowLabel)
                .ThenBy(x => x.SeatNumber)
                .ToListAsync();

            var vm = new ShowMapVM
            {
                ShowId = show.ShowId,
                MovieTitle = show.Movie?.Title ?? "Movie",
                TheatreName = show.HallSlot?.Hall?.Theatre?.Name ?? "Theatre",
                HallName = show.HallSlot?.Hall?.Name ?? "Hall",
                Date = show.Date,
                StartTime = show.HallSlot?.StartTime ?? default,
                Seats = seatCells
            };

            // Ensure your view has the fully qualified model:
            // @model Movie_Site_Management_System.ViewModels.Shows.ShowMapVM
            return View(vm);
        }
    }
}
