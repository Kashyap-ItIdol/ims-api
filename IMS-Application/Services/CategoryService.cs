using AutoMapper;
using IMS_Application.DTOs;
using IMS_Application.Interfaces;
using IMS_Application.Services.Interfaces;
using IMS_Domain.Entities;
using System.Security.Claims;


namespace IMS_Application.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly IMapper _mapper;

        public CategoryService(ICategoryRepository categoryRepository, IMapper mapper)
        {
            _categoryRepository = categoryRepository;
            _mapper = mapper;
        }

        public async Task<ApiResponse<object>> CreateCategoryAsync(CreateCategoryRequestDto request, int currentUserId)
        {
            // Check if category name already exists
            var existingCategory = await _categoryRepository.GetByNameAsync(request.Name);
            if (existingCategory != null)
            {
                return ApiResponse<object>.APIResponse(400, "Category name already exists", null, false);
            }

            int createdBy = currentUserId;
            if (createdBy <= 0)
            {
                return ApiResponse<object>.APIResponse(401, "Invalid CreatedBy", null, false);
            }

            var category = new Category
            {
                Name = request.Name,
                CreatedBy = createdBy,
                IsActive = true
            };

            await _categoryRepository.AddAsync(category);

            // Fetch the created category for full details
            var createdCategory = await _categoryRepository.GetByNameAsync(request.Name);
            var allCategories = await _categoryRepository.GetAllActiveCategoryNamesAsync();

            return ApiResponse<object>.APIResponse(200, "create category successfully", createdCategory, true);
        }

        public async Task<ApiResponse<List<ListCategoriesDto>>> GetAllCategoriesAsync()
        {
            var categories = await _categoryRepository.GetAllActiveCategoriesAsync();
            var categoryDtos = _mapper.Map<List<ListCategoriesDto>>(categories);
            return ApiResponse<List<ListCategoriesDto>>.APIResponse(200, "get all categories successfully", categoryDtos, true);
        }


    }
}
