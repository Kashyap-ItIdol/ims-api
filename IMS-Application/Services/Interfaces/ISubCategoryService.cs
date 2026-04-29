using IMS_Application.Common.Models;

namespace IMS_Application.Services.Interfaces
{
    public interface ISubCategoryService
    {
        Task<Result<int>> CreateSubCategoryAsync(string name, int categoryId, int createdBy);
    }
}
