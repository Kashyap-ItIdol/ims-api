using IMS_Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IMS_Application.Interfaces
{
    public interface ICategoryRepository
    {
        Task<Category?> GetByNameAsync(string name);
        Task AddAsync(Category category);
        Task<List<Category>> GetAllActiveCategoriesAsync();
        Task<List<string>> GetAllActiveCategoryNamesAsync();
    }
}
