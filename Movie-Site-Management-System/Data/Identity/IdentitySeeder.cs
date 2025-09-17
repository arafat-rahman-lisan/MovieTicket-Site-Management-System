using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Movie_Site_Management_System.Models;
using System;
using System.Threading.Tasks;

namespace Movie_Site_Management_System.Data.Identity
{
    public static class IdentitySeeder
    {
        public static async Task SeedAsync(
            IServiceProvider services,
            IConfiguration config,
            ILogger logger)
        {
            var roleMgr = services.GetRequiredService<RoleManager<IdentityRole>>();
            var userMgr = services.GetRequiredService<UserManager<ApplicationUser>>();

            // Ensure roles exist
            foreach (var role in new[] { Roles.Admin, Roles.User })
            {
                if (!await roleMgr.RoleExistsAsync(role))
                {
                    var result = await roleMgr.CreateAsync(new IdentityRole(role));
                    if (result.Succeeded)
                        logger.LogInformation("Created role {Role}", role);
                    else
                        logger.LogError("Failed to create role {Role}: {Errors}",
                            role, string.Join("; ", result.Errors));
                }
            }

            // Pull Admin config
            var adminEmail = config["AdminUser:Email"];
            var adminPassword = config["AdminUser:Password"];
            var adminFullName = config["AdminUser:FullName"];

            if (string.IsNullOrWhiteSpace(adminEmail) || string.IsNullOrWhiteSpace(adminPassword))
            {
                logger.LogWarning("AdminUser section missing email or password. Skipping admin seeding.");
                return;
            }

            // Check if admin user exists
            var admin = await userMgr.FindByEmailAsync(adminEmail);
            if (admin == null)
            {
                admin = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FullName = adminFullName ?? "Admin User",
                    EmailConfirmed = true
                };

                var createResult = await userMgr.CreateAsync(admin, adminPassword);
                if (createResult.Succeeded)
                {
                    logger.LogInformation("Created admin user {Email}", adminEmail);
                }
                else
                {
                    logger.LogError("Failed to create admin user: {Errors}",
                        string.Join("; ", createResult.Errors));
                    return;
                }
            }

            // Ensure admin is in Admin role
            if (!await userMgr.IsInRoleAsync(admin, Roles.Admin))
            {
                var result = await userMgr.AddToRoleAsync(admin, Roles.Admin);
                if (result.Succeeded)
                    logger.LogInformation("Added {Email} to Admin role", adminEmail);
                else
                    logger.LogError("Failed to add admin to role: {Errors}",
                        string.Join("; ", result.Errors));
            }
        }
    }
}
