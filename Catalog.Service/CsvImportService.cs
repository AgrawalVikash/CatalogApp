using Catalog.Repository.Entities;
using Catalog.Repository.Interface;
using Catalog.Service.Interface;
using Catalog.Service.Models;
using Catalog.Service.Utils;
using Microsoft.Extensions.Configuration;
using Serilog;

namespace Catalog.Service
{
    public class CsvImportService : ICsvImportService
    {
        private readonly IDBTransactionManager _dbTransactionManager;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IProductRepository _productRepository;
        private readonly ILogger _logger;
        private readonly string _invalidRecordsFile;
        private readonly string _summaryReportFile;

        public CsvImportService(IDBTransactionManager dbTransactionManager, ICategoryRepository categoryRepository, IProductRepository productRepository, ILogger logger, IConfiguration configuration)
        {
            _dbTransactionManager = dbTransactionManager;
            _categoryRepository = categoryRepository;
            _productRepository = productRepository;
            _logger = logger;
            var fileSettings = configuration.GetSection("FileSettings");
            _invalidRecordsFile = fileSettings["InvalidRecordsFile"] ?? "invalid_records.csv";
            _summaryReportFile = fileSettings["SummaryReportFile"] ?? "import_summary.log";
        }

        public async Task ImportCsvAsync(string? filePath)
        {
            var errorSummary = new Dictionary<string, string>();
            var categoriesCount = 0;
            var productsCount = 0;
            try
            {
                if (!CsvValidator.ValidateFile(filePath))
                {
                    return;
                }

                _logger.Information($"Starting CSV Import for file: {filePath}");
                var (validData, invalidRecords) = await ReadAndValidateCsvAsync(filePath!, errorSummary);

                if (await CsvValidator.ValidateUniqueProductCodes(validData, _productRepository, errorSummary))
                {
                    var newCategories = await GetNewCategoriesAsync(validData);
                    categoriesCount = newCategories.Count;

                    var newProducts = GetProducts(validData);
                    productsCount = newProducts.Count;

                    await InsertCSVData(newCategories, newProducts);
                }

                var summaryReport = GenerateSummaryReport(categoriesCount, productsCount, invalidRecords.Count, errorSummary);
                DirectoryHelper.EnsureDirectoryExists(_summaryReportFile);
                await File.WriteAllTextAsync(_summaryReportFile, summaryReport);
                if (invalidRecords.Count != 0)
                {
                    DirectoryHelper.EnsureDirectoryExists(_invalidRecordsFile);
                    await File.WriteAllLinesAsync(_invalidRecordsFile, invalidRecords);
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"Error occurred during CSV import: {ex.Message}");
            }
        }

        private async Task InsertCSVData(List<Category> categories, List<Product> products)
        {
            try
            {
                await _dbTransactionManager.BeginTransactionAsync();

                await _categoryRepository.AddCategoriesAsync(categories);
                await _productRepository.AddProductsAsync(products);

                await _dbTransactionManager.CommitTransactionAsync();

                _logger.Information($"CSV import completed successfully. Inserted {categories.Count} new categorie(s) and {products.Count} new product(s)");
            }
            catch (Exception ex)
            {
                await _dbTransactionManager.RollbackTransactionAsync();
                _logger.Error($"Error occurred while inserting CSV data: {ex.Message}");
            }
        }
        private async Task<(List<CsvRecord> validData, List<string> invalidRecords)> ReadAndValidateCsvAsync(string filePath, Dictionary<string, string> errorSummary)
        {
            var validData = new List<CsvRecord>();
            var invalidRecords = new List<string>();

            using (var reader = new StreamReader(filePath))
            {
                // Skip the first line (header)
                await reader.ReadLineAsync();

                while (!reader.EndOfStream)
                {
                    string? line = await reader.ReadLineAsync();

                    if (!CsvValidator.ValidateLine(line, out CsvRecord? record))
                    {
                        _logger.Warning($"Invalid line format: {line}");
                        invalidRecords.Add(line);
                        continue;
                    }
                    validData.Add(record!);
                }
            }

            if (invalidRecords.Count != 0)
            {
                errorSummary["Invalid Format"] = invalidRecords.Count.ToString();
            }

            return (validData, invalidRecords);
        }

        private async Task<List<Category>> GetNewCategoriesAsync(List<CsvRecord> validData)
        {
            var uniqueCategories = validData
                    .Select(x => new Category { Name = x.CategoryName, Code = x.CategoryCode, CreationDate = DateTime.UtcNow })
                    .DistinctBy(c => c.Code)
                    .ToList();

            var existingCategories = await _categoryRepository.GetAllCategoriesAsync();
            var newCategories = uniqueCategories
                    .Where(c => !existingCategories.Any(ec => ec.Code == c.Code))
                    .ToList();

            return newCategories;
        }

        private static List<Product> GetProducts(IEnumerable<CsvRecord> validData)
        {
            return validData
                    .Select(x => new Product { Name = x.ProductName, Code = x.ProductCode, CategoryCode = x.CategoryCode, CreationDate = DateTime.UtcNow })
                    .DistinctBy(c => c.Code)
            .ToList();
        }

        private static string GenerateSummaryReport(int categoriesCount, int productsCount, int invalidRecordsCount, Dictionary<string, string> errorSummary)
        {
            var summary = $"Import Summary:\nNew Categories: {categoriesCount}\nNew Products: {productsCount}\nInvalid Records: {invalidRecordsCount}";
            if (errorSummary.Any())
            {
                summary += "\n\nError Summary:";
                foreach (var error in errorSummary)
                {
                    summary += $"\n{error.Key}: {error.Value}";
                }
            }
            return summary;
        }
    }
}
