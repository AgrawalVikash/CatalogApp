namespace Catalog.Repository.Interface
{
    public interface IDBTransactionManager
    {
        Task BeginTransactionAsync();
        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();
        Task SaveChangesAsync();
    }
}
