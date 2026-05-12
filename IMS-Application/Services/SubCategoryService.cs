using AutoMapper;
using IMS_Application.Common.Constants;
using IMS_Application.Common.Models;
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
    }
}
