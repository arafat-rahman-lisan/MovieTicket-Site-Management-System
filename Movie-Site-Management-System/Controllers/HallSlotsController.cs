using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Movie_Site_Management_System.Data;
using Movie_Site_Management_System.Data.Identity;
using Movie_Site_Management_System.Models;
using Movie_Site_Management_System.ViewModels.HallSlots;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Movie_Site_Management_System.Controllers
{
    /// <summary>
    /// Browse slots per hall and inspect one slot with its show for a date.
    /// Routes:
    ///   GET    /hallslots?hallId=1
    ///   GET    /hallslots/details/{id}?date=YYYY-MM-DD
    ///   GET    /hallslots/create
    ///   POST   /hallslots/create
    ///   GET    /hallslots/edit/{id}
    ///   POST   /hallslots/edit/{id}
    ///   GET    /hallslots/delete/{id}
    ///   POST   /hallslots/delete/{id}
    /// </summary>
    [Authorize(Roles = Roles.Admin)]
    public class HallSlotsController : Controller
    {
        private readonly AppDbContext _db;
        public HallSlotsController(AppDbContext db) => _db = db;

        // Utility: halls dropdown
        private async Task<IEnumerable<SelectListItem>> HallOptionsAsync(long? selected = null)
        {
            var halls = await _db.Halls
                .AsNoTracking()
                .Include(h => h.Theatre)
                // avoid ?. in expression trees
                .OrderBy(h => h.Theatre != null ? h.Theatre.Name : "")
                .ThenBy(h => h.Name)
                .Select(h => new SelectListItem
                {
                    Value = h.HallId.ToString(),
                    Text = (h.Theatre == null ? "Theatre?" : h.Theatre.Name) + " / " + h.Name,
                    Selected = selected.HasValue && selected.Value == h.HallId
                })
                .ToListAsync();

            return halls;
        }

        // Validation: Start<End + unique slot# in hall + no overlap
        private async Task ValidateSlotAsync(HallSlotEditVM vm, long? currentId = null)
        {
            if (vm.StartTime >= vm.EndTime)
            {
                ModelState.AddModelError(nameof(vm.EndTime), "End time must be after Start time.");
            }

            bool slotNumberTaken = await _db.HallSlots.AsNoTracking().AnyAsync(s =>
                s.HallId == vm.HallId &&
                s.SlotNumber == vm.SlotNumber &&
                (currentId == null || s.HallSlotId != currentId.Value));

            if (slotNumberTaken)
            {
                ModelState.AddModelError(nameof(vm.SlotNumber), "This slot number already exists in the selected hall.");
            }

            bool overlaps = await _db.HallSlots.AsNoTracking().AnyAsync(s =>
                s.HallId == vm.HallId &&
                (currentId == null || s.HallSlotId != currentId.Value) &&
                vm.StartTime < s.EndTime && s.StartTime < vm.EndTime);

            if (overlaps)
            {
                ModelState.AddModelError(nameof(vm.StartTime), "Time range overlaps an existing slot in this hall.");
                ModelState.AddModelError(nameof(vm.EndTime), "Time range overlaps an existing slot in this hall.");
            }
        }

        // GET /hallslots?hallId=1
        public async Task<IActionResult> Index(long? hallId)
        {
            var q = _db.HallSlots
                .AsNoTracking()
                .Include(hs => hs.Hall).ThenInclude(h => h.Theatre)
                .AsQueryable();

            if (hallId.HasValue)
                q = q.Where(hs => hs.HallId == hallId.Value);

            var list = await q
                .OrderBy(hs => hs.Hall != null && hs.Hall.Theatre != null ? hs.Hall.Theatre.Name : "")
                .ThenBy(hs => hs.Hall != null ? hs.Hall.Name : "")
                .ThenBy(hs => hs.SlotNumber)
                .ToListAsync();

            ViewBag.Halls = new SelectList(await HallOptionsAsync(hallId), "Value", "Text");
            return View(list);
        }

        // GET /hallslots/details/{id}?date=YYYY-MM-DD
        public async Task<IActionResult> Details(long id, DateOnly? date)
        {
            var slot = await _db.HallSlots
                .Include(hs => hs.Hall).ThenInclude(h => h.Theatre)
                .AsNoTracking()
                .FirstOrDefaultAsync(hs => hs.HallSlotId == id);

            if (slot == null) return NotFound();

            var targetDate = date ?? DateOnly.FromDateTime(DateTime.UtcNow.Date);

            // include ShowSeats -> Seat -> SeatType so we can read BasePrice(s)
#pragma warning disable CS8602 // false-positive: EF navigation Includes
            var show = await _db.Shows.AsNoTracking()
                .Include(s => s.Movie)
                .Include(s => s.ShowSeats)
                    .ThenInclude(ss => ss.Seat)
                    .ThenInclude(seat => seat.SeatType)
                .FirstOrDefaultAsync(s => s.HallSlotId == id && s.ShowDate == targetDate);
#pragma warning restore CS8602

            ViewBag.Date = targetDate;
            ViewBag.Show = show; // can be null
            return View(slot);
        }

        // ----- Create
        public async Task<IActionResult> Create()
        {
            ViewBag.Halls = await HallOptionsAsync();
            return View(new HallSlotEditVM
            {
                StartTime = new TimeOnly(10, 0),
                EndTime = new TimeOnly(12, 30),
                IsActive = true
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(HallSlotEditVM vm)
        {
            await ValidateSlotAsync(vm);

            if (!ModelState.IsValid)
            {
                ViewBag.Halls = await HallOptionsAsync(vm.HallId);
                return View(vm);
            }

            var entity = new HallSlot
            {
                HallId = vm.HallId,
                SlotNumber = vm.SlotNumber,
                StartTime = vm.StartTime,
                EndTime = vm.EndTime,
                IsActive = vm.IsActive
            };

            _db.HallSlots.Add(entity);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index), new { hallId = vm.HallId });
        }

        // ----- Edit
        public async Task<IActionResult> Edit(long id)
        {
            var s = await _db.HallSlots.AsNoTracking().FirstOrDefaultAsync(x => x.HallSlotId == id);
            if (s == null) return NotFound();

            var vm = new HallSlotEditVM
            {
                HallSlotId = s.HallSlotId,
                HallId = s.HallId,
                SlotNumber = s.SlotNumber,
                StartTime = s.StartTime,
                EndTime = s.EndTime,
                IsActive = s.IsActive
            };

            ViewBag.Halls = await HallOptionsAsync(vm.HallId);
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, HallSlotEditVM vm)
        {
            if (id != vm.HallSlotId) return BadRequest();

            await ValidateSlotAsync(vm, currentId: id);

            if (!ModelState.IsValid)
            {
                ViewBag.Halls = await HallOptionsAsync(vm.HallId);
                return View(vm);
            }

            var s = await _db.HallSlots.FirstOrDefaultAsync(x => x.HallSlotId == id);
            if (s == null) return NotFound();

            s.HallId = vm.HallId;
            s.SlotNumber = vm.SlotNumber;
            s.StartTime = vm.StartTime;
            s.EndTime = vm.EndTime;
            s.IsActive = vm.IsActive;

            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index), new { hallId = vm.HallId });
        }

        // ----- Delete
        public async Task<IActionResult> Delete(long id)
        {
            var s = await _db.HallSlots
                .Include(hs => hs.Hall).ThenInclude(h => h.Theatre)
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.HallSlotId == id);

            if (s == null) return NotFound();
            return View(s);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            var s = await _db.HallSlots.FirstOrDefaultAsync(x => x.HallSlotId == id);
            if (s == null) return NotFound();

            // Prevent delete if any Show exists on this slot.
            bool usedInShows = await _db.Shows.AnyAsync(sh => sh.HallSlotId == id);
            if (usedInShows)
            {
                TempData["Error"] = "Cannot delete: one or more shows exist for this slot.";
                return RedirectToAction(nameof(Delete), new { id });
            }

            // cache before await to avoid CS8602 after removal/save
            var hallIdOfDeleted = s.HallId;

            _db.HallSlots.Remove(s);
            await _db.SaveChangesAsync();

            return RedirectToAction(nameof(Index), new { hallId = hallIdOfDeleted });
        }
    }
}
