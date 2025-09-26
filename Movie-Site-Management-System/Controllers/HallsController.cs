using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Movie_Site_Management_System.Data;
using Movie_Site_Management_System.Data.Identity;
using Movie_Site_Management_System.Models;
using Movie_Site_Management_System.ViewModels.Halls;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Movie_Site_Management_System.Controllers
{
    [Authorize(Roles = Roles.Admin)]
    public class HallsController : Controller
    {
        private readonly AppDbContext _db;
        public HallsController(AppDbContext db) => _db = db;

        private async Task<IEnumerable<SelectListItem>> TheatreOptionsAsync(long? selectedId = null)
        {
            return await _db.Theatres
                .AsNoTracking()
                .OrderBy(t => t.Name)
                .Select(t => new SelectListItem
                {
                    Value = t.TheatreId.ToString(),
                    Text = t.Name,
                    Selected = selectedId.HasValue && selectedId.Value == t.TheatreId
                })
                .ToListAsync();
        }

        // GET: /Halls
        public async Task<IActionResult> Index(long? theatreId)
        {
            var q = _db.Halls
                .AsNoTracking()
                .Include(h => h.Theatre)
                .AsQueryable();

            if (theatreId.HasValue)
                q = q.Where(h => h.TheatreId == theatreId.Value);

            var list = await q
                .OrderBy(h => h.Theatre.Name).ThenBy(h => h.Name)
                .ToListAsync();

            ViewBag.Theatres = new SelectList(
                await _db.Theatres.AsNoTracking().OrderBy(t => t.Name).ToListAsync(),
                "TheatreId", "Name", theatreId
            );

            return View(list);
        }

        // GET: /Halls/Details/5
        public async Task<IActionResult> Details(long id)
        {
            var hall = await _db.Halls
                .AsNoTracking()
                .Include(h => h.Theatre)
                .FirstOrDefaultAsync(h => h.HallId == id);

            if (hall == null) return NotFound();

            var seatCount = await _db.Seats.AsNoTracking().CountAsync(s => s.HallId == id);
            var slotCount = await _db.HallSlots.AsNoTracking().CountAsync(s => s.HallId == id);

            // ShowCount via HallSlot join (since Show has no HallId)
            var showCount = await (
                from sh in _db.Shows.AsNoTracking()
                join hs in _db.HallSlots.AsNoTracking() on sh.HallSlotId equals hs.HallSlotId
                where hs.HallId == id
                select sh
            ).CountAsync();

            ViewBag.SeatCount = seatCount;
            ViewBag.SlotCount = slotCount;
            ViewBag.ShowCount = showCount;

            return View(hall);
        }

        // GET: /Halls/Create
        public async Task<IActionResult> Create()
        {
            var vm = new HallFormVM
            {
                Theatres = await TheatreOptionsAsync()
            };
            return View(vm);
        }

        // POST: /Halls/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(HallFormVM vm)
        {
            if (!ModelState.IsValid)
            {
                vm.Theatres = await TheatreOptionsAsync(vm.TheatreId);
                return View(vm);
            }

            // normalize name + pre-check duplicate in the same theatre
            var normalizedName = (vm.Name ?? string.Empty).Trim();

            var exists = await _db.Halls.AsNoTracking()
                .AnyAsync(h => h.TheatreId == vm.TheatreId && h.Name == normalizedName);

            if (exists)
            {
                ModelState.AddModelError("Name", "A hall with this name already exists in the selected theatre.");
                vm.Theatres = await TheatreOptionsAsync(vm.TheatreId);
                return View(vm);
            }

            var hall = new Hall
            {
                Name = normalizedName,
                Capacity = vm.Capacity,
                SeatmapVersion = vm.SeatmapVersion,
                IsActive = vm.IsActive,
                TheatreId = vm.TheatreId
            };

            _db.Halls.Add(hall);

            try
            {
                await _db.SaveChangesAsync();
            }
            // fallback if DB unique index still catches a race-condition
            catch (DbUpdateException ex) when (ex.InnerException is Microsoft.Data.SqlClient.SqlException sql &&
                                              (sql.Number == 2601 || sql.Number == 2627))
            {
                ModelState.AddModelError("Name", "A hall with this name already exists in the selected theatre.");
                vm.Theatres = await TheatreOptionsAsync(vm.TheatreId);
                return View(vm);
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: /Halls/Edit/5
        public async Task<IActionResult> Edit(long id)
        {
            var hall = await _db.Halls.FindAsync(id);
            if (hall == null) return NotFound();

            var vm = new HallFormVM
            {
                HallId = hall.HallId,
                Name = hall.Name,
                Capacity = hall.Capacity,
                SeatmapVersion = hall.SeatmapVersion,
                IsActive = hall.IsActive,
                TheatreId = hall.TheatreId,
                Theatres = await TheatreOptionsAsync(hall.TheatreId)
            };

            return View(vm);
        }

        // POST: /Halls/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, HallFormVM vm)
        {
            if (id != vm.HallId) return BadRequest();

            if (!ModelState.IsValid)
            {
                vm.Theatres = await TheatreOptionsAsync(vm.TheatreId);
                return View(vm);
            }

            var hall = await _db.Halls.FirstOrDefaultAsync(h => h.HallId == id);
            if (hall == null) return NotFound();

            var normalizedName = (vm.Name ?? string.Empty).Trim();

            // pre-check duplicate (excluding self)
            var exists = await _db.Halls.AsNoTracking()
                .AnyAsync(h => h.TheatreId == vm.TheatreId &&
                               h.Name == normalizedName &&
                               h.HallId != id);

            if (exists)
            {
                ModelState.AddModelError("Name", "A hall with this name already exists in the selected theatre.");
                vm.Theatres = await TheatreOptionsAsync(vm.TheatreId);
                return View(vm);
            }

            hall.Name = normalizedName;
            hall.Capacity = vm.Capacity;
            hall.SeatmapVersion = vm.SeatmapVersion;
            hall.IsActive = vm.IsActive;
            hall.TheatreId = vm.TheatreId;

            try
            {
                await _db.SaveChangesAsync();
            }
            catch (DbUpdateException ex) when (ex.InnerException is Microsoft.Data.SqlClient.SqlException sql &&
                                              (sql.Number == 2601 || sql.Number == 2627))
            {
                ModelState.AddModelError("Name", "A hall with this name already exists in the selected theatre.");
                vm.Theatres = await TheatreOptionsAsync(vm.TheatreId);
                return View(vm);
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: /Halls/Delete/5
        public async Task<IActionResult> Delete(long id)
        {
            var hall = await _db.Halls
                .Include(h => h.Theatre)
                .FirstOrDefaultAsync(h => h.HallId == id);
            if (hall == null) return NotFound();

            var slotCount = await _db.HallSlots.CountAsync(s => s.HallId == id);

            // seats and links
            var seats = await _db.Seats
                .Where(s => s.HallId == id)
                .Select(s => new { s.SeatId })
                .ToListAsync();
            var seatCount = seats.Count;
            var seatIds = seats.Select(s => s.SeatId).ToList();

            var showCount = await (
                from sh in _db.Shows
                join hs in _db.HallSlots on sh.HallSlotId equals hs.HallSlotId
                where hs.HallId == id
                select sh
            ).CountAsync();

            int seatLinks = 0;
            if (seatIds.Count > 0)
            {
                seatLinks += await _db.ShowSeats.CountAsync(x => seatIds.Contains(x.SeatId));
                seatLinks += await _db.BookingSeats.CountAsync(x => seatIds.Contains(x.SeatId));
                seatLinks += await _db.SeatBlocks.CountAsync(x => seatIds.Contains(x.SeatId));
            }

            ViewBag.SlotCount = slotCount;
            ViewBag.SeatCount = seatCount;
            ViewBag.ShowCount = showCount;
            ViewBag.SeatLinks = seatLinks;

            // Can allow admin to delete seats with the hall only if:
            // no shows, and seats are not linked anywhere.
            ViewBag.CanDeleteSeats = seatCount > 0 && seatLinks == 0 && showCount == 0;

            return View(hall);
        }

        // POST: /Halls/Delete/5  (supports "deleteSeats" checkbox)
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(long id, bool? deleteSeats)
        {
            var hall = await _db.Halls.FirstOrDefaultAsync(h => h.HallId == id);
            if (hall == null) return NotFound();

            var hasSlots = await _db.HallSlots.AnyAsync(s => s.HallId == id);
            var hasShows = await (
                from sh in _db.Shows
                join hs in _db.HallSlots on sh.HallSlotId equals hs.HallSlotId
                where hs.HallId == id
                select sh
            ).AnyAsync();

            // Seats and links
            var seats = await _db.Seats.Where(s => s.HallId == id).ToListAsync();
            var seatIds = seats.Select(s => s.SeatId).ToList();
            int seatLinks = 0;
            if (seatIds.Count > 0)
            {
                seatLinks += await _db.ShowSeats.CountAsync(x => seatIds.Contains(x.SeatId));
                seatLinks += await _db.BookingSeats.CountAsync(x => seatIds.Contains(x.SeatId));
                seatLinks += await _db.SeatBlocks.CountAsync(x => seatIds.Contains(x.SeatId));
            }

            // Hard dependencies block deletion
            if (hasShows || hasSlots)
            {
                TempData["Danger"] = "Cannot delete this hall because it still has Shows and/or Slots. Remove those first.";
                return RedirectToAction(nameof(Delete), new { id });
            }

            // If seats exist but admin didn't confirm deleting them, bounce back
            if (seats.Count > 0 && deleteSeats != true)
            {
                TempData["Warning"] = "This hall has seats. Tick 'Also delete seats' to confirm.";
                return RedirectToAction(nameof(Delete), new { id });
            }

            // Seats referenced elsewhere — do not allow force delete
            if (seats.Count > 0 && seatLinks > 0)
            {
                TempData["Danger"] = "Some seats are referenced by ShowSeats/Bookings/Blocks. Remove those references first.";
                return RedirectToAction(nameof(Delete), new { id });
            }

            using var tx = await _db.Database.BeginTransactionAsync();
            try
            {
                if (seats.Count > 0 && deleteSeats == true)
                    _db.Seats.RemoveRange(seats);

                _db.Halls.Remove(hall);
                await _db.SaveChangesAsync();
                await tx.CommitAsync();

                TempData["Success"] = $"Hall '{hall.Name}' was deleted.";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException ex)
            {
                await tx.RollbackAsync();
                TempData["Danger"] = "Delete failed due to related data. " + ex.Message;
                return RedirectToAction(nameof(Delete), new { id });
            }
        }
    }
}
