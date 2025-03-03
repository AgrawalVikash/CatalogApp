using Catalog.Repository.Interface;
using Catalog.Service.Interface;
using Catalog.Service.Models;
using Serilog;

namespace Catalog.Service
{
    public class CatalogService : ICatalogService
    {
        private readonly IProductRepository _productRepository;
        private readonly ILogger _logger;

        public CatalogService(IProductRepository productRepository, ILogger logger)
        {
            _productRepository = productRepository;
            _logger = logger;
        }

        public async Task<(IEnumerable<ProductDTO> Products, int TotalPages, int TotalItems)> GetProductsAsync(int page, int pageSize, string? productCode)
        {
            try
            {
                var query = await _productRepository.GetAllProductsAsync();

                if (!string.IsNullOrEmpty(productCode))
                {
                    query = query.Where(p => p.Code.Contains(productCode));
                }

                int totalItems = query.Count();
                int totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

                var products = query
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(p => new ProductDTO
                    {
                        Id = p.Id,
                        Name = p.Name,
                        Code = p.Code,
                        CategoryCode = p.CategoryCode
                    })
                    .ToList();

                return (products, totalPages, totalItems);
            }
            catch (Exception ex)
            {
                _logger.Error($"Error retrieving products: {ex.Message}");
                throw;
            }
        }
    }
}
