using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace Movie_Site_Management_System
{
    /// <summary>
    /// .NET 8 host using the classic Startup pattern.
    /// </summary>
    public class Program
    {
        public static void Main(string[] args) =>
            CreateHostBuilder(args).Build().Run();

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    // Keeps Kestrel, IIS Integration, appsettings.* loading, etc.
                    webBuilder.UseStartup<Startup>();
                });
    }
}
