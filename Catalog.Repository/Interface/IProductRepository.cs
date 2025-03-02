using Catalog.Repository.Entities;

namespace Catalog.Repository.Interface
{
    public interface IProductRepository
    {
        Task<IQueryable<Product>> GetAllProductsAsync();
        Task AddProductsAsync(List<Product> products);
        Task SaveChangesAsync();
    }
}
