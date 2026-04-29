using IMS_Domain.Entities;

namespace IMS_Application.Interfaces
{
    public interface ISubCategoryRepository : IRepository<SubCategory>
    {
        Task<SubCategory?> GetByNameAsync(string name);
        Task<SubCategory?> GetByCategoryIdAndNameAsync(int categoryId, string name);
    }
}
