using Catalog.Entities;
using Catalog.Respository.Interface;
using Microsoft.EntityFrameworkCore;

namespace Catalog.Respository
{
    public class ProductRepository : IProductRepository
    {
        private readonly CatalogDBContext _context;

        public ProductRepository(CatalogDBContext context)
        {
            _context = context;
        }

        public Task<IQueryable<Product>> GetAllProductsAsync()
        {
            return Task.FromResult(_context.Products.AsNoTracking());
        }

        public async Task AddProductsAsync(List<Product> products)
        {
            await _context.Products.AddRangeAsync(products);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
