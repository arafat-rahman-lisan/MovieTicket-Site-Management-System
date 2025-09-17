using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Movie_Site_Management_System.Data;
using Movie_Site_Management_System.Data.Identity;
using System.Linq;
using System.Threading.Tasks;

namespace Movie_Site_Management_System.Controllers
{
    /// <summary>
    /// Read-only view of maintenance blocks by hall (helpful to test seat availability logic later).
    /// Routes:
    ///   GET /seatblocks?hallId=1
    /// </summary>
    [Authorize(Roles = Roles.Admin)]
    public class SeatBlocksController : Controller
    {
        private readonly AppDbContext _db;
        public SeatBlocksController(AppDbContext db) => _db = db;

        // GET /seatblocks?hallId=1
        public async Task<IActionResult> Index(long? hallId)
        {
            var q = _db.SeatBlocks.AsNoTracking()
                .Include(sb => sb.Seat)
                .ThenInclude(s => s.Hall)
                .ThenInclude(h => h.Theatre)
                .AsQueryable();

            if (hallId.HasValue) q = q.Where(sb => sb.Seat.HallId == hallId.Value);

            var list = await q
                .OrderBy(sb => sb.Seat.Hall.Theatre.Name)
                .ThenBy(sb => sb.Seat.Hall.Name)
                .ThenBy(sb => sb.Seat.RowLabel)
                .ThenBy(sb => sb.Seat.SeatNumber)
                .ToListAsync();

            var halls = await _db.Halls.Include(h => h.Theatre).AsNoTracking()
                .OrderBy(h => h.Theatre.Name).ThenBy(h => h.Name).ToListAsync();

            ViewBag.Halls = new SelectList(
                halls.Select(h => new { h.HallId, Label = $"{h.Theatre.Name} / {h.Name}" }),
                "HallId", "Label", hallId
            );

            return View(list);
        }
    }
}
