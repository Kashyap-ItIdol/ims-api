using AutoMapper;
using IMS_Application.Common.Constants;
using IMS_Application.Common.Models;
using IMS_Application.DTOs;
using IMS_Application.Interfaces;
using IMS_Application.Services.Interfaces;
using IMS_Domain.Entities;
using Microsoft.Extensions.Logging;



namespace IMS_Application.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<CategoryService> _logger;

        public CategoryService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<CategoryService> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<Result<ListCategoriesDto>> CreateCategoryAsync(CreateCategoryRequestDto request, int createdBy)
        {
            try
            {
                // Sanitize Input
                var cleanName = request.Name.Trim();

                //  Business Rule: Prevent Duplicates
                var existingCategory = await _unitOfWork.Categories.GetByNameAsync(cleanName);
                if (existingCategory != null)
                {
                    return Result<ListCategoriesDto>.Failure(ErrorMessages.CategoryalreadyExist, 400);
                }

                // Security Rule: Validate User
                if (createdBy <= 0)
                {
                    return Result<ListCategoriesDto>.Failure(ErrorMessages.InvalidCredentials, 401);
                }

                // Create Entity
                var category = new Category
                {
                    Name = cleanName,
                    CreatedBy = createdBy,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow 
                };

                // Save via Unit of Work
                await _unitOfWork.Categories.AddAsync(category);
                await _unitOfWork.SaveChangesAsync();

                // Map and Return Result
                var categoryDto = _mapper.Map<ListCategoriesDto>(category);
                return Result<ListCategoriesDto>.Success(categoryDto, SuccessMessages.CategoryCreated);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating category: {CategoryName}", request.Name);
                return Result<ListCategoriesDto>.Failure(ErrorMessages.UnexpectedError, 500);
            }
        }

        public async Task<Result<List<ListCategoriesDto>>> GetAllCategoriesAsync()
        {
            try
            {
                var categories = await _unitOfWork.Categories.GetAllActiveCategoriesAsync();
                var categoryDtos = _mapper.Map<List<ListCategoriesDto>>(categories);

                return Result<List<ListCategoriesDto>>.Success(categoryDtos, SuccessMessages.AllCategories);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving categories");
                return Result<List<ListCategoriesDto>>.Failure(ErrorMessages.UnexpectedError, 500);
            }
        }
    }
}
