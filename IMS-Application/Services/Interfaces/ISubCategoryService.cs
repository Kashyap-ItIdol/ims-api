using IMS_Application.Common.Models;
using IMS_Application.DTOs;

namespace IMS_Application.Services.Interfaces
{
    public interface ISubCategoryService
    {
        Task<Result<int>> CreateSubCategoryAsync(string name, int categoryId, int createdBy);
        Task<Result<List<SubCategoryDto>>> GetAllSubCategoriesAsync();
        Task<Result<List<SubCategoryDto>>> GetSubCategoriesByCategoryIdAsync(int categoryId);
        Task<Result<SubCategoryDto>> UpdateSubCategoryAsync(int id, UpdateSubCategoryDto request, int updatedBy);
        Task<Result<bool>> DeleteSubCategoryAsync(int id, int deletedBy);
    }
}
