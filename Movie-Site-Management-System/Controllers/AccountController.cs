using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Movie_Site_Management_System.Data.Identity;
using Movie_Site_Management_System.Models;
using Movie_Site_Management_System.ViewModels.Account;

namespace Movie_Site_Management_System.Controllers
{
    /// <summary>
    /// Authentication & profile + (Admin) users list.
    /// </summary>
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
        }

        // =========================
        // Login / Register / Logout
        // =========================

        [AllowAnonymous]
        public IActionResult Login(string? returnUrl = null)
            => View(new LoginVM { ReturnUrl = returnUrl });

        [HttpPost, AllowAnonymous, ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginVM vm)
        {
            if (!ModelState.IsValid) return View(vm);

            var user = await _userManager.FindByEmailAsync(vm.EmailAddress);
            if (user == null)
            {
                TempData["Error"] = "Invalid credentials.";
                return View(vm);
            }

            var result = await _signInManager.PasswordSignInAsync(
                user, vm.Password, vm.RememberMe, lockoutOnFailure: false);

            if (!result.Succeeded)
            {
                TempData["Error"] = "Login failed.";
                return View(vm);
            }

            if (!string.IsNullOrWhiteSpace(vm.ReturnUrl) && Url.IsLocalUrl(vm.ReturnUrl))
                return LocalRedirect(vm.ReturnUrl);

            return RedirectToAction("Index", "Movies");
        }

        [AllowAnonymous]
        public IActionResult Register() => View(new RegisterVM());

        [HttpPost, AllowAnonymous, ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterVM vm)
        {
            if (!ModelState.IsValid) return View(vm);

            var userExists = await _userManager.FindByEmailAsync(vm.EmailAddress);
            if (userExists != null)
            {
                TempData["Error"] = "Email is already registered.";
                return View(vm);
            }

            var user = new ApplicationUser
            {
                Email = vm.EmailAddress,
                UserName = vm.EmailAddress,
                FullName = vm.FullName
            };

            var result = await _userManager.CreateAsync(user, vm.Password);
            if (!result.Succeeded)
            {
                foreach (var e in result.Errors)
                    ModelState.AddModelError(string.Empty, e.Description);

                return View(vm);
            }

            await _signInManager.SignInAsync(user, isPersistent: false);
            return RedirectToAction("Index", "Movies");
        }

        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Movies");
        }

        [AllowAnonymous]
        public IActionResult AccessDenied() => View();

        // =========
        // My Profile
        // =========

        [Authorize]
        public async Task<IActionResult> Profile()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction(nameof(Login));

            var vm = new ProfileVM
            {
                FullName = user.FullName ?? string.Empty,
                EmailAddress = user.Email ?? string.Empty
            };
            return View(vm);
        }

        [HttpPost, Authorize, ValidateAntiForgeryToken]
        public async Task<IActionResult> Profile(ProfileVM vm)
        {
            if (!ModelState.IsValid) return View(vm);

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction(nameof(Login));

            user.FullName = vm.FullName;
            await _userManager.UpdateAsync(user);

            TempData["Success"] = "Profile updated.";
            return RedirectToAction(nameof(Profile));
        }

        // =====================
        // Admin: Users overview
        // =====================

        [Authorize(Roles = Roles.Admin)]
        public async Task<IActionResult> Users()
        {
            // Build a lightweight projection first (no roles yet).
            var users = await _userManager.Users
                .OrderBy(u => u.Email) // Email may be null in theory; OrderBy handles nulls.
                .Select(u => new UserRow
                {
                    Email = u.Email ?? string.Empty,
                    FullName = u.FullName ?? string.Empty,
                    Roles = string.Empty
                })
                .ToListAsync();

            // Fill Roles for each user (needs UserManager).
            foreach (var u in users)
            {
                var user = await _userManager.FindByEmailAsync(u.Email);
                if (user == null) continue;

                var roles = await _userManager.GetRolesAsync(user);
                u.Roles = string.Join(", ", roles);
            }

            var model = new UsersVM { Users = users };
            return View(model);
        }
    }
}
