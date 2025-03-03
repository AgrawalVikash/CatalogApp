using Catalog.Service.Models;

namespace Catalog.Service.Interface
{
    public interface ICatalogService
    {
        Task<(IEnumerable<ProductDTO> Products, int TotalPages, int TotalItems)> GetProductsAsync(int page, int pageSize, string? productCode);
    }
}
