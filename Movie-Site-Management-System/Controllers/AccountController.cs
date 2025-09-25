using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using Movie_Site_Management_System.Data.Identity;
using Movie_Site_Management_System.Models;
using Movie_Site_Management_System.ViewModels.Account;

using System.Security.Claims;

namespace Movie_Site_Management_System.Controllers
{
    /// <summary>
    /// Authentication & profile + (Admin) users list.
    /// Includes Google external login flow.
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
            var res = await _userManager.UpdateAsync(user);

            if (res.Succeeded)
            {
                TempData["Success"] = "Profile updated.";
                return RedirectToAction(nameof(Profile));
            }

            foreach (var e in res.Errors)
                ModelState.AddModelError(string.Empty, e.Description);

            // keep email displayed even on error
            vm.EmailAddress = user.Email ?? string.Empty;
            return View(vm);
        }

        // ===============
        // Change Password
        // ===============

        [Authorize]
        [HttpGet]
        public IActionResult ChangePassword()
        {
            return View(new ChangePasswordVM());
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordVM vm)
        {
            if (!ModelState.IsValid) return View(vm);

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction(nameof(Login));

            var result = await _userManager.ChangePasswordAsync(user, vm.CurrentPassword, vm.NewPassword);
            if (result.Succeeded)
            {
                await _signInManager.RefreshSignInAsync(user); // refresh auth cookie
                TempData["Success"] = "Password changed successfully.";
                return RedirectToAction(nameof(Profile));
            }

            foreach (var e in result.Errors)
                ModelState.AddModelError(string.Empty, e.Description);

            return View(vm);
        }

        // =====================
        // Admin: Users overview
        // =====================

        [Authorize(Roles = Roles.Admin)]
        public async Task<IActionResult> Users()
        {
            var users = await _userManager.Users
                .OrderBy(u => u.Email)
                .ToListAsync();

            var rows = new List<UserRowVM>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);

                rows.Add(new UserRowVM
                {
                    Id = user.Id,
                    Email = user.Email ?? string.Empty,
                    FullName = user.FullName ?? string.Empty,
                    Roles = string.Join(", ", roles),
                    Locked = user.LockoutEnd.HasValue && user.LockoutEnd.Value > DateTimeOffset.UtcNow
                });
            }

            var model = new UsersIndexVM { Users = rows };
            return View(model);
        }

        // =========================
        // Google External Login FLOW
        // =========================

        /// <summary>
        /// Starts the external login challenge (Google button posts here).
        /// </summary>
        [HttpPost, AllowAnonymous, ValidateAntiForgeryToken]
        public IActionResult ExternalLogin(string provider, string? returnUrl = null)
        {
            var redirectUrl = Url.Action(nameof(ExternalLoginCallback), "Account", new { returnUrl });
            var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl!);
            return Challenge(properties, provider);
        }

        /// <summary>
        /// Handles the Google callback, signs in existing users,
        /// or auto-creates a local ApplicationUser for first-time users.
        /// </summary>
        [AllowAnonymous]
        public async Task<IActionResult> ExternalLoginCallback(string? returnUrl = null, string? remoteError = null)
        {
            returnUrl ??= Url.Action("Index", "Movies");

            if (remoteError != null)
            {
                TempData["Error"] = $"External provider error: {remoteError}";
                return RedirectToAction(nameof(Login), new { returnUrl });
            }

            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                TempData["Error"] = "Failed to load external login information.";
                return RedirectToAction(nameof(Login), new { returnUrl });
            }

            var signInResult = await _signInManager.ExternalLoginSignInAsync(
                info.LoginProvider, info.ProviderKey, isPersistent: false);

            if (signInResult.Succeeded)
            {
                return LocalRedirect(returnUrl!);
            }

            var email = info.Principal.FindFirstValue(ClaimTypes.Email);
            var name = info.Principal.FindFirstValue(ClaimTypes.Name);

            if (email == null)
            {
                TempData["Error"] = "Google did not return an email address.";
                return RedirectToAction(nameof(Login), new { returnUrl });
            }

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                user = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    FullName = name ?? email
                };

                var createRes = await _userManager.CreateAsync(user);
                if (!createRes.Succeeded)
                {
                    foreach (var e in createRes.Errors)
                        ModelState.AddModelError(string.Empty, e.Description);

                    TempData["Error"] = "Could not create user from Google account.";
                    return RedirectToAction(nameof(Login), new { returnUrl });
                }
            }

            // Link external login (idempotent) and sign in
            var _ = await _userManager.AddLoginAsync(user, info);
            await _signInManager.SignInAsync(user, isPersistent: false);
            return LocalRedirect(returnUrl!);
        }
    }
}
