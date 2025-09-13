using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Movie_Site_Management_System.Data;

namespace Movie_Site_Management_System.Controllers
{
    /// <summary>
    /// Read-only seat browsing for a hall. Also exposes a JSON seat-map endpoint for UI tests.
    /// Routes:
    ///   GET  /seats?hallId=1
    ///   GET  /seats/grid/{hallId}     -> JSON: seats ordered by row+number
    /// </summary>
    public class SeatsController : Controller
    {
        private readonly AppDbContext _db;
        public SeatsController(AppDbContext db) => _db = db;

        // GET /seats?hallId=1
        public async Task<IActionResult> Index(long? hallId)
        {
            var halls = await _db.Halls.Include(h => h.Theatre).AsNoTracking()
                .OrderBy(h => h.Theatre.Name).ThenBy(h => h.Name).ToListAsync();

            ViewBag.Halls = new SelectList(
                halls.Select(h => new { h.HallId, Label = $"{h.Theatre.Name} / {h.Name}" }),
                "HallId", "Label", hallId
            );

            if (!hallId.HasValue)
                return View(Enumerable.Empty<Movie_Site_Management_System.Models.Seat>());

            var list = await _db.Seats.AsNoTracking()
                .Where(s => s.HallId == hallId.Value)
                .Include(s => s.SeatType)
                .OrderBy(s => s.RowLabel).ThenBy(s => s.SeatNumber)
                .ToListAsync();

            return View(list);
        }

        // GET /seats/grid/1
        // Handy for AJAX seat-map drawing during testing.
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
    }
}
