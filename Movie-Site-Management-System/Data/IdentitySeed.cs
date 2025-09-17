using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Movie_Site_Management_System.Data.Identity;
using Movie_Site_Management_System.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Movie_Site_Management_System.Data
{
    /// <summary>
    /// Seeds default roles (Admin, User) and a default Admin account.
    /// </summary>
    public static class IdentitySeed
    {
        public static async Task SeedAsync(IServiceProvider services)
        {
            var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
            var adminOpts = services.GetRequiredService<IOptions<AdminUserOptions>>().Value;

            // 1) Ensure roles
            foreach (var roleName in new[] { Roles.Admin, Roles.User })
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                    await roleManager.CreateAsync(new IdentityRole(roleName));
            }

            // 2) Ensure default admin
            var adminEmail = adminOpts.Email?.Trim().ToLowerInvariant() ?? "admin@starcineplex.local";
            var existingAdmin = await userManager.FindByEmailAsync(adminEmail);
            if (existingAdmin == null)
            {
                var admin = new ApplicationUser
                {
                    FullName = string.IsNullOrWhiteSpace(adminOpts.FullName) ? "Site Administrator" : adminOpts.FullName,
                    Email = adminEmail,
                    UserName = adminEmail,
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(admin, string.IsNullOrWhiteSpace(adminOpts.Password) ? "Admin#12345" : adminOpts.Password);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(admin, Roles.Admin);
                }
                else
                {
                    var errors = string.Join("; ", result.Errors.Select(e => $"{e.Code}: {e.Description}"));
                    throw new Exception($"Failed to create default admin: {errors}");
                }
            }
        }
    }
}
