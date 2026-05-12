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

        public AssetService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<AssetService> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        private int GetStatusId(string status)
        {
            return status.ToLower() switch
            {
                "active" => 1,
                "inactive" => 2,
                "assigned" => 3,
                _ => throw new ArgumentException($"Unknown asset status: {status}")
            };
        }

        // ---------------- CREATE ----------------
        public async Task<Result<AssetResponseDto>> Create(
            CreateAssetDto dto,
            int createdBy)
        {
            try
            {
                // Validate that ConditionId exists in AssetConditions table
                var assetCondition = await _unitOfWork.Assets.GetAssetConditionByIdAsync(dto.ConditionId);
                if (assetCondition == null)
                {
                    _logger.LogError("Asset condition with ID {ConditionId} does not exist", dto.ConditionId);
                    return Result<AssetResponseDto>.Failure($"Asset condition with ID {dto.ConditionId} does not exist", 400);
                }

                // Validate that StatusId exists in AssetStatuses table
                var statusId = GetStatusId(dto.Status);
                var assetStatus = await _unitOfWork.Assets.GetAssetStatusByIdAsync(statusId);
                if (assetStatus == null)
                {
                    _logger.LogError("Asset status with ID {StatusId} does not exist", statusId);
                    return Result<AssetResponseDto>.Failure($"Asset status '{dto.Status}' is not valid", 400);
                }

                var asset = _mapper.Map<Asset>(dto);
                asset.StatusId = statusId; // Ensure correct StatusId is set

                asset.CreatedBy = createdBy;
                asset.CreatedAt = DateTime.UtcNow;
                asset.UpdatedAt = DateTime.UtcNow;

                await _unitOfWork.Assets.AddRangeAsync(
                    new List<Asset> { asset });

                await _unitOfWork.SaveChangesAsync();

                var response = _mapper.Map<AssetResponseDto>(asset);

                return Result<AssetResponseDto>.Success(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating asset");

                return Result<AssetResponseDto>.Failure(
                    ErrorMessages.UnexpectedError,
                    500);
            }
        }

        // ---------------- GET ALL ----------------
        public async Task<Result<List<AssetResponseDto>>> GetAll()
        {
            try
            {
                var assets = await _unitOfWork.Assets.GetAllAsync();

                var parents = assets
                    .Where(x => x.ParentAssetId == null)
                    .ToList();

                var result = _mapper.Map<List<AssetResponseDto>>(parents);

                foreach (var dto in result)
                {
                    var children = assets
                        .Where(x => x.ParentAssetId == dto.Id)
                        .ToList();

                    dto.Children =
                        _mapper.Map<List<AssetResponseDto>>(children);
                }

                return Result<List<AssetResponseDto>>.Success(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching assets");

                return Result<List<AssetResponseDto>>.Failure(
                    ErrorMessages.UnexpectedError,
                    500);
            }
        }

        // ---------------- GET BY ID ----------------
        public async Task<Result<AssetResponseDto>> GetById(int id)
        {
            try
            {
                var asset =
                    await _unitOfWork.Assets.GetByIdWithChildrenAsync(id);

                if (asset == null)
                {
                    return Result<AssetResponseDto>.Failure(
                        ErrorMessages.AssetNotFound,
                        404);
                }

                var response = _mapper.Map<AssetResponseDto>(asset);

                var children = asset.ChildAssets
                    .Where(x => x.IsActive)
                    .ToList();

                response.Children =
                    _mapper.Map<List<AssetResponseDto>>(children);

                return Result<AssetResponseDto>.Success(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error fetching asset by id {AssetId}",
                    id);

                return Result<AssetResponseDto>.Failure(
                    ErrorMessages.UnexpectedError,
                    500);
            }
        }

        // ---------------- UPDATE ----------------
        public async Task<Result<string>> UpdateAssetAsync(
            UpdateAssetDto dto,
            int updatedBy)
        {
            try
            {
                var asset =
                    await _unitOfWork.Assets.GetByIdWithChildrenAsync(dto.Id);

                if (asset == null)
                {
                    return Result<string>.Failure(
                        ErrorMessages.AssetNotFound,
                        404);
                }

                _mapper.Map(dto, asset);

                asset.UpdatedAt = DateTime.UtcNow;
                asset.UpdatedBy = updatedBy;

                await _unitOfWork.SaveChangesAsync();

                return Result<string>.Success(
                    SuccessMessages.AssetUpdatedSuccessfully);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error updating asset {AssetId}",
                    dto.Id);

                return Result<string>.Failure(
                    ErrorMessages.UnexpectedError,
                    500);
            }
        }

        // ---------------- DELETE ----------------
        public async Task<Result<AssetResponseDto>> Delete(
            int id,
            int deletedBy)
        {
            try
            {
                var asset =
                    await _unitOfWork.Assets.GetByIdWithChildrenAsync(id);

                if (asset == null)
                {
                    return Result<AssetResponseDto>.Failure(
                        ErrorMessages.AssetNotFound,
                        404);
                }

                // detach children
                foreach (var child in asset.ChildAssets)
                {
                    child.ParentAssetId = null;
                }

                // soft delete
                asset.IsActive = false;
                asset.DeletedAt = DateTime.UtcNow;
                asset.DeletedBy = deletedBy;

                // history
                await _unitOfWork.Assets.AddHistoryAsync(
                    new AssetHistory
                    {
                        AssetId = asset.Id,
                        Action = "Deleted",
                        Description = $"Asset {asset.ItemName} deleted"
                    });

                await _unitOfWork.SaveChangesAsync();

                var response =
                    _mapper.Map<AssetResponseDto>(asset);

                response.Children =
                    _mapper.Map<List<AssetResponseDto>>(
                        asset.ChildAssets
                            .Where(x => x.IsActive)
                            .ToList());

                return Result<AssetResponseDto>.Success(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error deleting asset {AssetId}",
                    id);

                return Result<AssetResponseDto>.Failure(
                    ErrorMessages.UnexpectedError,
                    500);
            }
        }

        // ---------------- SUGGESTED USERS ----------------
        public async Task<Result<List<UserDto>>> GetSuggestedUsersAsync()
        {
            try
            {
                var users =
                    await _unitOfWork.Users.GetUsersWithOpenTicketsAsync();

                var result =
                    _mapper.Map<List<UserDto>>(users);

                return Result<List<UserDto>>.Success(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error fetching suggested users");

                return Result<List<UserDto>>.Failure(
                    ErrorMessages.UnexpectedError,
                    500);
            }
        }

        // ---------------- SEARCH USERS ----------------
        public async Task<Result<List<UserDto>>> SearchUsersAsync(
            string query)
        {
            try
            {
                var users =
                    await _unitOfWork.Users.SearchAsync(query);

                var result =
                    _mapper.Map<List<UserDto>>(users);

                return Result<List<UserDto>>.Success(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error searching users");

                return Result<List<UserDto>>.Failure(
                    ErrorMessages.UnexpectedError,
                    500);
            }
        }

        // ---------------- ADD ASSETS ASYNC ----------------
        public async Task<Result<string>> AddAssetsAsync(AddAssetDto dto, int createdBy)
        {
            try
            {
                _logger.LogInformation("Starting AddAssetsAsync with {AssetCount} assets", dto?.Assets?.Count ?? 0);
                
                if (dto?.Assets == null || !dto.Assets.Any())
                {
                    _logger.LogWarning("No assets provided in AddAssetsAsync request");
                    return Result<string>.Failure("No assets provided", 400);
                }

                var assets = new List<Asset>();
                var currentDateTime = DateTime.UtcNow;
                var processedCount = 0;

                foreach (var assetItem in dto.Assets)
                {
                    try
                    {
                        processedCount++;
                        _logger.LogInformation("Processing asset {Index}/{Total}: {ItemName}", processedCount, dto.Assets.Count, assetItem.ItemName);
                        
                        // Validate that ConditionId exists in AssetConditions table
                        var assetCondition = await _unitOfWork.Assets.GetAssetConditionByIdAsync(assetItem.ConditionId);
                        if (assetCondition == null)
                        {
                            _logger.LogError("Asset condition with ID {ConditionId} does not exist for asset {ItemName}", assetItem.ConditionId, assetItem.ItemName);
                            return Result<string>.Failure($"Asset condition with ID {assetItem.ConditionId} does not exist for asset {assetItem.ItemName}", 400);
                        }
                        
                        var asset = _mapper.Map<Asset>(assetItem);
                        asset.CreatedBy = createdBy;
                        asset.CreatedAt = currentDateTime;
                        asset.UpdatedAt = currentDateTime;
                        asset.IsActive = true;
                                               
                        // Set assignment details if provided
                        asset.AssignedTo = dto.AssignedTo;
                        asset.AssignDate = dto.AssignedDate ?? DateTime.UtcNow;
                        asset.ExpectedReturnDate = dto.ExpectedReturnDate;
                        asset.Notes = dto.Location; // Using Notes field to store location info
                        
                        assets.Add(asset);
                        
                        _logger.LogInformation("Asset {Index} mapped successfully: {AssetId}", processedCount, asset.ItemName);
                    }
                    catch (Exception mappingEx)
                    {
                        _logger.LogError(mappingEx, "Error mapping asset {Index}: {ItemName}", processedCount, assetItem.ItemName);
                        throw; // Re-throw to be caught by outer try-catch
                    }
                }

                _logger.LogInformation("Saving {AssetCount} assets to database", assets.Count);
                await _unitOfWork.Assets.AddRangeAsync(assets);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Successfully added {AssetCount} assets", assets.Count);
                return Result<string>.Success("Assets added successfully", "Assets created successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding assets: {Message} | StackTrace: {StackTrace}", ex.Message, ex.StackTrace);
                return Result<string>.Failure($"Error: {ex.Message}. Details: {ex.InnerException?.Message ?? "No inner exception"}", 500);
            }
        }

        // ---------------- GET ALL ASSETS ASYNC ----------------
        public async Task<Result<List<AssetResponseDto>>> GetAllAssetsAsync()
        {
            try
            {
                var assets = await _unitOfWork.Assets.GetAllAsync();
                var result = _mapper.Map<List<AssetResponseDto>>(assets);
                return Result<List<AssetResponseDto>>.Success(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all assets");
                return Result<List<AssetResponseDto>>.Failure(ErrorMessages.UnexpectedError, 500);
            }
        }

        // ---------------- UPDATE ASYNC ----------------
        public async Task<Result<AssetResponseDto>> Update(int id, CreateAssetDto dto, int updatedBy)
        {
            try
            {
                var asset = await _unitOfWork.Assets.GetByIdAsync(id);
                if (asset == null)
                {
                    return Result<AssetResponseDto>.Failure(ErrorMessages.AssetNotFound, 404);
                }

                _mapper.Map(dto, asset);
                asset.UpdatedAt = DateTime.UtcNow;
                asset.UpdatedBy = updatedBy;

                await _unitOfWork.SaveChangesAsync();

                var response = _mapper.Map<AssetResponseDto>(asset);
                return Result<AssetResponseDto>.Success(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating asset");
                return Result<AssetResponseDto>.Failure(ErrorMessages.UnexpectedError, 500);
            }
        }

        // ---------------- DELETE ASSET ASYNC ----------------
        public async Task<Result<string>> DeleteAssetAsync(int id, int deletedBy)
        {
            try
            {
                var asset = await _unitOfWork.Assets.GetByIdAsync(id);
                if (asset == null)
                {
                    return Result<string>.Failure(ErrorMessages.AssetNotFound, 404);
                }

                asset.IsActive = true;
                asset.DeletedAt = DateTime.UtcNow;
                asset.DeletedBy = deletedBy;

                await _unitOfWork.SaveChangesAsync();
                return Result<string>.Success("Asset deleted successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting asset");
                return Result<string>.Failure(ErrorMessages.UnexpectedError, 500);
            }
        }

        
        // ---------------- GET ASSET BY ID ASYNC ----------------
        public async Task<Result<GetAssetByIdResponseDto>> GetAssetByIdAsync(int id)
        {
            try
            {
                var asset = await _unitOfWork.Assets.GetByIdAsync(id);
                if (asset == null)
                {
                    return Result<GetAssetByIdResponseDto>.Failure(ErrorMessages.AssetNotFound, 404);
                }

                var result = _mapper.Map<GetAssetByIdResponseDto>(asset);
                return Result<GetAssetByIdResponseDto>.Success(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting asset by id");
                return Result<GetAssetByIdResponseDto>.Failure(ErrorMessages.UnexpectedError, 500);
            }
        }

        // ---------------- ASSIGN ASSET ASYNC ----------------
        public async Task<Result<string>> AssignAssetAsync(AssignAssetDto dto)
        {
            try
            {
                _logger.LogInformation("Starting asset assignment for AssetId: {AssetId}, UserId: {UserId}", dto.AssetId, dto.UserId);

                if (dto.AssetId <= 0)
                {
                    return Result<string>.Failure("Invalid asset ID", 400);
                }

                if (dto.UserId <= 0)
                {
                    return Result<string>.Failure("Invalid user ID for assignment", 400);
                }

                var asset = await _unitOfWork.Assets.GetByIdAsync(dto.AssetId);
                if (asset == null)
                {
                    return Result<string>.Failure("Asset not found", 404);
                }

                var user = await _unitOfWork.Users.GetByIdAsync(dto.UserId);
                if (user == null)
                {
                    return Result<string>.Failure("User not found", 404);
                }

                // Update asset assignment
                asset.AssignedTo = dto.UserId;
                asset.AssignDate = dto.AssignedDate;
                asset.UpdatedAt = DateTime.UtcNow;
                asset.UpdatedBy = dto.UserId;

                await _unitOfWork.Assets.Update(asset);
                await _unitOfWork.SaveChangesAsync();

                // Create assignment history
                await _unitOfWork.Assets.AddHistoryAsync(
                    new AssetHistory
                    {
                        AssetId = asset.Id,
                        Action = "Assigned",
                        Description = $"Asset {asset.ItemName} assigned to user {user.FullName}"
                    });

                _logger.LogInformation("Asset {AssetId} successfully assigned to user {UserId}", dto.AssetId, dto.UserId);
                
                return Result<string>.Success("Asset assigned successfully", "Asset assignment completed");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error assigning asset {AssetId} to user {UserId}", dto.AssetId, dto.UserId);
                return Result<string>.Failure(ErrorMessages.UnexpectedError, 500);
            }
        }

        // ---------------- ATTACH CHILD ASYNC ----------------
        public async Task<Result<string>> AttachChildAsync(AttachChildDto dto)
        {
            try
            {
                _logger.LogInformation("Starting AttachChildAsync for parent {ParentId} and child {ChildId}", dto.ParentId, dto.ChildId);

                // Validate parent asset exists
                var parentAsset = await _unitOfWork.Assets.GetByIdAsync(dto.ParentId);
                if (parentAsset == null)
                {
                    _logger.LogWarning("Parent asset not found: {ParentId}", dto.ParentId);
                    return Result<string>.Failure("Parent asset not found", 404);
                }

                // Validate child asset exists
                var childAsset = await _unitOfWork.Assets.GetByIdAsync(dto.ChildId);
                if (childAsset == null)
                {
                    _logger.LogWarning("Child asset not found: {ChildId}", dto.ChildId);
                    return Result<string>.Failure("Child asset not found", 404);
                }

                // Check if child is already attached to a parent
                if (childAsset.ParentAssetId != null)
                {
                    _logger.LogWarning("Child asset {ChildId} is already attached to parent {CurrentParentId}", 
                        dto.ChildId, childAsset.ParentAssetId);
                    return Result<string>.Failure("Child asset is already attached to a parent", 400);
                }

                // Check if trying to attach asset to itself
                if (dto.ParentId == dto.ChildId)
                {
                    _logger.LogWarning("Cannot attach asset to itself: {AssetId}", dto.ParentId);
                    return Result<string>.Failure("Cannot attach asset to itself", 400);
                }

                // Attach child to parent
                childAsset.ParentAssetId = dto.ParentId;
                childAsset.UpdatedAt = DateTime.UtcNow;

                await _unitOfWork.Assets.Update(childAsset);
                await _unitOfWork.SaveChangesAsync();

                // Create history record for parent
                await _unitOfWork.Assets.AddHistoryAsync(
                    new AssetHistory
                    {
                        AssetId = parentAsset.Id,
                        Action = "Child Attached",
                        Description = $"Child asset {childAsset.ItemName} attached to this asset"
                    });

                // Create history record for child
                await _unitOfWork.Assets.AddHistoryAsync(
                    new AssetHistory
                    {
                        AssetId = childAsset.Id,
                        Action = "Attached to Parent",
                        Description = $"Attached to parent asset {parentAsset.ItemName}"
                    });

                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Successfully attached child asset {ChildId} to parent {ParentId}", 
                    dto.ChildId, dto.ParentId);

                return Result<string>.Success($"Child asset attached successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error attaching child asset {ChildId} to parent {ParentId}", dto.ChildId, dto.ParentId);
                return Result<string>.Failure(ErrorMessages.UnexpectedError, 500);
            }
        }

        // ---------------- CREATE AND ATTACH CHILD ASYNC ----------------
        public async Task<Result<string>> CreateAndAttachChildAsync(CreateChildAssetDto dto)
        {
            try
            {
                _logger.LogInformation("Starting CreateAndAttachChildAsync for parent {ParentId}", dto.ParentId);

                // Validate parent asset exists and can have children
                var parentAsset = await _unitOfWork.Assets.GetByIdAsync(dto.ParentId);
                if (parentAsset == null)
                {
                    _logger.LogWarning("Parent asset not found: {ParentId}", dto.ParentId);
                    return Result<string>.Failure("Parent asset not found", 404);
                }

                // Check if serial number already exists
                if (await _unitOfWork.Assets.SerialExistsAsync(dto.SerialNo))
                {
                    _logger.LogWarning("Serial number already exists: {SerialNo}", dto.SerialNo);
                    return Result<string>.Failure("Serial number already exists", 400);
                }

                // Create new child asset
                var childAsset = new Asset
                {
                    ItemName = dto.ItemName,
                    StatusId = dto.StatusId,
                    CategoryId = dto.CategoryId,
                    SubCategoryId = dto.SubCategoryId,
                    ConditionId = dto.ConditionId,
                    Brand = dto.Brand,
                    Model = dto.Model,
                    SerialNo = dto.SerialNo,
                    Vendor = dto.Vendor,
                    PurchaseCost = dto.PurchaseCost,
                    PurchaseDate = dto.PurchaseDate,
                    InvoiceNumber = dto.InvoiceNumber,
                    ParentAssetId = dto.ParentId, // Link to parent
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    IsActive = false
                };

                // Add the child asset
                await _unitOfWork.Assets.Add(childAsset);
                await _unitOfWork.SaveChangesAsync();

                // Create history record
                await _unitOfWork.Assets.AddHistoryAsync(
                    new AssetHistory
                    {
                        AssetId = childAsset.Id,
                        Action = "Created and Attached",
                        Description = $"Child asset {childAsset.ItemName} created and attached to parent {parentAsset.ItemName}"
                    });

                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Successfully created and attached child asset {ChildId} to parent {ParentId}", 
                    childAsset.Id, dto.ParentId);

                return Result<string>.Success($"Child asset created and attached successfully with ID: {childAsset.Id}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating and attaching child asset for parent {ParentId}", dto.ParentId);
                return Result<string>.Failure(ErrorMessages.UnexpectedError, 500);
            }
        }

        // ---------------- DETACH CHILD ASYNC ----------------
        public async Task<Result<string>> DetachChildAsync(DetachChildDto dto)
        {
            try
            {
                _logger.LogInformation("Starting DetachChildAsync for parent {ParentId} and child {ChildId}", dto.ParentId, dto.ChildId);

                // Validate child asset exists
                var childAsset = await _unitOfWork.Assets.GetByIdAsync(dto.ChildId);
                if (childAsset == null)
                {
                    _logger.LogWarning("Child asset not found: {ChildId}", dto.ChildId);
                    return Result<string>.Failure("Child asset not found", 404);
                }

                // Validate parent asset exists
                var parentAsset = await _unitOfWork.Assets.GetByIdAsync(dto.ParentId);
                if (parentAsset == null)
                {
                    _logger.LogWarning("Parent asset not found: {ParentId}", dto.ParentId);
                    return Result<string>.Failure("Parent asset not found", 404);
                }

                // Check if child is actually attached to the specified parent
                if (childAsset.ParentAssetId != dto.ParentId)
                {
                    _logger.LogWarning("Child asset {ChildId} is not attached to parent {ParentId}. Current parent: {CurrentParentId}", 
                        dto.ChildId, dto.ParentId, childAsset.ParentAssetId);
                    return Result<string>.Failure("Child asset is not attached to the specified parent", 400);
                }

                // Detach child from parent
                childAsset.ParentAssetId = null;
                childAsset.UpdatedAt = DateTime.UtcNow;

                await _unitOfWork.Assets.Update(childAsset);
                await _unitOfWork.SaveChangesAsync();

                // Create history record for parent
                await _unitOfWork.Assets.AddHistoryAsync(
                    new AssetHistory
                    {
                        AssetId = parentAsset.Id,
                        Action = "Child Detached",
                        Description = $"Child asset {childAsset.ItemName} detached from this asset"
                    });

                // Create history record for child
                await _unitOfWork.Assets.AddHistoryAsync(
                    new AssetHistory
                    {
                        AssetId = childAsset.Id,
                        Action = "Detached from Parent",
                        Description = $"Detached from parent asset {parentAsset.ItemName}"
                    });

                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Successfully detached child asset {ChildId} from parent {ParentId}", 
                    dto.ChildId, dto.ParentId);

                return Result<string>.Success($"Child asset detached successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error detaching child asset {ChildId} from parent {ParentId}", dto.ChildId, dto.ParentId);
                return Result<string>.Failure(ErrorMessages.UnexpectedError, 500);
            }
        }

        // ---------------- FILTER ASSETS ASYNC ----------------
        public async Task<Result<List<AssetListDto>>> FilterAssetsAsync(AssetFilterDto dto)
        {
            try
            {
                // TODO: Implement when AssetFilterDto is available
                throw new NotImplementedException("FilterAssetsAsync not implemented yet");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error filtering assets");
                return Result<List<AssetListDto>>.Failure(ErrorMessages.UnexpectedError, 500);
            }
        }

        // ---------------- ADD OR UPDATE NETWORK ASYNC ----------------
        public async Task<Result<string>> AddOrUpdateNetworkAsync(int assetId, NetworkDetailsDto dto, int userId)
        {
            try
            {
                _logger.LogInformation("Starting AddOrUpdateNetworkAsync for asset {AssetId}", assetId);

                // Validate asset exists
                var asset = await _unitOfWork.Assets.GetByIdAsync(assetId);
                if (asset == null)
                {
                    _logger.LogWarning("Asset not found: {AssetId}", assetId);
                    return Result<string>.Failure("Asset not found", 404);
                }

                // Get existing network details for the asset
                var existingNetworkDetail = await _unitOfWork.NetworkDetails.GetByAssetIdAsync(assetId);

                if (existingNetworkDetail != null)
                {
                    // Update existing network details
                    _mapper.Map(dto, existingNetworkDetail);
                    existingNetworkDetail.updatedBy = userId.ToString();
                    _unitOfWork.NetworkDetails.Update(existingNetworkDetail);
                    
                    _logger.LogInformation("Updating existing network details for asset {AssetId}", assetId);
                }
                else
                {
                    // Create new network details
                    var newNetworkDetail = _mapper.Map<NetworkDetail>(dto);
                    newNetworkDetail.AssetId = assetId;
                    newNetworkDetail.createdBy = userId.ToString();
                    
                    await _unitOfWork.NetworkDetails.AddAsync(newNetworkDetail);
                    
                    _logger.LogInformation("Creating new network details for asset {AssetId}", assetId);
                }

                await _unitOfWork.SaveChangesAsync();

                // Create history record
                await _unitOfWork.Assets.AddHistoryAsync(
                    new AssetHistory
                    {
                        AssetId = assetId,
                        Action = "Network Details Updated",
                        Description = $"Network details updated for asset {asset.ItemName}"
                    });

                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Successfully updated network details for asset {AssetId}", assetId);

                return Result<string>.Success("Network details updated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding or updating network details for asset {AssetId}", assetId);
                return Result<string>.Failure(ErrorMessages.UnexpectedError, 500);
            }
        }
    }
}