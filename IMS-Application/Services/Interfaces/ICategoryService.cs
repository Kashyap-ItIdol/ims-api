using IMS_Application.Common.Models;
using IMS_Application.DTOs;

namespace IMS_Application.Services.Interfaces
{
    public interface ICategoryService
    {
        Task<Result<ListCategoriesDto>> CreateCategoryAsync(CreateCategoryRequestDto request, int currentUserId);
        Task<Result<List<ListCategoriesDto>>> GetAllCategoriesAsync();

    }
}
