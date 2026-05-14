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
        private readonly ISettingRepository _settingRepository;

        public CategoryService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<CategoryService> logger, ISettingRepository settingRepository)

        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
            _settingRepository = settingRepository;
        }

        public async Task<Result<ListCategoriesDto>> CreateCategoryAsync(string name, int createdBy)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(name))
                {
                    return Result<ListCategoriesDto>.Failure(ErrorMessages.CategoryNameRequired, 400);
                }
                var cleanName = name.Trim();
                if (cleanName.Length < 2)
                {
                    return Result<ListCategoriesDto>.Failure(ErrorMessages.CategoryNameTooShort, 400);
                }
                if (!System.Text.RegularExpressions.Regex.IsMatch(cleanName, @"^[a-zA-Z0-9 &-_]*$"))
                {
                    return Result<ListCategoriesDto>.Failure(ErrorMessages.CategoryNameInvalidChars, 400);
                }

                var existingCategory = await _unitOfWork.Categories.GetByNameAsync(cleanName);
                if (existingCategory != null)
                {
                    return Result<ListCategoriesDto>.Failure(ErrorMessages.CategoryalreadyExist, 400);
                }

                var category = new Category
                {
                    Name = cleanName,
                    CreatedBy = createdBy,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                await _unitOfWork.Categories.AddAsync(category);
                await _unitOfWork.SaveChangesAsync();

                await _settingRepository.AddRecentActivityAsync(new RecentActivity
                {
                    ItemId = category.Id,
                    ItemName = LogicStrings.CategoryItemName,
                    Action = LogicStrings.ActionCreated,
                    UserId = createdBy,
                    Details = $"Category created: {category.Name}",
                    DateTime = category.CreatedAt,
                    IsDeleted = false
                });
                await _unitOfWork.SaveChangesAsync();

                return Result<ListCategoriesDto>.Success(_mapper.Map<ListCategoriesDto>(category), SuccessMessages.CategoryCreated);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating category: {CategoryName}", name);
                return Result<ListCategoriesDto>.Failure(ErrorMessages.UnexpectedError, 500);
            }
        }

        public async Task<Result<List<ListCategoriesDto>>> GetAllCategoriesAsync()
        {
            try
            {
                var categories = await _unitOfWork.Categories.GetAllActiveCategoriesAsync();

                return Result<List<ListCategoriesDto>>.Success(_mapper.Map<List<ListCategoriesDto>>(categories), SuccessMessages.AllCategories);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving categories");
                return Result<List<ListCategoriesDto>>.Failure(ErrorMessages.UnexpectedError, 500);
            }
        }

        public async Task<Result<ListCategoriesDto>> UpdateCategoryAsync(int id, string name, int updatedBy)
        {
            try
            {
                if (id <= 0 || updatedBy <= 0)
                {
                    return Result<ListCategoriesDto>.Failure(ErrorMessages.InvalidInput, 400);
                }
                if (string.IsNullOrWhiteSpace(name))
                {
                    return Result<ListCategoriesDto>.Failure(ErrorMessages.CategoryNameRequired, 400);
                }

                var category = await _unitOfWork.Categories.GetByIdAsync(id);
                if (category == null)
                {
                    return Result<ListCategoriesDto>.Failure(ErrorMessages.CategoryNotFound, 404);
                }

                var cleanName = name.Trim();
                var existingWithSameName = await _unitOfWork.Categories.GetByNameAsync(cleanName);
                if (existingWithSameName != null && existingWithSameName.Id != id)
                {
                    return Result<ListCategoriesDto>.Failure(ErrorMessages.DuplicateCategoryName, 400);
                }
                category.Name = cleanName;
                category.IsActive = true;
                category.UpdatedAt = DateTime.UtcNow;
                category.UpdatedBy = updatedBy;

                _unitOfWork.Categories.Update(category);
                await _unitOfWork.SaveChangesAsync();

                await _settingRepository.AddRecentActivityAsync(new RecentActivity
                {
                    ItemId = category.Id,
                    ItemName = LogicStrings.CategoryItemName,
                    Action = LogicStrings.ActionUpdated,
                    UserId = updatedBy,
                    Details = $"Category updated: {category.Name}",
                    DateTime = category.UpdatedAt!.Value,
                    IsDeleted = false
                });
                await _unitOfWork.SaveChangesAsync();

                return Result<ListCategoriesDto>.Success(_mapper.Map<ListCategoriesDto>(category), SuccessMessages.CategoryUpdated);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating category ID {CategoryId}: {CategoryName}", id, name);
                return Result<ListCategoriesDto>.Failure(ErrorMessages.UnexpectedError, 500);
            }
        }

        public async Task<Result<ListCategoriesDto>> DeleteCategoryAsync(int categoryId, int updatedBy)
        {
            try
            {
                if (categoryId <= 0 || updatedBy <= 0)
                {
                    return Result<ListCategoriesDto>.Failure(ErrorMessages.InvalidInput, 400);
                }

                var category = await _unitOfWork.Categories.GetByIdAsync(categoryId);
                if (category == null)
                {
                    return Result<ListCategoriesDto>.Failure(ErrorMessages.CategoryNotFound, 404);
                }

                if (!category.IsActive)
                {
                    return Result<ListCategoriesDto>.Failure(ErrorMessages.CategoryAlreadyDeleted, 400);
                }

                category.IsActive = false;
                category.DeletedAt = DateTime.UtcNow;
                category.DeletedBy = updatedBy;

                _unitOfWork.Categories.Update(category);
                await _unitOfWork.SaveChangesAsync();

                await _settingRepository.AddRecentActivityAsync(new RecentActivity
                {
                    ItemId = category.Id,
                    ItemName = LogicStrings.CategoryItemName,
                    Action = LogicStrings.ActionDeleted,
                    UserId = updatedBy,
                    Details = $"Category deleted: {category.Name}",
                    DateTime = category.DeletedAt!.Value,
                    IsDeleted = true
                });
                await _unitOfWork.SaveChangesAsync();

                return Result<ListCategoriesDto>.Success(_mapper.Map<ListCategoriesDto>(category), SuccessMessages.CategoryDeleted);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting category {CategoryId}", categoryId);
                return Result<ListCategoriesDto>.Failure(ErrorMessages.UnexpectedError, 500);
            }
        }

        public async Task<Result<GetCategoryDto>> GetCategoryByIdAsync(int id)
        {
            try
            {
                if (id <= 0)
                {
                    return Result<GetCategoryDto>.Failure(ErrorMessages.CategoryNotFound, 400);
                }

                var category = await _unitOfWork.Categories.GetByIdWithSubCategoriesAsync(id);
                if (category == null)
                {
                    return Result<GetCategoryDto>.Failure(ErrorMessages.CategoryNotFound, 404);
                }

                return Result<GetCategoryDto>.Success(_mapper.Map<GetCategoryDto>(category), SuccessMessages.CategoryById);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving category {CategoryId}", id);
                return Result<GetCategoryDto>.Failure(ErrorMessages.UnexpectedError, 500);
            }
        }
    }
}
