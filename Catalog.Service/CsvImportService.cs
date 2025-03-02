using Catalog.Respository.Entities;
using Catalog.Respository.Interface;
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
        private readonly string _invalidRecordsFile = "invalid_records.csv";
        private readonly string _summaryReportFile = "import_summary.log";

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
                if(!CsvValidator.ValidateFile(filePath))
                {
                    return;
                }

                _logger.Information($"Starting CSV Import for file: {filePath}");
                var validData = ReadAndValidateCsv(filePath!, out List<string> invalidRecords, errorSummary);

                if(await ValidateUniqueProductCodes(validData, errorSummary))
                {
                    await _dbTransactionManager.BeginTransactionAsync();

                    var newCategories = await GetNewCategoriesAsync(validData);
                    categoriesCount = newCategories.Count;
                    await _categoryRepository.AddCategoriesAsync(newCategories);

                    var newProducts = GetProducts(validData);
                    productsCount = newProducts.Count;
                    await _productRepository.AddProductsAsync(newProducts);

                    await _categoryRepository.SaveChangesAsync();
                    await _productRepository.SaveChangesAsync();

                    await _dbTransactionManager.CommitTransactionAsync();

                    _logger.Information($"CSV import completed successfully. Inserted {categoriesCount} new categories and {productsCount} new products");
                }

                var summaryReport = GenerateSummaryReport(categoriesCount, productsCount, invalidRecords.Count, errorSummary);
                await File.WriteAllTextAsync(_summaryReportFile, summaryReport);
                if (invalidRecords.Count != 0)
                {
                    await File.WriteAllLinesAsync(_invalidRecordsFile, invalidRecords);
                }
            }
            catch (Exception ex)
            {
                await _dbTransactionManager.RollbackTransactionAsync();
                _logger.Error($"Error occurred during CSV import: {ex.Message}");
            }
        }

        private List<CsvRecord> ReadAndValidateCsv(string filePath, out List<string> invalidRecords, Dictionary<string, string> errorSummary)
        {
            var validData = new List<CsvRecord>();
            invalidRecords = new List<string>();

            using (var reader = new StreamReader(filePath))
            {
                // Skip the first line (header)
                reader.ReadLineAsync();

                while (!reader.EndOfStream)
                {
                    string line = reader.ReadLine()!;
                    var data = line.Split(',');
                    if (data.Length != 4)
                    {
                        _logger.Warning($"Invalid line format: {line}");
                        invalidRecords.Add(line);
                        continue;
                    }
                    validData.Add(new CsvRecord { ProductName = data[0].Trim(), ProductCode = data[1].Trim().ToUpper(), CategoryName = data[2].Trim(), CategoryCode = data[3].Trim().ToUpper() });
                }
            }

            if (invalidRecords.Count != 0)
            {
                errorSummary["Invalid Format"] = invalidRecords.Count.ToString();
            }

            return validData;
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
                    .Select(x => new Product { Name = x.ProductName, Code = x.ProductCode, CreationDate = DateTime.UtcNow })
                    .DistinctBy(c => c.Code)
                    .ToList();
        }

        private async Task<bool> ValidateUniqueProductCodes(IEnumerable<CsvRecord> records, Dictionary<string, string> errorSummary)
        {
            var isUnique = true;
            var productCodesFromCsv = records.Select(r => r.ProductCode).ToList();
            var duplicateInCsv = productCodesFromCsv.GroupBy(code => code).Where(g => g.Count() > 1).Select(g => g.Key).ToList();

            if (duplicateInCsv.Count != 0)
            {
                var duplicateCodesInCSV = string.Join(", ", duplicateInCsv);
                _logger.Error($"Duplicate product codes found in CSV: {duplicateCodesInCSV}");
                errorSummary["Duplicate product codes found in CSV"] = duplicateCodesInCSV;
                isUnique = false;
            }

            var existingProductCodes = (await _productRepository.GetAllProductsAsync()).Select(p => p.Code).ToList();
            var duplicateInDb = productCodesFromCsv.Intersect(existingProductCodes).ToList();

            if (duplicateInDb.Count != 0)
            {
                var duplicateCodesInDb = string.Join(", ", duplicateInDb);
                _logger.Error($"Duplicate product codes found in Database: {duplicateCodesInDb}");
                errorSummary["Duplicate product codes found in Database"] = duplicateCodesInDb;
                isUnique = false;
            }

            return isUnique;
        }

        private static string GenerateSummaryReport(int categoriesCount, int productsCount, int invalidRecordsCount, Dictionary<string, string> errorSummary)
        {
            var summary = $"Import Summary:\nNew Categories: {categoriesCount}\nNew Products: {productsCount}\nInvalid Records: {invalidRecordsCount}";
            if (errorSummary.Any())
            {
                summary += "\nError Summary:";
                foreach (var error in errorSummary)
                {
                    summary += $"\n{error.Key}: {error.Value}";
                }
            }
            return summary;
        }
    }
}
