using Catalog.Repository.Entities;
using Catalog.Repository.Interface;
using Microsoft.EntityFrameworkCore;

namespace Catalog.Repository
{
    public class CategoryRepository : ICategoryRepository
    {
        private readonly CatalogDBContext _dbContext;

        public CategoryRepository(CatalogDBContext dbContext)
        {
            _dbContext = dbContext;
        }

        public Task<IQueryable<Category>> GetAllCategoriesAsync()
        {
            return Task.FromResult(_dbContext.Categories.AsNoTracking());
        }

        public async Task AddCategoriesAsync(List<Category> categories)
        {
            await _dbContext.Categories.AddRangeAsync(categories);
        }

        public async Task SaveChangesAsync()
        {
            await _dbContext.SaveChangesAsync();
        }
    }
}
