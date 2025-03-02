using Catalog.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Catalog.Respository.Interface
{
    public interface IProductRepository
    {
        Task<IQueryable<Product>> GetAllProductsAsync();
        Task AddProductsAsync(List<Product> products);
        Task SaveChangesAsync();
    }
}
