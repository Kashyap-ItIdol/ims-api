using IMS_Application.Common.Models;
using IMS_Application.DTOs;

namespace IMS_Application.Services.Interfaces
{
    public interface ISubCategoryService
    {
        Task<Result<int>> CreateSubCategoryAsync(string name, int categoryId, int createdBy);
        Task<Result<List<DTOs.SubCategory.SubCategoryDto>>> GetAllSubCategoriesAsync();
        Task<Result<DTOs.SubCategory.SubCategoryDto>> UpdateSubCategoryAsync(int id, UpdateSubCategoryDto request, int updatedBy);
        Task<Result<bool>> DeleteSubCategoryAsync(int id, int deletedBy);
    }
}
