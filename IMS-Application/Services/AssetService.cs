﻿﻿﻿using AutoMapper;
using IMS_Application.Common.Constants;
using IMS_Application.Common.Models;
using IMS_Application.DTOs;
using IMS_Application.Interfaces;
using IMS_Application.Services.Interfaces;
using IMS_Domain.Entities;
using Microsoft.Extensions.Logging;

namespace IMS_Application.Services
{
       public class AssetService : IAssetService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<AssetService> _logger;
        private readonly ISettingRepository _settingRepository;

        public AssetService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<AssetService> logger, ISettingRepository settingRepository)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
            _settingRepository = settingRepository;
        }

        public async Task<Result<AssetResponseDto>> Create(CreateAssetDto dto, int createdBy)
        {
            try
            {
                var assetCondition = await _unitOfWork.Assets.GetAssetConditionByIdAsync(dto.ConditionId);
                if (assetCondition == null)
                    return Result<AssetResponseDto>.Failure($"Asset condition with ID {dto.ConditionId} does not exist", 400);

                var asset = _mapper.Map<Asset>(dto);
                asset.CreatedAt = DateTime.UtcNow;
                asset.UpdatedAt = DateTime.UtcNow;
                asset.CreatedBy = createdBy;
                asset.UpdatedBy = createdBy;
                asset.IsActive = true;

                await _unitOfWork.Assets.AddRangeAsync(new List<Asset> { asset });
                await _unitOfWork.SaveChangesAsync();

                await _unitOfWork.Assets.AddHistoryAsync(new AssetHistory
                {
                    AssetId = asset.Id,
                    Action = LogicStrings.ActionCreated,
                    Description = $"Asset {asset.ItemName} created"
                });

                await _settingRepository.AddRecentActivityAsync(new RecentActivity
                {
                    ItemId = asset.Id,
                    ItemName = LogicStrings.AssetItemName,
                    Action = LogicStrings.ActionCreated,
                    UserId = createdBy,
                    Details = $"Asset {asset.ItemName} created",
                    DateTime = DateTime.UtcNow,
                    IsDeleted = false
                });

                await _unitOfWork.SaveChangesAsync();

                return Result<AssetResponseDto>.Success(_mapper.Map<AssetResponseDto>(asset));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating asset");
                return Result<AssetResponseDto>.Failure(ErrorMessages.UnexpectedError, 500);
            }
        }

        public async Task<Result<List<AssetResponseDto>>> GetAllAssetsAsync()
        {
            return await GetAll();
        }
        


        public async Task<Result<List<AssetResponseDto>>> GetAll()
        {


            try
            {
                var assets = await _unitOfWork.Assets.GetAllAsync();
                var result = _mapper.Map<List<AssetResponseDto>>(assets);

                foreach (var dto in result)
                {
                    var children = assets.Where(x => x.ParentAssetId == dto.Id).ToList();
                    dto.Children = _mapper.Map<List<AssetResponseDto>>(children);
                }

                return Result<List<AssetResponseDto>>.Success(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching assets");
                return Result<List<AssetResponseDto>>.Failure(ErrorMessages.UnexpectedError, 500);
            }
        }

        public async Task<Result<AssetResponseDto>> GetById(int id)
        {
            try
            {
                var asset = await _unitOfWork.Assets.GetByIdWithChildrenAsync(id);
                if (asset == null)
                    return Result<AssetResponseDto>.Failure(ErrorMessages.AssetNotFound, 404);

                var response = _mapper.Map<AssetResponseDto>(asset);
                response.Children = _mapper.Map<List<AssetResponseDto>>(asset.ChildAssets?.Where(x => x.IsActive).ToList() ?? new List<Asset>());

                return Result<AssetResponseDto>.Success(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching asset by id {AssetId}", id);
                return Result<AssetResponseDto>.Failure(ErrorMessages.UnexpectedError, 500);
            }
        }

        public async Task<Result<string>> UpdateAssetAsync(UpdateAssetDto dto, int updatedBy)
        {
            return await Update(dto, updatedBy);
        }

        public async Task<Result<string>> Update(UpdateAssetDto dto, int updatedBy)
        {

            try
            {
                var asset = await _unitOfWork.Assets.GetByIdAsync(dto.Id);
                if (asset == null)
                    return Result<string>.Failure(ErrorMessages.AssetNotFound, 404);

                _mapper.Map(dto, asset);
                asset.UpdatedAt = DateTime.UtcNow;
                asset.UpdatedBy = updatedBy;

                await _unitOfWork.Assets.AddHistoryAsync(new AssetHistory
                {
                    AssetId = asset.Id,
                    Action = LogicStrings.ActionUpdated,
                    Description = $"Asset {asset.ItemName} updated"
                });

                await _unitOfWork.SaveChangesAsync();

                await _settingRepository.AddRecentActivityAsync(new RecentActivity
                {
                    ItemId = asset.Id,
                    ItemName = LogicStrings.AssetItemName,
                    Action = LogicStrings.ActionUpdated,
                    UserId = updatedBy,
                    Details = $"Asset {asset.ItemName} updated",
                    DateTime = DateTime.UtcNow,
                    IsDeleted = false
                });

                await _unitOfWork.SaveChangesAsync();

                return Result<string>.Success(SuccessMessages.AssetUpdatedSuccessfully);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating asset {AssetId}", dto.Id);
                return Result<string>.Failure(ErrorMessages.UnexpectedError, 500);
            }
        }

        public async Task<Result<string>> DeleteAssetAsync(int id, int deletedBy)
        {
            var result = await Delete(id, deletedBy);
            return result.IsSuccess
                ? Result<string>.Success(SuccessMessages.AssetDeletedSuccessfully)
                : Result<string>.Failure( ErrorMessages.AssetNotFound, 400);
        }

        public async Task<Result<AssetResponseDto>> Delete(int id, int deletedBy)
        {


            try
            {
                var asset = await _unitOfWork.Assets.GetByIdWithChildrenAsync(id);
                if (asset == null)
                    return Result<AssetResponseDto>.Failure(ErrorMessages.AssetNotFound, 404);

                asset.IsActive = false;
                asset.DeletedAt = DateTime.UtcNow;
                asset.DeletedBy = deletedBy;

                await _unitOfWork.Assets.AddHistoryAsync(new AssetHistory
                {
                    AssetId = asset.Id,
                    Action = LogicStrings.ActionDeleted,
                    Description = $"Asset {asset.ItemName} deleted"
                });

                await _settingRepository.AddRecentActivityAsync(new RecentActivity
                {
                    ItemId = asset.Id,
                    ItemName = LogicStrings.AssetItemName,
                    Action = LogicStrings.ActionDeleted,
                    UserId = deletedBy,
                    Details = $"Asset {asset.ItemName} deleted",
                    DateTime = DateTime.UtcNow,
                    IsDeleted = true
                });

                await _unitOfWork.SaveChangesAsync();

                var response = _mapper.Map<AssetResponseDto>(asset);
                response.Children = _mapper.Map<List<AssetResponseDto>>(asset.ChildAssets?.Where(x => x.IsActive).ToList() ?? new List<Asset>());

                return Result<AssetResponseDto>.Success(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting asset {AssetId}", id);
                return Result<AssetResponseDto>.Failure(ErrorMessages.UnexpectedError, 500);
            }
        }

        public async Task<Result<List<UserDto>>> GetSuggestedUsersAsync()
        {
            try
            {
                var users = await _unitOfWork.Users.GetUsersWithOpenTicketsAsync();
                return Result<List<UserDto>>.Success(_mapper.Map<List<UserDto>>(users));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching suggested users");
                return Result<List<UserDto>>.Failure(ErrorMessages.UnexpectedError, 500);
            }
        }

        public async Task<Result<List<UserDto>>> SearchUsersAsync(string query)
        {
            try
            {
                query ??= string.Empty;
                query = query.Trim();

                var users = await _unitOfWork.Users.GetAllWithRolesAsync();

                var isNumeric = int.TryParse(query, out var userId);

                var filtered = users.Where(u =>
                    (isNumeric && u.Id == userId) ||
                    (!string.IsNullOrEmpty(u.FullName) && u.FullName.Contains(query, StringComparison.OrdinalIgnoreCase)) ||
                    (!string.IsNullOrEmpty(u.Email) && u.Email.Contains(query, StringComparison.OrdinalIgnoreCase)) ||
                    (u.Department != null && !string.IsNullOrEmpty(u.Department.Name) &&
                     u.Department.Name.Contains(query, StringComparison.OrdinalIgnoreCase)) ||
                    (u.Role != null && !string.IsNullOrEmpty(u.Role.Name) &&
                     u.Role.Name.Contains(query, StringComparison.OrdinalIgnoreCase))
                ).ToList();

                return Result<List<UserDto>>.Success(_mapper.Map<List<UserDto>>(filtered));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching users");
                return Result<List<UserDto>>.Failure(ErrorMessages.UnexpectedError, 500);
            }
        }

        public async Task<Result<string>> AssignAssetAsync(AssignAssetDto dto)
        {
            if (dto == null)
                return Result<string>.Failure(ErrorMessages.UnexpectedError, 400);
            var asset = await _unitOfWork.Assets.GetByIdAsync(dto.AssetId);
            if (asset == null)
                return Result<string>.Failure(ErrorMessages.AssetNotFound, 404);

            asset.AssignedTo = dto.UserId;
            asset.AssignDate = dto.AssignedDate;
            asset.ExpectedReturnDate = dto.ExpectedReturnDate;
            asset.Notes = dto.Location;
            asset.UpdatedAt = DateTime.UtcNow;
            asset.UpdatedBy = dto.UserId;

            await _unitOfWork.Assets.AddHistoryAsync(new AssetHistory
            {
                AssetId = asset.Id,
                Action = LogicStrings.ActionAssigned,
                Description = $"Asset {asset.ItemName} assigned"
            });

            await _unitOfWork.SaveChangesAsync();

            await _settingRepository.AddRecentActivityAsync(new RecentActivity
            {
                ItemId = asset.Id,
                ItemName = LogicStrings.AssetItemName,
                Action = LogicStrings.ActionAssigned,
                UserId = dto.UserId,
                Details = $"Asset {asset.ItemName} assigned",
                DateTime = DateTime.UtcNow,
                IsDeleted = false
            });

            await _unitOfWork.SaveChangesAsync();

            return Result<string>.Success(SuccessMessages.AssetAssignedSuccessfully);
        }

        public async Task<Result<string>> AddAssetsAsync(AddAssetDto dto, int createdBy)

        {
            try
            {
                if (dto?.Assets == null || !dto.Assets.Any())
                    return Result<string>.Failure("No assets provided", 400);

                var assets = new List<Asset>();
                var now = DateTime.UtcNow;

                foreach (var assetItem in dto.Assets)
                {
                    var assetCondition = await _unitOfWork.Assets.GetAssetConditionByIdAsync(assetItem.ConditionId);
                    if (assetCondition == null)
                        return Result<string>.Failure($"Asset condition with ID {assetItem.ConditionId} does not exist for asset {assetItem.ItemName}", 400);

                    var asset = _mapper.Map<Asset>(assetItem);
                    asset.CreatedBy = createdBy;
                    asset.CreatedAt = now;
                    asset.UpdatedAt = now;
                    asset.UpdatedBy = createdBy;
                    asset.IsActive = true;

                    asset.AssignedTo = dto.AssignedTo;
                    asset.AssignDate = dto.AssignedDate ?? now;
                    asset.ExpectedReturnDate = dto.ExpectedReturnDate;
                    asset.Notes = dto.Location;

                    assets.Add(asset);
                }

                await _unitOfWork.Assets.AddRangeAsync(assets);
                await _unitOfWork.SaveChangesAsync();

                // Add history + recent activities for each created asset (and optional assignment)
                foreach (var asset in assets)
                {
                    await _unitOfWork.Assets.AddHistoryAsync(new AssetHistory
                    {
                        AssetId = asset.Id,
                        Action = LogicStrings.ActionCreated,
                        Description = $"Asset {asset.ItemName} created",
                        CreatedBy = createdBy
                    });

                    await _settingRepository.AddRecentActivityAsync(new RecentActivity
                    {
                        ItemId = asset.Id,
                        ItemName = LogicStrings.AssetItemName,
                        Action = LogicStrings.ActionCreated,
                        UserId = createdBy,
                        Details = $"Asset {asset.ItemName} created",
                        DateTime = DateTime.UtcNow,
                        IsDeleted = false
                    });

                    if (dto.AssignedTo.HasValue)
                    {
                        await _unitOfWork.Assets.AddHistoryAsync(new AssetHistory
                        {
                            AssetId = asset.Id,
                            Action = LogicStrings.ActionAssigned,
                            Description = $"Asset {asset.ItemName} assigned",
                            CreatedBy = dto.AssignedTo.Value
                        });

                        await _settingRepository.AddRecentActivityAsync(new RecentActivity
                        {
                            ItemId = asset.Id,
                            ItemName = LogicStrings.AssetItemName,
                            Action = LogicStrings.ActionAssigned,
                            UserId = dto.AssignedTo.Value,
                            Details = $"Asset {asset.ItemName} assigned",
                            DateTime = DateTime.UtcNow,
                            IsDeleted = false
                        });
                    }
                }

                await _unitOfWork.SaveChangesAsync();

                return Result<string>.Success(SuccessMessages.AssetsAddedSuccessfully);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding assets");
                return Result<string>.Failure(ErrorMessages.UnexpectedError, 500);
            }
        }

        public async Task<Result<GetAssetByIdResponseDto>> GetAssetByIdAsync(int id)
        {
            try
            {
                var asset = await _unitOfWork.Assets.GetByIdWithChildrenAsync(id);
                if (asset == null)
                    return Result<GetAssetByIdResponseDto>.Failure(ErrorMessages.AssetNotFound, 404);

                var response = _mapper.Map<GetAssetByIdResponseDto>(asset);
                response.Overview.Children = _mapper.Map<List<ChildAssetDto>>(asset.ChildAssets?.Where(c => c.IsActive).ToList() ?? new List<Asset>());

                var network = await _unitOfWork.NetworkDetails.GetByAssetIdAsync(asset.Id);
                if (network != null)
                    response.Assignment.Network = _mapper.Map<NetworkDetailsDto>(network);

                return Result<GetAssetByIdResponseDto>.Success(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during retrieving asset by id {AssetId}", id);
                return Result<GetAssetByIdResponseDto>.Failure(ErrorMessages.UnexpectedError, 500);
            }
        }

        public async Task<Result<string>> AttachChildAsync(AttachChildDto dto)
        {
            try
            {
                var parent = await _unitOfWork.Assets.GetByIdAsync(dto.ParentId);
                if (parent == null || parent.ParentAssetId != null)
                    return Result<string>.Failure(ErrorMessages.InvalidParentAsset, 400);

                var child = await _unitOfWork.Assets.GetByIdAsync(dto.ChildId);
                if (child == null)
                    return Result<string>.Failure(ErrorMessages.ChildAssetNotFound, 404);

                if (child.ParentAssetId != null)
                    return Result<string>.Failure(ErrorMessages.AssetAlreadyAttached, 400);

                if (child.StatusId != 1)
                    return Result<string>.Failure(ErrorMessages.OnlyAvailableAssetsAttachable, 400);

                if (child.AssignedTo != null)
                    return Result<string>.Failure(ErrorMessages.OnlyUnassignedAssetsCanBeAttached, 400);

                child.UpdatedAt = DateTime.UtcNow;
                child.ParentAssetId = parent.Id;

                if (parent.AssignedTo != null)
                {
                    child.AssignedTo = parent.AssignedTo;
                    child.AssignDate = parent.AssignDate;
                    child.StatusId = 2;
                }

                await _unitOfWork.SaveChangesAsync();

                await _unitOfWork.Assets.AddHistoryAsync(new AssetHistory
                {
                    AssetId = child.Id,
                    Action = LogicStrings.ActionAttached,
                    Description = $"Attached to parent asset {parent.ItemName}"
                });

                await _unitOfWork.SaveChangesAsync();
                return Result<string>.Success(SuccessMessages.ChildAttachedSuccessfully);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during attaching child {ChildId} to parent {ParentId}", dto.ChildId, dto.ParentId);
                return Result<string>.Failure(ErrorMessages.UnexpectedError, 500);
            }
        }

        public async Task<Result<string>> CreateAndAttachChildAsync(CreateChildAssetDto dto)
        {
            try
            {
                var parent = await _unitOfWork.Assets.GetByIdAsync(dto.ParentId);
                if (parent == null || parent.ParentAssetId != null)
                    return Result<string>.Failure(ErrorMessages.InvalidParentAsset, 400);

                var serialExists = await _unitOfWork.Assets.SerialExistsAsync(dto.SerialNo);
                if (serialExists)
                    return Result<string>.Failure(ErrorMessages.SerialAlreadyExists, 400);

                var child = _mapper.Map<Asset>(dto);
                child.ParentAssetId = parent.Id;

                if (parent.AssignedTo != null)
                {
                    child.AssignedTo = parent.AssignedTo;
                    child.AssignDate = parent.AssignDate;
                    child.StatusId = 2;
                }

                await _unitOfWork.Assets.AddRangeAsync(new List<Asset> { child });
                await _unitOfWork.SaveChangesAsync();

                await _unitOfWork.Assets.AddHistoryAsync(new AssetHistory
                {
                    AssetId = child.Id,
                    Action = LogicStrings.ActionCreatedAndAttached,
                    Description = $"Created and attached to {parent.ItemName}"
                });

                await _unitOfWork.SaveChangesAsync();
                return Result<string>.Success(SuccessMessages.ChildCreatedAndAttachedSuccessfully);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during creating and attaching child to parent {ParentId}", dto.ParentId);
                return Result<string>.Failure(ErrorMessages.UnexpectedError, 500);
            }
        }

        public async Task<Result<string>> DetachChildAsync(DetachChildDto dto)
        {
            try
            {
                var child = await _unitOfWork.Assets.GetByIdAsync(dto.ChildId);
                if (child == null || child.ParentAssetId == null)
                    return Result<string>.Failure(ErrorMessages.InvalidChildAsset, 400);

                child.UpdatedAt = DateTime.UtcNow;
                child.ParentAssetId = null;
                child.AssignedTo = null;
                child.AssignDate = null;
                child.StatusId = 1;

                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.Assets.AddHistoryAsync(new AssetHistory
                {
                    AssetId = child.Id,
                    Action = LogicStrings.ActionDetached,
                    Description = "Removed from parent asset"
                });

                await _unitOfWork.SaveChangesAsync();
                return Result<string>.Success(SuccessMessages.ChildDetachedSuccessfully);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during detaching child {ChildId}", dto.ChildId);
                return Result<string>.Failure(ErrorMessages.UnexpectedError, 500);
            }
        }

        public async Task<Result<List<AssetListDto>>> FilterAssetsAsync(AssetFilterDto dto)
        {
            try
            {
                var assets = await _unitOfWork.Assets.FilterAsync(dto);
                return Result<List<AssetListDto>>.Success(_mapper.Map<List<AssetListDto>>(assets));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during filtering assets");
                return Result<List<AssetListDto>>.Failure(ErrorMessages.UnexpectedError, 500);
            }
        }

        public async Task<Result<string>> AddOrUpdateNetworkAsync(int assetId, NetworkDetailsDto dto, int userId)
        {
            try
            {
                var asset = await _unitOfWork.Assets.GetByIdAsync(assetId);
                if (asset == null)
                    return Result<string>.Failure(ErrorMessages.AssetNotFound, 404);

                var existing = await _unitOfWork.NetworkDetails.GetByAssetIdAsync(assetId);

                if (existing == null)
                {
                    var newNetwork = _mapper.Map<NetworkDetail>(dto);
                    newNetwork.AssetId = assetId;
                    newNetwork.createdBy = userId.ToString();
                    newNetwork.updatedBy = userId.ToString();

                    await _unitOfWork.NetworkDetails.AddAsync(newNetwork);

                    await _unitOfWork.Assets.AddHistoryAsync(new AssetHistory
                    {
                        AssetId = assetId,
                        Action = LogicStrings.ActionNetworkAdded,
                        Description = $"Network details added for asset {asset.ItemName}"
                    });
                }
                else
                {
                    _mapper.Map(dto, existing);
                    existing.updatedBy = userId.ToString();
                    _unitOfWork.NetworkDetails.Update(existing);

                    await _unitOfWork.Assets.AddHistoryAsync(new AssetHistory
                    {
                        AssetId = assetId,
                        Action = LogicStrings.ActionNetworkUpdated,
                        Description = $"Network details updated for asset {asset.ItemName}"
                    });
                }

                await _unitOfWork.SaveChangesAsync();
                return Result<string>.Success(SuccessMessages.NetworkUpdatedSuccessfully);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during adding/updating network for asset {AssetId}", assetId);
                return Result<string>.Failure(ErrorMessages.UnexpectedError, 500);
            }
        }
    }
}
