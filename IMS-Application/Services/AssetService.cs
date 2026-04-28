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
    public class AssetService : IAssetService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<AssetService> _logger;

        public AssetService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<AssetService> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<Result<string>> AddAssetsAsync(AddAssetDto dto, int createdBy)
        {
            try
            {
                var mainDto = dto.Assets.First(x => x.IsPrimary);
                var serials = dto.Assets.Select(x => x.SerialNo).ToList();

                foreach (var serial in serials)
                {
                    if (await _unitOfWork.Assets.SerialExistsAsync(serial))
                        return Result<string>.Failure(ErrorMessages.SerialAlreadyExistsFormatted, 400);
                }

                if (dto.AssignedTo.HasValue)
                {
                    var user = await _unitOfWork.Users.GetByIdAsync(dto.AssignedTo.Value);

                    if (user == null)
                        return Result<string>.Failure(string.Format(ErrorMessages.UserNotFoundById, dto.AssignedTo.Value), 404);

                    if (!string.IsNullOrEmpty(dto.TableNo))
                    {
                        var isUsed = await _unitOfWork.Users.TableAlreadyAssignedAsync(dto.TableNo);

                        if (isUsed)
                            return Result<string>.Failure(string.Format(ErrorMessages.TableAlreadyAssignedToUser, dto.TableNo), 400);

                        user.TableNo = dto.TableNo;
                    }

                    if (!string.IsNullOrEmpty(dto.Location))
                        user.Location = dto.Location;
                }

                var mainAsset = _mapper.Map<Asset>(mainDto);
                mainAsset.AssignedTo = dto.AssignedTo;
                mainAsset.AssignDate = dto.AssignedTo.HasValue ? (dto.AssignedDate ?? DateTime.UtcNow) : null;
                mainAsset.ExpectedReturnDate = dto.ExpectedReturnDate;
                mainAsset.CreatedAt = DateTime.UtcNow;
                mainAsset.UpdatedAt = DateTime.UtcNow;
                mainAsset.CreatedBy = createdBy;
                mainAsset.UpdatedBy = createdBy;

                await _unitOfWork.Assets.AddRangeAsync(new List<Asset> { mainAsset });
                await _unitOfWork.SaveChangesAsync();

                await _unitOfWork.Assets.AddHistoryAsync(new AssetHistory
                {
                    AssetId = mainAsset.Id,
                    Action = "Created",
                    Description = $"Asset {mainAsset.ItemName} created{(dto.AssignedTo.HasValue ? $" and assigned to user {dto.AssignedTo}" : "")}"
                });

                var childAssets = new List<Asset>();

                foreach (var item in dto.Assets.Where(x => !x.IsPrimary))
                {
                    var asset = _mapper.Map<Asset>(item);
                    asset.StatusId = mainAsset.StatusId;
                    asset.AssignedTo = dto.AssignedTo;
                    asset.AssignDate = mainAsset.AssignDate;
                    asset.ExpectedReturnDate = dto.ExpectedReturnDate;
                    asset.ParentAssetId = mainAsset.Id;
                    asset.CreatedAt = DateTime.UtcNow;
                    asset.UpdatedAt = DateTime.UtcNow;
                    asset.CreatedBy = createdBy;
                    asset.UpdatedBy = createdBy;

                    if (item.IsPurchaseDetailsSame)
                    {
                        asset.Vendor = mainAsset.Vendor;
                        asset.PurchaseCost = mainAsset.PurchaseCost;
                        asset.PurchaseDate = mainAsset.PurchaseDate;
                        asset.InvoiceNumber = mainAsset.InvoiceNumber;
                    }

                    childAssets.Add(asset);
                }

                if (childAssets.Any())
                {
                    await _unitOfWork.Assets.AddRangeAsync(childAssets);
                    await _unitOfWork.SaveChangesAsync();

                    foreach (var child in childAssets)
                    {
                        await _unitOfWork.Assets.AddHistoryAsync(new AssetHistory
                        {
                            AssetId = child.Id,
                            Action = "Created",
                            Description = $"Child asset {child.ItemName} created and attached to parent {mainAsset.ItemName}"
                        });
                    }

                    await _unitOfWork.SaveChangesAsync();
                }
                else
                {
                    await _unitOfWork.SaveChangesAsync();
                }

                return Result<string>.Success(SuccessMessages.AssetsAddedSuccessfully);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during adding assets");
                return Result<string>.Failure(ErrorMessages.UnexpectedError, 500);
            }
        }

        public async Task<Result<List<AssetResponseDto>>> GetAllAssetsAsync()
        {
            try
            {
                var assets = await _unitOfWork.Assets.GetAllAsync();
                var parentAssets = assets.Where(a => a.ParentAssetId == null).ToList();
                var result = _mapper.Map<List<AssetResponseDto>>(parentAssets);

                foreach (var dto in result)
                {
                    var childAssets = assets.Where(x => x.ParentAssetId == dto!.Id).ToList();
                    dto!.Children = _mapper.Map<List<AssetResponseDto>>(childAssets);
                }

                return Result<List<AssetResponseDto>>.Success(result);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during retrieving all assets");
                return Result<List<AssetResponseDto>>.Failure(ErrorMessages.UnexpectedError, 500);
            }
        }

        public async Task<Result<string>> UpdateAssetAsync(UpdateAssetDto dto, int updatedBy)
        {
            try
            {
                var asset = await _unitOfWork.Assets.GetByIdWithChildrenAsync(dto.Id);

                if (asset == null)
                    return Result<string>.Failure(ErrorMessages.AssetNotFound, 404);

                if (await _unitOfWork.Assets.SerialExistsAsync(dto.SerialNo, dto.Id))
                    return Result<string>.Failure(ErrorMessages.SerialAlreadyExists, 400);

                bool isParent = asset.ParentAssetId == null && asset.ChildAssets.Any();
                bool isChild = asset.ParentAssetId != null;

                if (dto.IsManualUnlink && isChild)
                {
                    asset.ParentAssetId = null;
                    asset.StatusId = 1;
                    asset.AssignedTo = null;
                    asset.AssignDate = null;
                    asset.UpdatedAt = DateTime.UtcNow;
                    asset.UpdatedBy = updatedBy;

                    await _unitOfWork.Assets.AddHistoryAsync(new AssetHistory
                    {
                        AssetId = asset.Id,
                        Action = "Unlinked",
                        Description = $"Child asset {asset.ItemName} manually unlinked from parent"
                    });

                    await _unitOfWork.SaveChangesAsync();

                    return Result<string>.Success(SuccessMessages.ChildUnlinkedSuccessfully);
                }

                if (dto.IsFromParentContext)
                {
                    if (isChild && dto.StatusId == 1)
                    {
                        asset.AssignedTo = null;
                        asset.AssignDate = null;
                        asset.UpdatedAt = DateTime.UtcNow;
                        asset.UpdatedBy = updatedBy;

                        await _unitOfWork.Assets.AddHistoryAsync(new AssetHistory
                        {
                            AssetId = asset.Id,
                            Action = "Updated",
                            Description = $"Child asset {asset.ItemName} marked as available"
                        });

                        await _unitOfWork.SaveChangesAsync();

                        return Result<string>.Success(SuccessMessages.ChildMarkedAvailable);
                    }

                    if (isChild && dto.StatusId == 2)
                    {
                        var parent = await _unitOfWork.Assets.GetByIdAsync(asset.ParentAssetId!.Value);

                        if (parent?.AssignedTo == null)
                            return Result<string>.Failure(ErrorMessages.CannotAssignChildParentNotAssigned, 400);

                        if (parent.AssignedTo != dto.AssignedTo)
                            return Result<string>.Failure(ErrorMessages.ChildMustMatchParentAssignment, 400);
                    }

                    if (isParent)
                    {
                        if (asset.AssignedTo != dto.AssignedTo && dto.AssignedTo.HasValue)
                        {
                            foreach (var child in asset.ChildAssets)
                            {
                                child.ParentAssetId = null;
                            }
                        }

                        if (dto.StatusId == 1)
                        {
                            asset.AssignedTo = null;
                            asset.AssignDate = null;
                        }
                    }

                    _mapper.Map(dto, asset);
                    asset.StatusId = dto.StatusId;
                    asset.AssignedTo = dto.AssignedTo;
                    asset.AssignDate = dto.AssignedDate;
                    asset.ExpectedReturnDate = dto.ExpectedReturnDate;
                    asset.UpdatedAt = DateTime.UtcNow;
                    asset.UpdatedBy = updatedBy;

                    await _unitOfWork.Assets.AddHistoryAsync(new AssetHistory
                    {
                        AssetId = asset.Id,
                        Action = "Updated",
                        Description = $"Asset {asset.ItemName} updated from parent context"
                    });

                    await _unitOfWork.SaveChangesAsync();

                    return Result<string>.Success(SuccessMessages.AssetUpdatedSuccessfully);
                }
                else
                {
                    if (dto.StatusId == 2)
                    {
                        if (asset.StatusId != 1)
                            return Result<string>.Failure(ErrorMessages.OnlyAvailableAssetsCanBeAssigned, 400);

                        if (!dto.AssignedTo.HasValue)
                            return Result<string>.Failure(ErrorMessages.AssignedToRequired, 400);

                        if (isChild)
                        {
                            asset.ParentAssetId = null;
                        }

                        asset.AssignedTo = dto.AssignedTo;
                        asset.AssignDate = dto.AssignedDate ?? DateTime.UtcNow;
                        asset.StatusId = 2;
                    }
                    else
                    {
                        asset.StatusId = dto.StatusId;
                        asset.AssignedTo = null;
                        asset.AssignDate = null;
                    }

                    _mapper.Map(dto, asset);
                    asset.UpdatedAt = DateTime.UtcNow;
                    asset.UpdatedBy = updatedBy;

                    await _unitOfWork.Assets.AddHistoryAsync(new AssetHistory
                    {
                        AssetId = asset.Id,
                        Action = "Updated",
                        Description = $"Asset {asset.ItemName} updated"
                    });

                    await _unitOfWork.SaveChangesAsync();

                    return Result<string>.Success(SuccessMessages.AssetUpdatedSuccessfully);
                }
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during updating asset {AssetId}", dto.Id);
                return Result<string>.Failure(ErrorMessages.UnexpectedError, 500);
            }
        }

        public async Task<Result<string>> DeleteAssetAsync(int id, int deletedBy)
        {
            try
            {
                var asset = await _unitOfWork.Assets.GetByIdWithChildrenAsync(id);

                if (asset == null)
                    return Result<string>.Failure(ErrorMessages.AssetNotFound, 404);

                bool isParent = asset.ParentAssetId == null;
                bool isChild = asset.ParentAssetId != null;

                if (isParent)
                {
                    if (asset.ChildAssets.Any())
                    {
                        foreach (var child in asset.ChildAssets)
                        {
                            child.ParentAssetId = null;

                            await _unitOfWork.Assets.AddHistoryAsync(new AssetHistory
                            {
                                AssetId = child.Id,
                                Action = "Detached",
                                Description = $"Child asset {child.ItemName} detached because parent {asset.ItemName} was deleted"
                            });
                        }
                    }

                    asset.IsActive = false;
                    asset.DeletedAt = DateTime.UtcNow;
                    asset.DeletedBy = deletedBy;
                }

                if (isChild)
                {
                    asset.ParentAssetId = null;
                    asset.AssignedTo = null;
                    asset.AssignDate = null;
                    asset.IsActive = false;
                    asset.DeletedAt = DateTime.UtcNow;
                    asset.DeletedBy = deletedBy;
                }

                await _unitOfWork.Assets.AddHistoryAsync(new AssetHistory
                {
                    AssetId = asset.Id,
                    Action = "Deleted",
                    Description = $"Asset {asset.ItemName} deleted"
                });

                await _unitOfWork.SaveChangesAsync();

                return Result<string>.Success(SuccessMessages.AssetDeletedSuccessfully);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during deleting asset {AssetId}", id);
                return Result<string>.Failure(ErrorMessages.UnexpectedError, 500);
            }
        }

        public async Task<Result<List<UserDto>>> GetSuggestedUsersAsync()
        {
            try
            {
                var users = await _unitOfWork.Users.GetUsersWithOpenTicketsAsync();
                var result = _mapper.Map<List<UserDto>>(users);

                return Result<List<UserDto>>.Success(result);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during retrieving suggested users");
                return Result<List<UserDto>>.Failure(ErrorMessages.UnexpectedError, 500);
            }
        }

        public async Task<Result<List<UserDto>>> SearchUsersAsync(string query)
        {
            try
            {
                var users = await _unitOfWork.Users.SearchAsync(query);
                var result = _mapper.Map<List<UserDto>>(users);

                return Result<List<UserDto>>.Success(result);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during searching users with query {Query}", query);
                return Result<List<UserDto>>.Failure(ErrorMessages.UnexpectedError, 500);
            }
        }

        public async Task<Result<string>> AssignAssetAsync(AssignAssetDto dto)
        {
            try
            {
                var asset = await _unitOfWork.Assets.GetByIdAsync(dto.AssetId);

                if (asset == null)
                    return Result<string>.Failure(ErrorMessages.AssetNotFound, 404);

                if (asset.StatusId != 1)
                    return Result<string>.Failure(ErrorMessages.OnlyAvailableAssetsCanBeAssigned, 400);

                var user = await _unitOfWork.Users.GetByIdAsync(dto.UserId);

                if (user == null)
                    return Result<string>.Failure(ErrorMessages.UserNotFound, 404);

                if (!string.IsNullOrEmpty(dto.TableNo))
                {
                    var isTableUsed = await _unitOfWork.Users.TableAlreadyAssignedAsync(dto.TableNo);

                    if (isTableUsed)
                        return Result<string>.Failure(string.Format(ErrorMessages.TableAlreadyAssignedShort, dto.TableNo), 400);

                    user.TableNo = dto.TableNo;
                }

                if (!string.IsNullOrEmpty(dto.Location))
                    user.Location = dto.Location;

                asset.AssignedTo = dto.UserId;
                asset.AssignDate = dto.AssignedDate;
                asset.ExpectedReturnDate = dto.ExpectedReturnDate;
                asset.StatusId = 2;

                await _unitOfWork.Assets.AddHistoryAsync(new AssetHistory
                {
                    AssetId = asset.Id,
                    Action = "Assigned",
                    Description = $"Asset {asset.ItemName} assigned to user {user.FullName}"
                });

                await _unitOfWork.SaveChangesAsync();

                return Result<string>.Success(SuccessMessages.AssetAssignedSuccessfully);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during assigning asset {AssetId} to user {UserId}", dto.AssetId, dto.UserId);
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

                var isParent = asset.ParentAssetId == null;
                var response = _mapper.Map<GetAssetByIdResponseDto>(asset);
                var network = await _unitOfWork.NetworkDetails.GetByAssetIdAsync(asset.Id);

                if (network != null)
                {
                    response.Assignment.Network = _mapper.Map<NetworkDetailsDto>(network);
                }

                List<AssetHistory> historyList;

                if (isParent && asset.ChildAssets.Any(c => c.IsActive))
                {
                    var assetIds = new List<int> { asset.Id };
                    assetIds.AddRange(asset.ChildAssets.Where(c => c.IsActive).Select(c => c.Id));
                    historyList = await _unitOfWork.Assets.GetHistoryByAssetIdsAsync(assetIds);
                }
                else
                {
                    historyList = await _unitOfWork.Assets.GetHistoryByAssetIdAsync(asset.Id);
                }

                response.Assignment.History = _mapper.Map<List<AssetHistoryDto>>(historyList);

                if (isParent)
                {
                    response.Overview.Children = _mapper.Map<List<ChildAssetDto>>(asset.ChildAssets.Where(c => c.IsActive));
                }

                return Result<GetAssetByIdResponseDto>.Success(response);
            }
            catch (OperationCanceledException)
            {
                throw;
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

                child.ParentAssetId = parent.Id;

                if (parent.AssignedTo != null)
                {
                    child.AssignedTo = parent.AssignedTo;
                    child.AssignDate = parent.AssignDate;
                    child.StatusId = 2;
                }

                await _unitOfWork.Assets.AddHistoryAsync(new AssetHistory
                {
                    AssetId = child.Id,
                    Action = "Attached",
                    Description = $"Attached to parent asset {parent.ItemName}"
                });

                await _unitOfWork.SaveChangesAsync();

                return Result<string>.Success(SuccessMessages.ChildAttachedSuccessfully);
            }
            catch (OperationCanceledException)
            {
                throw;
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

                if (await _unitOfWork.Assets.SerialExistsAsync(dto.SerialNo))
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
                    Action = "Created & Attached",
                    Description = $"Created and attached to {parent.ItemName}"
                });

                await _unitOfWork.SaveChangesAsync();

                return Result<string>.Success(SuccessMessages.ChildCreatedAndAttachedSuccessfully);
            }
            catch (OperationCanceledException)
            {
                throw;
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

                child.ParentAssetId = null;
                child.AssignedTo = null;
                child.AssignDate = null;
                child.StatusId = 1;

                await _unitOfWork.Assets.AddHistoryAsync(new AssetHistory
                {
                    AssetId = child.Id,
                    Action = "Detached",
                    Description = "Removed from parent asset"
                });

                await _unitOfWork.SaveChangesAsync();

                return Result<string>.Success(SuccessMessages.ChildDetachedSuccessfully);
            }
            catch (OperationCanceledException)
            {
                throw;
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
                var response = _mapper.Map<List<AssetListDto>>(assets);

                return Result<List<AssetListDto>>.Success(response);
            }
            catch (OperationCanceledException)
            {
                throw;
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
                        Action = "Network Added",
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
                        Action = "Network Updated",
                        Description = $"Network details updated for asset {asset.ItemName}"
                    });
                }

                await _unitOfWork.SaveChangesAsync();

                return Result<string>.Success(SuccessMessages.NetworkUpdatedSuccessfully);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during adding/updating network for asset {AssetId}", assetId);
                return Result<string>.Failure(ErrorMessages.UnexpectedError, 500);
            }
        }
    }
}
