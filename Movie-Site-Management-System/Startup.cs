using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Movie_Site_Management_System.Data;
using Movie_Site_Management_System.Data.Identity;
using Movie_Site_Management_System.Models;
using Movie_Site_Management_System.Services.Interfaces;
using Movie_Site_Management_System.Services.Service;

namespace Movie_Site_Management_System
{
    /// <summary>
    /// .NET 8 Startup including:
    /// - EF Core (SQL Server)
    /// - ASP.NET Core Identity (ApplicationUser + Roles)
    /// - Cookie paths (Login/AccessDenied)
    /// - Authorization policies
    /// - Your existing service registrations (EntityBaseRepository pattern)
    /// - Domain + Identity seeding on app start
    /// </summary>
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration) => Configuration = configuration;

        // Register services here
        public void ConfigureServices(IServiceCollection services)
        {
            // 1) DbContext configuration (SQL Server)
            services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("DefaultConnectionString")));

            // 2) ASP.NET Core Identity
            services
                .AddIdentity<ApplicationUser, IdentityRole>(opts =>
                {
                    // Tweak as desired
                    opts.Password.RequireDigit = true;
                    opts.Password.RequireLowercase = true;
                    opts.Password.RequireUppercase = false;
                    opts.Password.RequireNonAlphanumeric = false;
                    opts.Password.RequiredLength = 6;

                    opts.SignIn.RequireConfirmedEmail = false;
                    opts.SignIn.RequireConfirmedAccount = false;
                })
                .AddEntityFrameworkStores<AppDbContext>()
                .AddDefaultTokenProviders();

            // 3) Auth cookie paths
            services.ConfigureApplicationCookie(options =>
            {
                options.LoginPath = "/Account/Login";
                options.AccessDeniedPath = "/Account/AccessDenied";
                options.SlidingExpiration = true;
            });

            // 4) Authorization policies (optional helpers)
            services.AddAuthorization(options =>
            {
                options.AddPolicy("AdminOnly", policy => policy.RequireRole(Roles.Admin));
                options.AddPolicy("UserOnly", policy => policy.RequireRole(Roles.User));
            });

            // 5) MVC
            services.AddControllersWithViews();

            // 6) Options for default Admin seeding (from appsettings.json: "AdminUser")
            services.Configure<AdminUserOptions>(Configuration.GetSection("AdminUser"));

            // ===== 7) Application Services (EntityBaseRepository pattern) =====
            services.AddScoped<ITheatresService, TheatresService>();
            services.AddScoped<IHallsService, HallsService>();
            services.AddScoped<IHallSlotsService, HallSlotsService>();
            services.AddScoped<ISeatTypesService, SeatTypesService>();
            services.AddScoped<ISeatsService, SeatsService>();
            services.AddScoped<IMoviesService, MoviesService>();
            services.AddScoped<IShowsService, ShowsService>();
            services.AddScoped<IShowSeatsService, ShowSeatsService>();
            services.AddScoped<IShowNotesService, ShowNotesService>();
            services.AddScoped<ISeatBlocksService, SeatBlocksService>();
            services.AddScoped<IBookingsService, BookingsService>();
            services.AddScoped<IBookingSeatsService, BookingSeatsService>();
            // =================================================================
        }

        // Configure HTTP pipeline here
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            // ✅ Authentication must come before Authorization
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                // ✅ Make Movies/Index the root "/"
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Movies}/{action=Index}/{id?}");
            });

            // ---- Seed database (domain + identity) ----
            // Domain seed (your existing initializer)
            AppDbInitializer.SeedAsync(app).GetAwaiter().GetResult();

            // Identity roles + default admin (from appsettings: AdminUser)
            using var scope = app.ApplicationServices.CreateScope();
            IdentitySeed.SeedAsync(scope.ServiceProvider).GetAwaiter().GetResult();
        }
    }

    /// <summary>
    /// Options for seeding a default Admin user; values pulled from appsettings.json:
    /// 
    /// "AdminUser": {
    ///   "Email": "admin@starcineplex.local",
    ///   "Password": "Admin#12345",
    ///   "FullName": "Site Administrator"
    /// }
    /// </summary>
    public class AdminUserOptions
    {
        public string Email { get; set; } = "admin@starcineplex.local";
        public string Password { get; set; } = "Admin#12345";
        public string FullName { get; set; } = "Site Administrator";
    }
}
