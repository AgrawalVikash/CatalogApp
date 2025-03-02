using Catalog.Repository.Interface;
using Catalog.Service.Models;
using Serilog;

namespace Catalog.Service.Utils
{
    public class CsvValidator
    {
        private static readonly int ExpectedColumns = 4;

        public static bool ValidateFile(string? filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                Log.Error("File path is empty or null.");
                return false;
            }

            if (!File.Exists(filePath))
            {
                Log.Error($"File does not exist at the given path: {filePath}");
                return false;
            }

            if (Path.GetExtension(filePath).ToLower() != ".csv")
            {
                Log.Error("Invalid file extension. Only .csv files are allowed.");
                return false;
            }

            return true;
        }

        public static bool ValidateLine(string? line, out CsvRecord? record)
        {
            record = null;
            if (string.IsNullOrWhiteSpace(line))
            {
                return false;
            }

            var data = line.Split(',');

            if (data.Length != ExpectedColumns)
            {
                return false;
            }

            string productName = data[0].Trim();
            string productCode = data[1].Trim().ToUpper();
            string categoryName = data[2].Trim();
            string categoryCode = data[3].Trim().ToUpper();

            if (string.IsNullOrWhiteSpace(productName) || string.IsNullOrWhiteSpace(productCode) ||
                string.IsNullOrWhiteSpace(categoryName) || string.IsNullOrWhiteSpace(categoryCode))
            {
                return false;
            }

            record = new CsvRecord
            {
                ProductName = productName,
                ProductCode = productCode,
                CategoryName = categoryName,
                CategoryCode = categoryCode
            };

            return true;
        }

        public static async Task<bool> ValidateUniqueProductCodes(IEnumerable<CsvRecord> records, IProductRepository _productRepository, Dictionary<string, string> errorSummary)
        {
            var isUnique = true;
            var productCodesFromCsv = records.Select(r => r.ProductCode).ToList();
            var duplicateInCsv = productCodesFromCsv.GroupBy(code => code).Where(g => g.Count() > 1).Select(g => g.Key).ToList();

            if (duplicateInCsv.Count != 0)
            {
                var duplicateCodesInCSV = string.Join(", ", duplicateInCsv);
                Log.Error($"Duplicate product codes found in CSV: {duplicateCodesInCSV}");
                errorSummary["Duplicate product codes found in CSV"] = duplicateCodesInCSV;
                isUnique = false;
            }

            var existingProductCodes = (await _productRepository.GetAllProductsAsync()).Select(p => p.Code).ToList();
            var duplicateInDb = productCodesFromCsv.Intersect(existingProductCodes).ToList();

            if (duplicateInDb.Count != 0)
            {
                var duplicateCodesInDb = string.Join(", ", duplicateInDb);
                Log.Error($"Duplicate product codes found in Database: {duplicateCodesInDb}");
                errorSummary["Duplicate product codes found in Database"] = duplicateCodesInDb;
                isUnique = false;
            }

            return isUnique;
        }
    }
}
