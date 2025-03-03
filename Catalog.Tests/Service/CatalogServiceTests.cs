using Catalog.Repository.Entities;
using Catalog.Repository.Interface;
using Catalog.Service;
using FluentAssertions;
using Moq;
using Serilog;
using Xunit;

namespace Catalog.Tests.Service
{
    public class CatalogServiceTests
    {
        private readonly Mock<IProductRepository> _mockProductRepository;
        private readonly Mock<ILogger> _mockLogger;
        private readonly CatalogService _catalogService;

        public CatalogServiceTests()
        {
            _mockProductRepository = new Mock<IProductRepository>();
            _mockLogger = new Mock<ILogger>();

            _catalogService = new CatalogService(
                _mockProductRepository.Object,
                _mockLogger.Object
            );
        }

        [Fact]
        public async Task GetProductsAsync_Should_Return_Paginated_Products()
        {
            var products = new List<Product>
            {
                new Product { Id = Guid.NewGuid(), Name = "Product01", Code = "P01", CategoryCode = "C01" },
                new Product { Id = Guid.NewGuid(), Name = "Product02", Code = "P02", CategoryCode = "C02" },
                new Product { Id = Guid.NewGuid(), Name = "Product03", Code = "P03", CategoryCode = "C03" }
            }.AsQueryable();

            _mockProductRepository
                .Setup(repo => repo.GetAllProductsAsync())
                .ReturnsAsync(products);

            var (result, totalPages, totalItems) = await _catalogService.GetProductsAsync(1, 2, null);

            result.Should().HaveCount(2);
            totalPages.Should().Be(2);
            totalItems.Should().Be(3);
        }

        [Fact]
        public async Task GetProductsAsync_Should_Filter_Products_By_ProductCode()
        {
            var products = new List<Product>
            {
                new Product { Id = Guid.NewGuid(), Name = "Product01", Code = "P01", CategoryCode = "C01" },
                new Product { Id = Guid.NewGuid(), Name = "Product02", Code = "P02", CategoryCode = "C02" }
            }.AsQueryable();

            _mockProductRepository
                .Setup(repo => repo.GetAllProductsAsync())
                .ReturnsAsync(products);

            var (result, totalPages, totalItems) = await _catalogService.GetProductsAsync(1, 2, "P02");

            result.Should().HaveCount(1);
            result.First().Code.Should().Be("P02");
            totalPages.Should().Be(1);
            totalItems.Should().Be(1);
        }

        [Fact]
        public async Task GetProductsAsync_Should_Log_Error_And_Throw_Exception_When_Failure_Occurs()
        {
            _mockProductRepository
                .Setup(repo => repo.GetAllProductsAsync())
                .ThrowsAsync(new Exception("Database error"));

            Func<Task> act = async () => await _catalogService.GetProductsAsync(1, 2, null);

            await act.Should().ThrowAsync<Exception>().WithMessage("Database error");

            _mockLogger.Verify(
                x => x.Error(It.Is<string>(s => s.Contains("Error retrieving products"))),
                Times.Once
            );
        }
    }
}
