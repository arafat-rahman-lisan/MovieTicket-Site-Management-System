using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Movie_Site_Management_System.Data;
using Movie_Site_Management_System.Data.Identity;
using Movie_Site_Management_System.Models;
using System.Linq;
using System.Threading.Tasks;

namespace Movie_Site_Management_System.Controllers
{
    [Authorize(Roles = Roles.Admin)]
    public class SeatTypesController : Controller
    {
        private readonly AppDbContext _db;
        public SeatTypesController(AppDbContext db) => _db = db;

        // GET: /SeatTypes
        public async Task<IActionResult> Index()
        {
            var list = await _db.SeatTypes
                .AsNoTracking()
                .OrderBy(s => s.BasePrice)
                .ToListAsync();

            return View(list);
        }

        // GET: /SeatTypes/Details/5
        public async Task<IActionResult> Details(short? id)
        {
            if (id is null) return NotFound();

            var st = await _db.SeatTypes
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.SeatTypeId == id);

            if (st is null) return NotFound();
            return View(st);
        }

        // GET: /SeatTypes/Create
        public IActionResult Create() => View();

        // POST: /SeatTypes/Create
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SeatType model)
        {
            // Uniqueness on Name (case-insensitive)
            if (await _db.SeatTypes.AnyAsync(s => s.Name.ToLower() == model.Name.ToLower()))
            {
                ModelState.AddModelError(nameof(model.Name), "Seat type name must be unique.");
            }

            if (!ModelState.IsValid) return View(model);

            _db.SeatTypes.Add(model);
            await _db.SaveChangesAsync();
            TempData["Success"] = "Seat type created.";
            return RedirectToAction(nameof(Index));
        }

        // GET: /SeatTypes/Edit/5
        public async Task<IActionResult> Edit(short? id)
        {
            if (id is null) return NotFound();

            var st = await _db.SeatTypes.FindAsync(id);
            if (st is null) return NotFound();

            return View(st);
        }

        // POST: /SeatTypes/Edit/5
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(short id, SeatType model)
        {
            if (id != model.SeatTypeId) return NotFound();

            // Unique name except self
            if (await _db.SeatTypes.AnyAsync(s =>
                s.SeatTypeId != id && s.Name.ToLower() == model.Name.ToLower()))
            {
                ModelState.AddModelError(nameof(model.Name), "Seat type name must be unique.");
            }

            if (!ModelState.IsValid) return View(model);

            try
            {
                _db.Entry(model).State = EntityState.Modified;
                await _db.SaveChangesAsync();
                TempData["Success"] = "Seat type updated.";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _db.SeatTypes.AnyAsync(s => s.SeatTypeId == id))
                    return NotFound();
                throw;
            }
        }

        // GET: /SeatTypes/Delete/5
        public async Task<IActionResult> Delete(short? id)
        {
            if (id is null) return NotFound();

            var st = await _db.SeatTypes
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.SeatTypeId == id);

            if (st is null) return NotFound();

            // Optional: show related counts to warn user (if you have these tables)
            ViewBag.RelatedSeatCount = await _db.Seats
                .AsNoTracking()
                .CountAsync(se => se.SeatTypeId == id);

            // If you have ShowSeats or similar, also check there:
            // ViewBag.RelatedShowSeatCount = await _db.ShowSeats.AsNoTracking().CountAsync(x => x.SeatTypeId == id);

            return View(st);
        }

        // POST: /SeatTypes/Delete/5
        [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(short id)
        {
            var st = await _db.SeatTypes
                .Include(s => s.Seats) // so we can check quickly
                .FirstOrDefaultAsync(s => s.SeatTypeId == id);

            if (st is null) return NotFound();

            // Hard-protect delete if in use
            var inUseSeats = st.Seats?.Any() == true;
            // If you have ShowSeats:
            // var inUseShowSeats = await _db.ShowSeats.AnyAsync(x => x.SeatTypeId == id);

            if (inUseSeats /*|| inUseShowSeats*/)
            {
                TempData["Error"] = "Cannot delete: this seat type is already in use.";
                return RedirectToAction(nameof(Index));
            }

            _db.SeatTypes.Remove(st);
            await _db.SaveChangesAsync();
            TempData["Success"] = "Seat type deleted.";
            return RedirectToAction(nameof(Index));
        }
    }
}
