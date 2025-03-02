using Catalog.Repository.Entities;

namespace Catalog.Repository.Interface
{
    public interface ICategoryRepository
    {
        Task<IEnumerable<Category>> GetAllCategoriesAsync();
        Task AddCategoriesAsync(List<Category> categories);
        Task SaveChangesAsync();
    }
}
