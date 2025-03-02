using Catalog.Repository.Interface;
using Microsoft.EntityFrameworkCore.Storage;

namespace Catalog.Repository
{
    public class DBTransactionManager : IDBTransactionManager
    {
        private readonly CatalogDBContext _dbContext;
        private IDbContextTransaction _transaction;

        public DBTransactionManager(CatalogDBContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task BeginTransactionAsync()
        {
            _transaction = await _dbContext.Database.BeginTransactionAsync();
        }

        public async Task CommitTransactionAsync()
        {
            await _dbContext.SaveChangesAsync();
            await _transaction.CommitAsync();
        }

        public async Task RollbackTransactionAsync()
        {
            await _transaction.RollbackAsync();
        }

        public async Task SaveChangesAsync()
        {
            await _dbContext.SaveChangesAsync();
        }
    }
}
