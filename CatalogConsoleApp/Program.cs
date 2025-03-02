using Catalog.Respository;
using Catalog.Respository.Interface;
using Catalog.Service;
using Catalog.Service.Interface;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

class Program
{
    static async Task Main(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .Build();

        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(configuration)
            .CreateLogger();

        var host = Host.CreateDefaultBuilder()
                .ConfigureServices((context, services) =>
                {
                    services.AddDbContext<CatalogDBContext>(options =>
                        options.UseSqlServer(context.Configuration.GetConnectionString("CatalogDB")));
                    services.AddScoped<ICategoryRepository, CategoryRepository>();
                    services.AddScoped<IProductRepository, ProductRepository>();
                    services.AddScoped<ICsvImportService, CsvImportService>();
                    services.AddSingleton(Log.Logger);
                })
                .Build();

        using var scope = host.Services.CreateScope();
        var services = scope.ServiceProvider;
        var importService = services.GetRequiredService<ICsvImportService>();

        Console.Write("Enter CSV file path: ");
        var filePath = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
        {
            Console.WriteLine("Invalid file path. Exiting.");
            return;
        }

        await importService.ImportCsvAsync(filePath);
        Console.WriteLine("Data import complete.");
    }
}
