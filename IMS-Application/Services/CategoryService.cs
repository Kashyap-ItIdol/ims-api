using AutoMapper;
using IMS_Application.Common.Constants;
using IMS_Application.Common.Models;
using IMS_Application.DTOs;
using IMS_Application.Interfaces;
using IMS_Application.Services.Interfaces;
using IMS_Domain.Entities;



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

        public async Task<Result<ListCategoriesDto>> CreateCategoryAsync(CreateCategoryRequestDto request, int currentUserId)
        {
           
            var existingCategory = await _categoryRepository.GetByNameAsync(request.Name);
            if (existingCategory != null)
            {
                
                return Result<ListCategoriesDto>.Failure(ErrorMessages.CategoryalreadyExist, 400);
            }

            int createdBy = currentUserId;
            if (createdBy <= 0)
            {
                return Result<ListCategoriesDto>.Failure(ErrorMessages.InvalidCredentials,401);
            }

            var category = new Category
            {
                Name = request.Name,
                CreatedBy = createdBy,
                IsActive = true
            };

            await _categoryRepository.AddAsync(category);

            var CategoriesDto = new ListCategoriesDto
            {
                Id = category.Id,
                Name = request.Name,
                CreatedBy = createdBy
            };

            
            var createdCategory = await _categoryRepository.GetByNameAsync(request.Name);
            var allCategories = await _categoryRepository.GetAllActiveCategoryNamesAsync();

            return Result<ListCategoriesDto>.Success(CategoriesDto, SuccessMessages.CategoryCreated);
        }

        public async Task<Result<List<ListCategoriesDto>>> GetAllCategoriesAsync()
        {
            var categories = await _categoryRepository.GetAllActiveCategoriesAsync();
            var categoryDtos = _mapper.Map<List<ListCategoriesDto>>(categories);
            return Result<List<ListCategoriesDto>>.Success(categoryDtos, SuccessMessages.AllCategories);
        }


    }
}
