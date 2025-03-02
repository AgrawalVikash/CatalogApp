using Catalog.Respository.Entities;

namespace Catalog.Respository.Interface
{
    public interface IProductRepository
    {
        Task<IQueryable<Product>> GetAllProductsAsync();
        Task AddProductsAsync(List<Product> products);
        Task SaveChangesAsync();
    }
}
