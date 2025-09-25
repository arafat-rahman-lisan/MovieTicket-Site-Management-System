using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Movie_Site_Management_System.Data.Identity;
using Movie_Site_Management_System.Models;
using Movie_Site_Management_System.ViewModels.Account;

using System.Linq;
using System.Threading.Tasks;

namespace Movie_Site_Management_System.Controllers
{
    [Authorize(Roles = Roles.Admin)]
    public class UsersController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public UsersController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        // Helper: get the first (primary) role for a user
        private async Task<string?> GetFirstRoleAsync(ApplicationUser u)
        {
            var roles = await _userManager.GetRolesAsync(u);
            return roles.FirstOrDefault();
        }

        // Helper: role select list for Create/Edit
        private async Task<SelectList> BuildRoleSelectListAsync(string? selected = null)
        {
            var roles = _roleManager.Roles.OrderBy(r => r.Name).Select(r => r.Name!).ToList();
            return await Task.FromResult(new SelectList(roles, selected));
        }

        // ========= Index =========
        public async Task<IActionResult> Index()
        {
            var users = await _userManager.Users.OrderBy(u => u.Email).ToListAsync();

            var rows = new List<UserRowVM>();
            foreach (var u in users)
            {
                var firstRole = await GetFirstRoleAsync(u);
                rows.Add(new UserRowVM
                {
                    Id = u.Id,
                    Email = u.Email ?? "",
                    FullName = u.FullName ?? "",
                    Roles = firstRole ?? "",
                    Locked = u.LockoutEnd.HasValue && u.LockoutEnd.Value > DateTimeOffset.UtcNow
                });
            }

            return View("Index", new UsersIndexVM { Users = rows });
        }

        // ========= Create =========
        public async Task<IActionResult> Create()
        {
            ViewBag.Roles = await BuildRoleSelectListAsync();
            return View("Create", new CreateUserVM());
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateUserVM vm)
        {
            ViewBag.Roles = await BuildRoleSelectListAsync(vm.Role);
            if (!ModelState.IsValid) return View("Create", vm);

            if (!await _roleManager.RoleExistsAsync(vm.Role))
            {
                ModelState.AddModelError(nameof(vm.Role), "Role does not exist.");
                return View("Create", vm);
            }

            var user = new ApplicationUser
            {
                UserName = vm.Email,
                Email = vm.Email,
                FullName = vm.FullName,
                EmailConfirmed = true
            };

            var createRes = await _userManager.CreateAsync(user, vm.Password);
            if (!createRes.Succeeded)
            {
                foreach (var e in createRes.Errors) ModelState.AddModelError(string.Empty, e.Description);
                return View("Create", vm);
            }

            var roleRes = await _userManager.AddToRoleAsync(user, vm.Role);
            if (!roleRes.Succeeded)
            {
                foreach (var e in roleRes.Errors) ModelState.AddModelError(string.Empty, e.Description);
                return View("Create", vm);
            }

            TempData["Success"] = "User created.";
            return RedirectToAction(nameof(Index));
        }

        // ========= Edit =========
        public async Task<IActionResult> Edit(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            var role = await GetFirstRoleAsync(user) ?? "";

            var vm = new EditUserVM
            {
                Id = user.Id,
                Email = user.Email ?? "",
                FullName = user.FullName ?? "",
                Role = role,
                LockoutEnabled = user.LockoutEnabled,
                LockoutEnd = user.LockoutEnd
            };

            ViewBag.Roles = await BuildRoleSelectListAsync(role);
            return View("Edit", vm);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditUserVM vm)
        {
            ViewBag.Roles = await BuildRoleSelectListAsync(vm.Role);
            if (!ModelState.IsValid) return View("Edit", vm);

            var user = await _userManager.FindByIdAsync(vm.Id);
            if (user == null) return NotFound();

            user.FullName = vm.FullName;
            user.LockoutEnabled = vm.LockoutEnabled;
            user.LockoutEnd = vm.LockoutEnd;

            var updateRes = await _userManager.UpdateAsync(user);
            if (!updateRes.Succeeded)
            {
                foreach (var e in updateRes.Errors) ModelState.AddModelError(string.Empty, e.Description);
                return View("Edit", vm);
            }

            // replace existing role(s)
            var existingRoles = await _userManager.GetRolesAsync(user);
            if (existingRoles.Any())
            {
                await _userManager.RemoveFromRolesAsync(user, existingRoles);
            }

            var roleRes = await _userManager.AddToRoleAsync(user, vm.Role);
            if (!roleRes.Succeeded)
            {
                foreach (var e in roleRes.Errors) ModelState.AddModelError(string.Empty, e.Description);
                return View("Edit", vm);
            }

            TempData["Success"] = "User updated.";
            return RedirectToAction(nameof(Index));
        }

        // ========= Reset Password =========
        public async Task<IActionResult> ResetPassword(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();
            return View("ResetPassword", new ResetPasswordVM { Id = id, Email = user.Email ?? "" });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordVM vm)
        {
            if (!ModelState.IsValid) return View("ResetPassword", vm);

            var user = await _userManager.FindByIdAsync(vm.Id);
            if (user == null) return NotFound();

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var res = await _userManager.ResetPasswordAsync(user, token, vm.NewPassword);

            if (!res.Succeeded)
            {
                foreach (var e in res.Errors) ModelState.AddModelError(string.Empty, e.Description);
                return View("ResetPassword", vm);
            }

            TempData["Success"] = "Password reset.";
            return RedirectToAction(nameof(Index));
        }

        // ========= Delete =========
        public async Task<IActionResult> Delete(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            var firstRole = await GetFirstRoleAsync(user) ?? "";
            var vm = new UserRowVM
            {
                Id = user.Id,
                Email = user.Email ?? "",
                FullName = user.FullName ?? "",
                Roles = firstRole,
                Locked = user.LockoutEnd.HasValue && user.LockoutEnd.Value > DateTimeOffset.UtcNow
            };
            return View("Delete", vm);
        }

        [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var meId = _userManager.GetUserId(User);
            if (meId == id)
            {
                TempData["Error"] = "You cannot delete your own account.";
                return RedirectToAction(nameof(Index));
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return RedirectToAction(nameof(Index));

            var isAdmin = await _userManager.IsInRoleAsync(user, Roles.Admin);
            if (isAdmin)
            {
                var admins = await _userManager.GetUsersInRoleAsync(Roles.Admin);
                if (admins.Count <= 1)
                {
                    TempData["Error"] = "Cannot delete the last Admin user.";
                    return RedirectToAction(nameof(Index));
                }
            }

            var res = await _userManager.DeleteAsync(user);
            TempData[res.Succeeded ? "Success" : "Error"] =
                res.Succeeded ? "User deleted." : string.Join("; ", res.Errors.Select(e => e.Description));

            return RedirectToAction(nameof(Index));
        }
    }
}
