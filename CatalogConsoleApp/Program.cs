﻿using Catalog.Repository;
using Catalog.Repository.Interface;
using Catalog.Service;
using Catalog.Service.Interface;
using Catalog.Service.Utils;
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

        DirectoryHelper.EnsureDirectoryExists("logs");

        var host = Host.CreateDefaultBuilder()
                .ConfigureServices((context, services) =>
                {
                    services.AddDbContext<CatalogDBContext>(options =>
                        options.UseSqlServer(context.Configuration.GetConnectionString("CatalogDB")));
                    services.AddScoped<ICategoryRepository, CategoryRepository>();
                    services.AddScoped<IProductRepository, ProductRepository>();
                    services.AddScoped<ICsvImportService, CsvImportService>();
                    services.AddScoped<IDBTransactionManager, DBTransactionManager>();
                    services.AddSingleton<ILogger>(Log.Logger);
                })
                .Build();

        using var scope = host.Services.CreateScope();
        var services = scope.ServiceProvider;
        var importService = services.GetRequiredService<ICsvImportService>();

        Console.Write("Enter CSV file path: ");

        await importService.ImportCsvAsync(Console.ReadLine());
    }
}
