using Catalog.Respository.Entities;

namespace Catalog.Respository.Interface
{
    public interface ICategoryRepository
    {
        Task<IEnumerable<Category>> GetAllCategoriesAsync();
        Task AddCategoriesAsync(List<Category> categories);
        Task SaveChangesAsync();
    }
}
