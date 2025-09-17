using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace Movie_Site_Management_System
{
    /// <summary>
    /// .NET 8 Program using Startup.
    /// </summary>
    public class Program
    {
        public static void Main(string[] args) =>
            CreateHostBuilder(args).Build().Run();

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
