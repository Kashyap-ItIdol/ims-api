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
                : Result<string>.Failure(ErrorMessages.AssetNotFound, 400);
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

        public async Task<Result<byte[]>> ExportAllAssetsCsvAsync()
        {
            try
            {
                var assetsResult = await GetAllAssetsAsync();
                if (!assetsResult.IsSuccess)
                    return Result<byte[]>.Failure(ErrorMessages.UnexpectedError, assetsResult.StatusCode);

                var assets = assetsResult.Data ?? new List<AssetResponseDto>();

                var sb = new System.Text.StringBuilder();
                sb.AppendLine("Item Name,Status,Category,Subcategory,Brand,Model,Serial Number,Condition,Vendor Name,Purchase Cost,Purchase Date,Invoice Number,Warranty Expiry Date,AMC Expiry Date");

                foreach (var a in assets)
                {
                    sb.AppendLine(string.Join(",",
                        EscapeCsv(a.ItemName),
                        EscapeCsv(a.StatusId.ToString()),
                        EscapeCsv(a.CategoryId.ToString()),
                        EscapeCsv(a.SubCategoryId.ToString()),
                        EscapeCsv(a.Brand),
                        EscapeCsv(a.Model),
                        EscapeCsv(a.SerialNo),
                        EscapeCsv(a.ConditionId.ToString()),
                        EscapeCsv(a.Vendor),
                        EscapeCsv(a.PurchaseCost.ToString()),
                        EscapeCsv(a.PurchaseDate?.ToString("yyyy-MM-dd") ?? string.Empty),
                        EscapeCsv(a.InvoiceNumber ?? string.Empty),
                        EscapeCsv(a.WarrantyExpiry?.ToString("yyyy-MM-dd") ?? string.Empty),
                        EscapeCsv(a.AmcExpiry?.ToString("yyyy-MM-dd") ?? string.Empty)
                    ));
                }

                var bytes = System.Text.Encoding.UTF8.GetBytes(sb.ToString());
                return Result<byte[]>.Success(bytes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting assets to CSV");
                return Result<byte[]>.Failure(ErrorMessages.UnexpectedError, 500);
            }
        }

        public async Task<Result<ImportAssetsResultDto>> ImportAssetsCsvAsync(ImportAssetsRequestDto dto, int createdBy)
        {

            try
            {
                var result = new ImportAssetsResultDto();

                using var stream = dto.CsvFile.OpenReadStream();
                using var reader = new System.IO.StreamReader(stream);
                var csvText = await reader.ReadToEndAsync();
                if (string.IsNullOrWhiteSpace(csvText))
                {
                    return Result<ImportAssetsResultDto>.Success(result);
                }

                List<string[]> rows;
                try
                {
                    rows = ParseCsv(csvText);
                }
                catch (Exception parseEx)
                {
                    _logger.LogError(parseEx, "Failed to parse import CSV");
                    return Result<ImportAssetsResultDto>.Failure(ErrorMessages.UnexpectedError, 400);
                }

                if (rows.Count < 2)
                    return Result<ImportAssetsResultDto>.Success(result);

                var header = rows[0];
                var headerMap = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
                for (var i = 0; i < header.Length; i++)
                {
                    if (!string.IsNullOrWhiteSpace(header[i]))
                        headerMap[header[i].Trim()] = i;
                }

                static string NormalizeHeader(string value)
                {
                    if (value == null) return string.Empty;

                    var trimmed = value.Trim();
                    var noSpaces = System.Text.RegularExpressions.Regex.Replace(trimmed, @"\s+", "");
                    var noSeparators = noSpaces.Replace("_", "", StringComparison.OrdinalIgnoreCase)
                                             .Replace("-", "");
                    return noSeparators;
                }


                var normalizedHeaderMap = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
                for (var i = 0; i < header.Length; i++)
                {
                    if (!string.IsNullOrWhiteSpace(header[i]))

                    {
                        var key = NormalizeHeader(header[i]);
                        if (!string.IsNullOrWhiteSpace(key))
                            normalizedHeaderMap[key] = i;
                    }
                }

                int GetIndex(string expectedHeader)
                {
                    var directKey = NormalizeHeader(expectedHeader);
                    if (normalizedHeaderMap.TryGetValue(directKey, out var directIdx))
                        return directIdx;

                    return -1;
                }

                int idxItemName = GetIndex("Item Name");
                int idxStatus = GetIndex("Status");
                int idxCategory = GetIndex("Category");
                int idxSubCategory = GetIndex("Subcategory");
                int idxBrand = GetIndex("Brand");
                int idxModel = GetIndex("Model");
                int idxSerialNo = GetIndex("Serial Number");
                int idxCondition = GetIndex("Condition");
                int idxVendor = GetIndex("Vendor Name");
                int idxPurchaseCost = GetIndex("Purchase Cost");
                int idxPurchaseDate = GetIndex("Purchase Date");
                int idxInvoiceNumber = GetIndex("Invoice Number");
                int idxWarrantyExpiry = GetIndex("Warranty Expiry Date");
                int idxAmcExpiry = GetIndex("AMC Expiry Date");

                int idxAssignedTo = GetIndex("AssignedTo");
                int idxAssignedDate = GetIndex("AssignedDate");
                int idxNotes = GetIndex("Notes");

                if (idxItemName < 0 || idxSerialNo < 0 || idxStatus < 0 || idxCategory < 0 || idxSubCategory < 0 || idxBrand < 0 || idxModel < 0 || idxCondition < 0 || idxVendor < 0 || idxPurchaseCost < 0 || idxPurchaseDate < 0 || idxInvoiceNumber < 0 || idxWarrantyExpiry < 0 || idxAmcExpiry < 0)
                    return Result<ImportAssetsResultDto>.Failure("CSV header does not match expected format.", 400);


                result.TotalRows = rows.Count - 1;
                var now = DateTime.UtcNow;
                var assetsToInsert = new List<Asset>();

                for (var rowNumber = 1; rowNumber < rows.Count; rowNumber++)
                {
                    var row = rows[rowNumber];
                    string Get(string col, int idx)
                    {
                        if (idx < 0) return string.Empty;
                        if (idx >= row.Length) return string.Empty;
                        return row[idx]?.Trim() ?? string.Empty;
                    }

                    string itemName = Get("Item Name", idxItemName);
                    string serialNo = Get("Serial Number", idxSerialNo);
                    string categoryStr = Get("Category", idxCategory);
                    string subCategoryStr = Get("Subcategory", idxSubCategory);
                    string statusStr = Get("Status", idxStatus);
                    string brand = Get("Brand", idxBrand);
                    string model = Get("Model", idxModel);
                    string vendor = Get("Vendor Name", idxVendor);
                    string conditionStr = Get("Condition", idxCondition);
                    string purchaseCostStr = Get("Purchase Cost", idxPurchaseCost);
                    string purchaseDateStr = Get("Purchase Date", idxPurchaseDate);
                    string invoiceNumber = Get("Invoice Number", idxInvoiceNumber);

                    var assignedToId = (int?)null;
                    var notes = (string?)null;
                    var assignedDateStr = (string?)null;

                    if (string.IsNullOrWhiteSpace(itemName) || string.IsNullOrWhiteSpace(serialNo))
                    {
                        result.Skipped++;
                        result.Errors.Add(new ImportAssetRowErrorDto { RowNumber = rowNumber, Message = "ItemName and SerialNo are required." });
                        continue;
                    }

                    if (!int.TryParse(categoryStr, out var categoryId) || categoryId <= 0)
                    {
                        result.Skipped++;
                        result.Errors.Add(new ImportAssetRowErrorDto { RowNumber = rowNumber, Message = "Category must be a valid numeric CategoryId." });
                        continue;
                    }

                    if (!int.TryParse(subCategoryStr, out var subCategoryId) || subCategoryId <= 0)
                    {
                        result.Skipped++;
                        result.Errors.Add(new ImportAssetRowErrorDto { RowNumber = rowNumber, Message = "SubCategory must be a valid numeric SubCategoryId." });
                        continue;
                    }

                    if (!int.TryParse(statusStr, out var statusId) || statusId <= 0)
                    {
                        result.Skipped++;
                        result.Errors.Add(new ImportAssetRowErrorDto { RowNumber = rowNumber, Message = "Status must be a valid numeric StatusId." });
                        continue;
                    }

                    if (string.IsNullOrWhiteSpace(brand) || string.IsNullOrWhiteSpace(model) || string.IsNullOrWhiteSpace(vendor))
                    {
                        result.Skipped++;
                        result.Errors.Add(new ImportAssetRowErrorDto { RowNumber = rowNumber, Message = "Brand, Model and Vendor are required." });
                        continue;
                    }

                    if (string.IsNullOrWhiteSpace(conditionStr) || !int.TryParse(conditionStr, out var conditionId))
                    {
                        result.Failed++;
                        result.Errors.Add(new ImportAssetRowErrorDto { RowNumber = rowNumber, Message = "ConditionId is required and must be numeric (expected column: Condition)." });
                        continue;
                    }

                    if (string.IsNullOrWhiteSpace(purchaseCostStr))
                    {
                        result.Failed++;
                        result.Errors.Add(new ImportAssetRowErrorDto { RowNumber = rowNumber, Message = "PurchaseCost is required (expected column: PurchaseCost)." });
                        continue;
                    }

                    if (!decimal.TryParse(
                            purchaseCostStr,
                            System.Globalization.NumberStyles.Any,
                            System.Globalization.CultureInfo.InvariantCulture,
                            out var purchaseCost))
                    {
                        if (!decimal.TryParse(purchaseCostStr, out purchaseCost))
                        {
                            result.Failed++;
                            result.Errors.Add(new ImportAssetRowErrorDto { RowNumber = rowNumber, Message = "PurchaseCost is required and must be decimal (expected column: PurchaseCost)." });
                            continue;
                        }
                    }

                    if (string.IsNullOrWhiteSpace(purchaseDateStr))
                    {
                        result.Failed++;
                        result.Errors.Add(new ImportAssetRowErrorDto { RowNumber = rowNumber, Message = "PurchaseDate is required (expected column: PurchaseDate)." });
                        continue;
                    }

                    if (!DateTime.TryParse(purchaseDateStr.Trim(), System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out var purchaseDate))
                    {
                        if (!DateTime.TryParse(purchaseDateStr.Trim(), out purchaseDate))
                        {
                            result.Failed++;
                            result.Errors.Add(new ImportAssetRowErrorDto { RowNumber = rowNumber, Message = "PurchaseDate is required and must be a valid date (expected column: PurchaseDate)." });
                            continue;
                        }
                    }

                    if (string.IsNullOrWhiteSpace(invoiceNumber))
                    {
                        result.Failed++;
                        result.Errors.Add(new ImportAssetRowErrorDto { RowNumber = rowNumber, Message = "InvoiceNumber is required (expected column: InvoiceNumber)." });
                        continue;
                    }

                    if (await _unitOfWork.Assets.SerialExistsAsync(serialNo))
                    {
                        result.Skipped++;
                        result.Errors.Add(new ImportAssetRowErrorDto { RowNumber = rowNumber, Message = $"SerialNo '{serialNo}' already exists." });
                        continue;
                    }

                    if (await _unitOfWork.Categories.GetByIdAsync(categoryId) == null)
                    {
                        result.Skipped++;
                        result.Errors.Add(new ImportAssetRowErrorDto { RowNumber = rowNumber, Message = $"CategoryId {categoryId} does not exist." });
                        continue;
                    }

                    if (await _unitOfWork.SubCategories.GetByIdAsync(subCategoryId) == null)
                    {
                        result.Skipped++;
                        result.Errors.Add(new ImportAssetRowErrorDto { RowNumber = rowNumber, Message = $"SubCategoryId {subCategoryId} does not exist." });
                        continue;
                    }

                    if (await _unitOfWork.Assets.GetAssetStatusByIdAsync(statusId) == null)
                    {
                        result.Skipped++;
                        result.Errors.Add(new ImportAssetRowErrorDto { RowNumber = rowNumber, Message = $"StatusId {statusId} does not exist." });
                        continue;
                    }

                    if (await _unitOfWork.Assets.GetAssetConditionByIdAsync(conditionId) == null)
                    {
                        result.Skipped++;
                        result.Errors.Add(new ImportAssetRowErrorDto { RowNumber = rowNumber, Message = $"ConditionId {conditionId} does not exist." });
                        continue;
                    }

                    if (false)
                    {
                    }

                    var asset = new Asset
                    {
                        ItemName = itemName,
                        SerialNo = serialNo,
                        CategoryId = categoryId,
                        SubCategoryId = subCategoryId,
                        StatusId = statusId,
                        ConditionId = conditionId,
                        Brand = brand,
                        Model = model,
                        Vendor = vendor,
                        Notes = string.IsNullOrWhiteSpace(notes) ? null : notes,
                        PurchaseCost = purchaseCost,
                        PurchaseDate = purchaseDate,
                        InvoiceNumber = invoiceNumber,
                        WarrantyExpiry = TryParseNullableDate(Get("Warranty Expiry Date", idxWarrantyExpiry)),
                        AmcExpiry = TryParseNullableDate(Get("AMC Expiry Date", idxAmcExpiry)),
                        IsActive = true,
                        CreatedAt = now,
                        UpdatedAt = now,
                        CreatedBy = createdBy,
                        UpdatedBy = createdBy,
                        AssignedTo = assignedToId,
                        AssignDate = ParseNullableDate(assignedDateStr),
                        ExpectedReturnDate = null
                    };


                    assetsToInsert.Add(asset);
                }

                if (assetsToInsert.Any())
                {
                    try
                    {
                        await _unitOfWork.Assets.AddRangeAsync(assetsToInsert);
                        await _unitOfWork.SaveChangesAsync();

                        foreach (var asset in assetsToInsert)
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
                        }

                        await _unitOfWork.SaveChangesAsync();
                    }
                    catch (Exception dbEx)
                    {
                        _logger.LogError(dbEx, "Error saving imported assets CSV");

                        result.Failed += assetsToInsert.Count;
                        result.Errors.Add(new ImportAssetRowErrorDto
                        {
                            RowNumber = 0,
                            Message = $"Database error while inserting assets: {dbEx.InnerException?.Message ?? dbEx.Message}"
                        });

                        result.Inserted = 0;
                        return Result<ImportAssetsResultDto>.Success(result);
                    }
                }

                result.Inserted = assetsToInsert.Count;
                return Result<ImportAssetsResultDto>.Success(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error importing assets CSV");
                return Result<ImportAssetsResultDto>.Failure(ErrorMessages.UnexpectedError, 500);
            }

            static int? ParseNullableInt(string value)
            {
                if (string.IsNullOrWhiteSpace(value)) return null;
                return int.TryParse(value.Trim(), out var i) ? i : null;
            }

            static DateTime? ParseNullableDate(string value)
            {
                if (string.IsNullOrWhiteSpace(value)) return null;
                if (DateTime.TryParse(value.Trim(), System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out var dt))
                    return dt;
                return DateTime.TryParse(value.Trim(), out dt) ? dt : null;
            }

            static DateTime? TryParseNullableDate(string value)
            {
                if (string.IsNullOrWhiteSpace(value)) return null;
                if (DateTime.TryParse(value.Trim(), System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out var dt))
                    return dt;
                return DateTime.TryParse(value.Trim(), out dt) ? dt : null;
            }
        }

        private static List<string[]> ParseCsv(string csvText)
        {
            var rows = new List<string[]>();
            var row = new List<string>();
            var current = new System.Text.StringBuilder();
            bool inQuotes = false;

            for (int i = 0; i < csvText.Length; i++)
            {
                char c = csvText[i];

                if (c == '"')
                {
                    if (inQuotes && i + 1 < csvText.Length && csvText[i + 1] == '"')
                    {
                        current.Append('"');
                        i++;
                        continue;
                    }

                    inQuotes = !inQuotes;
                    continue;
                }

                if (!inQuotes)
                {
                    if (c == ',')
                    {
                        row.Add(current.ToString());
                        current.Clear();
                        continue;
                    }

                    if (c == '\r')
                        continue;

                    if (c == '\n')
                    {
                        row.Add(current.ToString());
                        current.Clear();
                        rows.Add(row.ToArray());
                        row = new List<string>();
                        continue;
                    }
                }

                current.Append(c);
            }

            if (current.Length > 0 || row.Count > 0)
            {
                row.Add(current.ToString());
                rows.Add(row.ToArray());
            }

            return rows;
        }

        private static string EscapeCsv(string? value)
        {
            value ??= string.Empty;
            var needsQuotes = value.Contains(',') || value.Contains('"') || value.Contains('\n') || value.Contains('\r');
            value = value.Replace('"', '"');
            return needsQuotes ? $"\"{value}\"" : value;
        }
    }
}
