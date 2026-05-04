using IMS_Application.Common.Models;
using IMS_Application.DTOs;


namespace IMS_Application.Services.Interfaces
{
    public interface ICategoryService
    {
        Task<Result<ListCategoriesDto>> CreateCategoryAsync(string name, int createdBy);
        Task<Result<List<ListCategoriesDto>>> GetAllCategoriesAsync();
        Task<Result<ListCategoriesDto>> UpdateCategoryAsync(int id, string? name, int updatedBy);
        Task<Result<ListCategoriesDto>> DeleteCategoryAsync(int categoryId, int updatedBy);
        Task<Result<GetCategoryDto>> GetCategoryByIdAsync(int id);

    }
}
