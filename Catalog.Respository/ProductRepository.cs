using Catalog.Entities;
using Catalog.Respository.Interface;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Catalog.Respository
{
    public class ProductRepository : IProductRepository
    {
        private readonly CatalogDBContext _context;

        public ProductRepository(CatalogDBContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Product>> GetAllProductsAsync()
        {
            return await _context.Products.AsNoTracking().ToListAsync();
        }

        public async Task<int> AddProductsAsync(List<Product> products)
        {
            await _context.Products.AddRangeAsync(products);
            return await _context.SaveChangesAsync();
        }
    }
}
