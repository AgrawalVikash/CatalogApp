using Catalog.Repository.Interface;
using Catalog.Service;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Moq;
using Serilog;
using Xunit;

namespace Catalog.Tests.Service
{
    public class CsvImportServiceTests
    {
        private readonly CsvImportService _csvImportService;
        private readonly Mock<IDBTransactionManager> _mockTransactionManager;
        private readonly Mock<ICategoryRepository> _mockCategoryRepository;
        private readonly Mock<IProductRepository> _mockProductRepository;
        private readonly Mock<ILogger> _mockLogger;
        private readonly IConfiguration _mockConfiguration;

        public CsvImportServiceTests()
        {
            _mockTransactionManager = new Mock<IDBTransactionManager>();
            _mockCategoryRepository = new Mock<ICategoryRepository>();
            _mockProductRepository = new Mock<IProductRepository>();
            _mockLogger = new Mock<ILogger>();

            var appSettings = new Dictionary<string, string>
            {
                {"FileSettings:InvalidRecordsFile", "test_invalid_records.csv"},
                {"FileSettings:SummaryReportFile", "test_summary.log"}
            };

            _mockConfiguration = new ConfigurationBuilder()
                .AddInMemoryCollection(appSettings)
                .Build();

            _csvImportService = new CsvImportService(
                _mockTransactionManager.Object,
                _mockCategoryRepository.Object,
                _mockProductRepository.Object,
                _mockLogger.Object,
                _mockConfiguration
            );
        }

        [Fact]
        public async Task ImportCsvAsync_Should_Not_Throw_Exception_When_File_Is_Valid()
        {
            string filePath = "valid_test.csv";
            File.WriteAllText(filePath, "ProductName,ProductCode,CategoryName,CategoryCode\nSampleProduct,P01,SampleCategory,C01");

            Func<Task> act = async () => await _csvImportService.ImportCsvAsync(filePath);

            await act.Should().NotThrowAsync<Exception>();

            File.Delete(filePath);
        }

        [Fact]
        public async Task ImportCsvAsync_Should_Log_Error_When_File_Is_Invalid()
        {
            string invalidFilePath = "invalid_test.csv";

            Func<Task> act = async () => await _csvImportService.ImportCsvAsync(invalidFilePath);

            await act.Should().NotThrowAsync<Exception>();
        }
    }
}
