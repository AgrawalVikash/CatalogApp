using Catalog.Entities;
using Catalog.Respository.Interface;
using Catalog.Service.Interface;
using Catalog.Service.Models;
using Serilog;

namespace Catalog.Service
{
    public class CsvImportService : ICsvImportService
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly IProductRepository _productRepository;
        private readonly ILogger _logger;

        public CsvImportService(ICategoryRepository categoryRepository, IProductRepository productRepository, ILogger logger)
        {
            _categoryRepository = categoryRepository;
            _productRepository = productRepository;
            _logger = logger;
        }

        public async Task ImportCsvAsync(string filePath)
        {
            bool hasErrors = false;
            var errorSummary = new Dictionary<string, int>();
            try
            {
                var validData = ReadAndValidateCsv(filePath, out List<string> skippedRecords, ref hasErrors, errorSummary);
                
                var newCategories = await GetNewCategoriesAsync(validData);
                await _categoryRepository.AddCategoriesAsync(newCategories);

                var newProducts = await GetNewProductsAsync(validData);
                await _productRepository.AddProductsAsync(newProducts);

                _logger.Information("CSV import completed successfully.");
            }
            catch (Exception ex)
            {
                _logger.Error($"Error occurred during CSV import: {ex.Message}");
            }
        }

        private List<CsvRecord> ReadAndValidateCsv(string filePath, out List<string> skippedRecords, ref bool hasErrors, Dictionary<string, int> errorSummary)
        {
            var validData = new List<CsvRecord>();
            skippedRecords = new List<string>();
            var lines = File.ReadAllLines(filePath);

            foreach (var line in lines.Skip(1))
            {
                var data = line.Split(',');
                if (data.Length != 4)
                {
                    _logger.Warning($"Invalid line format: {line}");
                    skippedRecords.Add(line);
                    hasErrors = true;
                    continue;
                }
                validData.Add(new CsvRecord { ProductName = data[0].Trim(), ProductCode = data[1].Trim(), CategoryName = data[2].Trim(), CategoryCode = data[3].Trim() });
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

        private async Task<List<Product>> GetNewProductsAsync(List<CsvRecord> validData)
        {
            var uniqueProducts = validData
                    .Select(x => new Product { Name = x.ProductName, Code = x.ProductCode, CreationDate = DateTime.UtcNow })
                    .DistinctBy(c => c.Code)
                    .ToList();

            var existingProducts = await _productRepository.GetAllProductsAsync();
            var newProdcuts = uniqueProducts
                    .Where(c => !existingProducts.Any(ec => ec.Code == c.Code))
                    .ToList();

            return newProdcuts;
        }
    }
}
