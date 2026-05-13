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
    public class SubCategoryService : ISubCategoryService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<SubCategoryService> _logger;
        private readonly ISettingRepository _settingRepository;

        public SubCategoryService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<SubCategoryService> logger, ISettingRepository settingRepository)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
            _settingRepository = settingRepository;
        }

        public async Task<Result<int>> CreateSubCategoryAsync(string name, int categoryId, int createdBy)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(name))
                {
                    return Result<int>.Failure(ErrorMessages.SubCategoryNameRequired, 400);
                }

                var trimmedName = name.Trim();
                if (trimmedName.Length < 2)
                {
                    return Result<int>.Failure(ErrorMessages.SubCategoryNameTooShort, 400);
                }
                if (!System.Text.RegularExpressions.Regex.IsMatch(trimmedName, @"^[a-zA-Z0-9 &amp;-_]*$"))
                {
                    return Result<int>.Failure(ErrorMessages.SubCategoryNameInvalidChars, 400);
                }

                if (categoryId <= 0)
                {
                    return Result<int>.Failure(ErrorMessages.SubCategoryCategoryIdInvalid, 400);
                }

                if (createdBy <= 0)
                {
                    return Result<int>.Failure(ErrorMessages.SubCategoryInvalidUser, 401);
                }

                var category = await _unitOfWork.Categories.GetByIdAsync(categoryId);
                if (category == null || !category.IsActive)
                {
                    return Result<int>.Failure(ErrorMessages.SubCategoryCategoryNotFound, 404);
                }
                var existing = await _unitOfWork.SubCategories.GetByCategoryIdAndNameAsync(categoryId, trimmedName);
                if (existing != null)
                {
                    return Result<int>.Failure(ErrorMessages.DuplicateSubCategoryName, 400);
                }

                var subCategory = new SubCategory
                {
                    Name = trimmedName,
                    CategoryId = categoryId,
                    CreatedBy = createdBy,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                await _unitOfWork.SubCategories.AddAsync(subCategory);
                await _unitOfWork.SaveChangesAsync();

                await _settingRepository.AddRecentActivityAsync(new RecentActivity
                {
                    ItemId = subCategory.Id,
                    ItemName = LogicStrings.SubCategoryItemName,
                    Action = LogicStrings.ActionCreated,
                    UserId = createdBy,
                    Details = $"SubCategory created: {subCategory.Name}",
                    DateTime = subCategory.CreatedAt,
                    IsDeleted = false
                });
                await _unitOfWork.SaveChangesAsync();

                return Result<int>.Success(subCategory.Id, SuccessMessages.SubCategoryCreated);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating sub category: {Name} for category {CategoryId}", name, categoryId);
                return Result<int>.Failure(ErrorMessages.UnexpectedError, 500);
            }
        }

        public async Task<Result<List<DTOs.SubCategory.SubCategoryDto>>> GetAllSubCategoriesAsync()
        {
            try
            {
                var subCategories = await _unitOfWork.SubCategories.GetAllAsync();
                var subCategoryDtos = _mapper.Map<List<DTOs.SubCategory.SubCategoryDto>>(subCategories);
                return Result<List<DTOs.SubCategory.SubCategoryDto>>.Success(subCategoryDtos, "Sub-categories retrieved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all sub categories");
                return Result<List<DTOs.SubCategory.SubCategoryDto>>.Failure(ErrorMessages.UnexpectedError, 500);
            }
        }

        public async Task<Result<List<DTOs.SubCategory.SubCategoryDto>>> GetSubCategoriesByCategoryIdAsync(int categoryId)
        {
            try
            {
                if (categoryId <= 0)
                {
                    return Result<List<DTOs.SubCategory.SubCategoryDto>>.Failure("Invalid category ID", 400);
                }

                var subCategories = await _unitOfWork.SubCategories.GetByCategoryIdAsync(categoryId);
                var subCategoryDtos = _mapper.Map<List<DTOs.SubCategory.SubCategoryDto>>(subCategories);
                return Result<List<DTOs.SubCategory.SubCategoryDto>>.Success(subCategoryDtos, "Sub-categories retrieved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving sub categories for category {CategoryId}", categoryId);
                return Result<List<DTOs.SubCategory.SubCategoryDto>>.Failure(ErrorMessages.UnexpectedError, 500);
            }
        }

        public async Task<Result<DTOs.SubCategory.SubCategoryDto>> UpdateSubCategoryAsync(int id, UpdateSubCategoryDto request, int updatedBy)
        {
            try
            {
                if (id <= 0)
                {
                    return Result<DTOs.SubCategory.SubCategoryDto>.Failure("Invalid sub-category ID", 400);
                }

                if (updatedBy <= 0)
                {
                    return Result<DTOs.SubCategory.SubCategoryDto>.Failure(ErrorMessages.InvalidCredentials, 401);
                }

                var existingSubCategory = await _unitOfWork.SubCategories.GetByIdAsync(id);
                if (existingSubCategory == null)
                {
                    return Result<DTOs.SubCategory.SubCategoryDto>.Failure("Sub-category not found", 404);
                }

                var category = await _unitOfWork.Categories.GetByIdAsync(request.CategoryId);
                if (category == null || !category.IsActive)
                {
                    return Result<DTOs.SubCategory.SubCategoryDto>.Failure("Category not found or inactive", 404);
                }

                if (existingSubCategory.Name != request.Name.Trim())
                {
                    var duplicate = await _unitOfWork.SubCategories.GetByCategoryIdAndNameAsync(request.CategoryId, request.Name.Trim());
                    if (duplicate != null && duplicate.Id != id)
                    {
                        return Result<DTOs.SubCategory.SubCategoryDto>.Failure("Sub-category with this name already exists in the category", 400);
                    }
                }

                existingSubCategory.Name = request.Name.Trim();
                existingSubCategory.CategoryId = request.CategoryId;
                existingSubCategory.IsActive = request.IsActive;
                existingSubCategory.UpdatedAt = DateTime.UtcNow;
                existingSubCategory.UpdatedBy = updatedBy;

                _unitOfWork.SubCategories.Update(existingSubCategory);
                await _unitOfWork.SaveChangesAsync();

                await _settingRepository.AddRecentActivityAsync(new RecentActivity
                {
                    ItemId = existingSubCategory.Id,
                    ItemName = LogicStrings.SubCategoryItemName,
                    Action = LogicStrings.ActionUpdated,
                    UserId = updatedBy,
                    Details = $"SubCategory updated: {existingSubCategory.Name}",
                    DateTime = existingSubCategory.UpdatedAt ?? DateTime.UtcNow,
                    IsDeleted = false
                });
                await _unitOfWork.SaveChangesAsync();

                var updatedDto = _mapper.Map<DTOs.SubCategory.SubCategoryDto>(existingSubCategory);
                return Result<DTOs.SubCategory.SubCategoryDto>.Success(updatedDto, "Sub-category updated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating sub category with ID: {SubCategoryId}", id);
                return Result<DTOs.SubCategory.SubCategoryDto>.Failure(ErrorMessages.UnexpectedError, 500);
            }
        }

        public async Task<Result<bool>> DeleteSubCategoryAsync(int id, int deletedBy)
        {
            try
            {
                if (id <= 0)
                {
                    return Result<bool>.Failure("Invalid sub-category ID", 400);
                }

                if (deletedBy <= 0)
                {
                    return Result<bool>.Failure(ErrorMessages.InvalidCredentials, 401);
                }

                var existingSubCategory = await _unitOfWork.SubCategories.GetByIdAsync(id);
                if (existingSubCategory == null)
                {
                    return Result<bool>.Failure("Sub-category not found", 404);
                }

                var deletedName = existingSubCategory.Name;
                _unitOfWork.SubCategories.Remove(existingSubCategory);
                await _unitOfWork.SaveChangesAsync();

                await _settingRepository.AddRecentActivityAsync(new RecentActivity
                {
                    ItemId = existingSubCategory.Id,
                    ItemName = LogicStrings.SubCategoryItemName,
                    Action = LogicStrings.ActionDeleted,
                    UserId = deletedBy,
                    Details = $"SubCategory deleted: {deletedName}",
                    DateTime = DateTime.UtcNow,
                    IsDeleted = true
                });
                await _unitOfWork.SaveChangesAsync();

                return Result<bool>.Success(true, "Sub-category deleted successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting sub category with ID: {SubCategoryId}", id);
                return Result<bool>.Failure(ErrorMessages.UnexpectedError, 500);
            }
        }
    }
}
