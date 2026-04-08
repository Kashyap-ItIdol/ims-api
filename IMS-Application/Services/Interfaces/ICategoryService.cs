using IMS_Application.DTOs;

namespace IMS_Application.Services.Interfaces
{
    public interface ICategoryService
    {
        Task<ApiResponse<object>> CreateCategoryAsync(CreateCategoryRequestDto request, int currentUserId);
        Task<ApiResponse<List<ListCategoriesDto>>> GetAllCategoriesAsync();

    }
}
