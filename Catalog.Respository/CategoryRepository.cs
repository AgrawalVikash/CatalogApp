﻿using Catalog.Respository.Entities;
using Catalog.Respository.Interface;
using Microsoft.EntityFrameworkCore;

namespace Catalog.Respository
{
    public class CategoryRepository : ICategoryRepository
    {
        private readonly CatalogDBContext _dbContext;
        private readonly DbSet<Category> _dbSet;

        public CategoryRepository(CatalogDBContext dbContext)
        {
            _dbContext = dbContext;
            _dbSet = dbContext.Set<Category>();
        }

        public async Task<IEnumerable<Category>> GetAllCategoriesAsync()
        {
            return await _dbSet.AsNoTracking().ToListAsync();
        }

        public async Task AddCategoriesAsync(List<Category> categories)
        {
            await _dbSet.AddRangeAsync(categories);
        }

        public async Task SaveChangesAsync()
        {
            await _dbContext.SaveChangesAsync();
        }
    }
}
