using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

// NEW:
using Microsoft.AspNetCore.Authentication.OAuth;
using System.Threading.Tasks;

using Movie_Site_Management_System.Data;
using Movie_Site_Management_System.Data.Identity;
using Movie_Site_Management_System.Models;
using Movie_Site_Management_System.Services.Interfaces;
using Movie_Site_Management_System.Services.Service;
using Movie_Site_Management_System.Services.Invoices;
using QuestPDF.Infrastructure;

namespace Movie_Site_Management_System
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration) => Configuration = configuration;

        public void ConfigureServices(IServiceCollection services)
        {
            // QuestPDF license (must be set once)
            QuestPDF.Settings.License = LicenseType.Community;

            // 1) DbContext configuration (SQL Server)
            services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("DefaultConnectionString")));

            // 2) ASP.NET Core Identity
            services
                .AddIdentity<ApplicationUser, IdentityRole>(opts =>
                {
                    opts.Password.RequireDigit = true;
                    opts.Password.RequireLowercase = true;
                    opts.Password.RequireUppercase = false;
                    opts.Password.RequireNonAlphanumeric = false;
                    opts.Password.RequiredLength = 6;

                    // You disabled email confirmation earlier
                    opts.SignIn.RequireConfirmedEmail = false;
                    opts.SignIn.RequireConfirmedAccount = false;
                })
                .AddEntityFrameworkStores<AppDbContext>()
                .AddDefaultTokenProviders();

            // 3) Cookies
            services.ConfigureApplicationCookie(options =>
            {
                options.LoginPath = "/Account/Login";
                options.AccessDeniedPath = "/Account/AccessDenied";
                options.SlidingExpiration = true;
            });

            // 4) Authorization policies
            services.AddAuthorization(options =>
            {
                options.AddPolicy("AdminOnly", policy => policy.RequireRole(Roles.Admin));
                options.AddPolicy("UserOnly", policy => policy.RequireRole(Roles.User));
            });

            // 5) MVC
            services.AddControllersWithViews();

            // 6) Admin seeding options
            services.Configure<AdminUserOptions>(Configuration.GetSection("AdminUser"));

            // 7) Application services
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

            // 8) PDF Invoice Service
            services.AddScoped<IInvoicePdfService, InvoicePdfService>();

            // 9) Email (SMTP) — options + service
            services.Configure<SmtpOptions>(Configuration.GetSection("Smtp"));
            services.AddScoped<IEmailService, SmtpEmailService>();

            // 10) Google Authentication with global account chooser
            // Identity already adds cookie auth; we just add Google as an external handler.
            services
                .AddAuthentication()
                .AddGoogle(options =>
                {
                    options.ClientId = Configuration["Authentication:Google:ClientId"]!;
                    options.ClientSecret = Configuration["Authentication:Google:ClientSecret"]!;
                    options.SaveTokens = true;

                    // Always show Google account picker
                    options.Events = new OAuthEvents
                    {
                        OnRedirectToAuthorizationEndpoint = ctx =>
                        {
                            ctx.Response.Redirect(ctx.RedirectUri + "&prompt=select_account");
                            return Task.CompletedTask;
                        }
                    };

                    // Optional: respect overridden callback path if you set it in appsettings
                    var cb = Configuration["Authentication:Google:CallbackPath"];
                    if (!string.IsNullOrWhiteSpace(cb))
                        options.CallbackPath = cb!;
                });
        }

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
                // Make Movies/Index the root "/"
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Movies}/{action=Index}/{id?}");
            });

            // ---- Seed database (domain + identity) ----
            AppDbInitializer.SeedAsync(app).GetAwaiter().GetResult();

            using var scope = app.ApplicationServices.CreateScope();
            IdentitySeed.SeedAsync(scope.ServiceProvider).GetAwaiter().GetResult();
        }
    }

    public class AdminUserOptions
    {
        public string Email { get; set; } = "admin@starcineplex.local";
        public string Password { get; set; } = "Admin#12345";
        public string FullName { get; set; } = "Site Administrator";
    }
}
