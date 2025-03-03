using Catalog.Repository.Entities;
using Catalog.Repository.Interface;
using Catalog.Service.Models;
using Catalog.Service.Utils;
using FluentAssertions;
using Moq;
using Xunit;

namespace Catalog.Tests.Utils
{
    public class CsvValidatorTests
    {
        private readonly Mock<IProductRepository> _mockProductRepository;

        public CsvValidatorTests()
        {
            _mockProductRepository = new Mock<IProductRepository>();
        }

        [Fact]
        public void ValidateFile_Should_Return_True_When_File_Exists_And_Is_Csv()
        {
            string filePath = "test_valid.csv";
            File.WriteAllText(filePath, "Sample Content");

            bool result = CsvValidator.ValidateFile(filePath);

            result.Should().BeTrue();

            File.Delete(filePath);
        }

        [Fact]
        public void ValidateFile_Should_Return_False_When_File_Does_Not_Exist()
        {
            string filePath = "non_existing.csv";

            bool result = CsvValidator.ValidateFile(filePath);

            result.Should().BeFalse();
        }

        [Fact]
        public void ValidateFile_Should_Return_False_When_File_Has_Invalid_Extension()
        {
            string filePath = "invalid.txt";
            File.WriteAllText(filePath, "Sample Content");

            bool result = CsvValidator.ValidateFile(filePath);

            result.Should().BeFalse();

            File.Delete(filePath);
        }

        [Fact]
        public void ValidateLine_Should_Return_True_When_Line_Is_Valid()
        {
            string validLine = "Product01,P01,Category01,C01";

            bool result = CsvValidator.ValidateLine(validLine, out CsvRecord? record);

            result.Should().BeTrue();
            record.Should().NotBeNull();
            record!.ProductCode.Should().Be("P01");
            record.CategoryCode.Should().Be("C01");
        }

        [Fact]
        public void ValidateLine_Should_Return_False_When_Line_Has_Missing_Columns()
        {
            string invalidLine = "Product01,P01,Category01";

            bool result = CsvValidator.ValidateLine(invalidLine, out CsvRecord? record);

            result.Should().BeFalse();
            record.Should().BeNull();
        }

        [Fact]
        public void ValidateLine_Should_Return_False_When_Line_Is_Empty()
        {
            string emptyLine = string.Empty;

            bool result = CsvValidator.ValidateLine(emptyLine, out CsvRecord? record);

            result.Should().BeFalse();
            record.Should().BeNull();
        }

        [Fact]
        public async Task ValidateUniqueProductCodes_Should_Return_True_When_No_Duplicates()
        {
            var records = new List<CsvRecord>
            {
                new CsvRecord { ProductCode = "P01" },
                new CsvRecord { ProductCode = "P02" }
            };

            SetupMockProdcutRepository();

            var errorSummary = new Dictionary<string, string>();

            bool result = await CsvValidator.ValidateUniqueProductCodes(records, _mockProductRepository.Object, errorSummary);

            result.Should().BeTrue();
            errorSummary.Should().BeEmpty();
        }

        [Fact]
        public async Task ValidateUniqueProductCodes_Should_Return_False_When_Duplicates_In_CSV()
        {
            var records = new List<CsvRecord>
            {
                new CsvRecord { ProductCode = "P01" },
                new CsvRecord { ProductCode = "P01" }
            };

            SetupMockProdcutRepository();

            var errorSummary = new Dictionary<string, string>();

            bool result = await CsvValidator.ValidateUniqueProductCodes(records, _mockProductRepository.Object, errorSummary);

            result.Should().BeFalse();
            errorSummary.Should().ContainKey("Duplicate product codes found in CSV");
        }

        [Fact]
        public async Task ValidateUniqueProductCodes_Should_Return_False_When_Duplicates_In_Database()
        {
            var records = new List<CsvRecord>
            {
                new CsvRecord { ProductCode = "P01" }
            };

            var existingProducts = new List<Product>
            {
                new Product { Code = "P01" }
            };

            SetupMockProdcutRepository(existingProducts);

            var errorSummary = new Dictionary<string, string>();

            bool result = await CsvValidator.ValidateUniqueProductCodes(records, _mockProductRepository.Object, errorSummary);

            result.Should().BeFalse();
            errorSummary.Should().ContainKey("Duplicate product codes found in Database");
        }

        private void SetupMockProdcutRepository(List<Product> products = null)
        {
            products ??= new List<Product>();
            _mockProductRepository
                .Setup(repo => repo.GetAllProductsAsync())
                .ReturnsAsync(products.AsQueryable());
        }
    }
}
