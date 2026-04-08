using IMS_Domain.Entities;

namespace IMS_Application.Interfaces
{
    public interface ICategoryRepository : IRepository<Category>
    {
        Task<Category?> GetByNameAsync(string name);
        Task<List<Category>> GetAllActiveCategoriesAsync();
        Task<List<string>> GetAllActiveCategoryNamesAsync();
    }
}
