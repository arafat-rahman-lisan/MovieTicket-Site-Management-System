using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Movie_Site_Management_System.Data;

namespace Movie_Site_Management_System.Controllers
{
    /// <summary>
    /// Read-only list of seat types (useful for price displays/tests).
    /// Routes:
    ///   GET /seattypes
    /// </summary>
    public class SeatTypesController : Controller
    {
        private readonly AppDbContext _db;
        public SeatTypesController(AppDbContext db) => _db = db;

        // GET /seattypes
        public async Task<IActionResult> Index()
        {
            var list = await _db.SeatTypes.AsNoTracking()
                .OrderBy(s => s.BasePrice)
                .ToListAsync();

            return View(list);
        }
    }
}
