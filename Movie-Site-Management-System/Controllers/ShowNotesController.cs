using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Movie_Site_Management_System.Data;
using Movie_Site_Management_System.Data.Identity;
using System.Linq;
using System.Threading.Tasks;

namespace Movie_Site_Management_System.Controllers
{
    /// <summary>
    /// Read-only list of notes for a show (useful to verify cascade + admin screens later).
    /// Routes:
    ///   GET /shownotes?showId=123
    /// </summary>
    [Authorize(Roles = Roles.Admin)]
    public class ShowNotesController : Controller
    {
        private readonly AppDbContext _db;
        public ShowNotesController(AppDbContext db) => _db = db;

        // GET /shownotes?showId=123
        public async Task<IActionResult> Index(long showId)
        {
            var show = await _db.Shows
                .Include(s => s.Movie)
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.ShowId == showId);

            if (show == null) return NotFound();
            ViewBag.Show = show;

            var notes = await _db.ShowNotes.AsNoTracking()
                .Where(n => n.ShowId == showId)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();

            return View(notes);
        }
    }
}
