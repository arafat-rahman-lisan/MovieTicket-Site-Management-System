using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Movie_Site_Management_System.Data;

namespace Movie_Site_Management_System.Controllers
{
    /// <summary>
    /// Browse halls (optionally filter by theatre) and view a single hall with seats & slots.
    /// Routes:
    ///   GET /halls?theatreId=1
    ///   GET /halls/details/{id}
    /// </summary>
    public class HallsController : Controller
    {
        private readonly AppDbContext _db;
        public HallsController(AppDbContext db) => _db = db;

        // GET /halls?theatreId=1
        // Simple list for testing; dropdown preloaded via ViewBag.Theatres.
        public async Task<IActionResult> Index(long? theatreId)
        {
            var q = _db.Halls
                .AsNoTracking()
                .Include(h => h.Theatre)
                .AsQueryable();

            if (theatreId.HasValue) q = q.Where(h => h.TheatreId == theatreId.Value);

            var list = await q
                .OrderBy(h => h.Theatre.Name).ThenBy(h => h.Name)
                .ToListAsync();

            ViewBag.Theatres = new SelectList(
                await _db.Theatres.AsNoTracking().OrderBy(t => t.Name).ToListAsync(),
                "TheatreId", "Name", theatreId
            );

            return View(list);
        }

        // GET /halls/details/5
        // Returns the hall, its seats and its slots (no editing yet).
        public async Task<IActionResult> Details(long id)
        {
            var hall = await _db.Halls
                .Include(h => h.Theatre)
                .AsNoTracking()
                .FirstOrDefaultAsync(h => h.HallId == id);
            if (hall == null) return NotFound();

            var seats = await _db.Seats.AsNoTracking()
                .Where(s => s.HallId == id)
                .Include(s => s.SeatType)
                .OrderBy(s => s.RowLabel).ThenBy(s => s.SeatNumber)
                .ToListAsync();

            var slots = await _db.HallSlots.AsNoTracking()
                .Where(hs => hs.HallId == id)
                .OrderBy(hs => hs.SlotNumber)
                .ToListAsync();

            ViewBag.Seats = seats;
            ViewBag.Slots = slots;
            return View(hall);
        }
    }
}
