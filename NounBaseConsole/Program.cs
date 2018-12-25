using System;
using System.Linq;
using NounBase;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace NounBaseConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            // Configuration
            var builder = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
            var configuration = builder.Build();

            // Database
            var optionsBuilder = new DbContextOptionsBuilder<TokenContext>()
                .UseSqlite(configuration.GetConnectionString("DefaultConnection"));
            var context = new TokenContext(optionsBuilder.Options);
            context.Database.EnsureCreated();

            // Services
            var services = new ServiceCollection()
                .AddLogging()
                .AddSingleton(configuration)
                .AddSingleton(optionsBuilder.Options)
                .AddSingleton<ITokenService, TokenService>()
                .AddDbContextPool<TokenContext>(options => options.UseSqlite(configuration.GetConnectionString("DefaultConnection")))
                .BuildServiceProvider();

            // Logger
            var logger = services.GetService<ILoggerFactory>()
                .CreateLogger<Program>();

            logger.LogInformation($"Starting application at: {DateTime.Now}");

            // Service
            var service = services.GetService<ITokenService>();
            // Concept graph
            var countries = service.GetToken("Countries");// Countries has no root context; it is a globally unique context identifier
            var country = service.GetToken("US", countries);
            var states = service.GetToken("States", country);
            var state = service.GetToken("HI", states);
            var cities = service.GetToken("Cities", state);
            var city = service.GetToken("Hilo", cities);
            var cityzips = service.GetToken("Zips", city);
            var zip = service.GetToken("96720", cityzips);
            var streets = service.GetToken("Streets", city);
            var streetName = service.GetToken("Kalanianaole Ave", streets);

            // Test a path
            var nounTestCollectionResult = service.Find(@"Countries\US\States");
            Console.WriteLine(nounTestCollectionResult.Children.Where(x => x.Key.Label == "HI").Count() == 1 ?"PASS":"FAIL");
        }
    }
}
