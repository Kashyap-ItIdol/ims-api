using AutoMapper;
using IMS_Application.Common.Models;
using IMS_Application.DTOs;
using IMS_Application.Interfaces;
using IMS_Application.Services.Interfaces;
using IMS_Domain.Entities;
using AutoMapper;


namespace IMS_Application.Services
{
    public class AssetService : IAssetService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public AssetService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Result<string>> AddAssetsAsync(AddAssetDto dto, bool isClient)
        {

            if (dto.Assets == null || !dto.Assets.Any())
                return Result<string>.Failure("At least one asset is required", 400);

            if (dto.Assets.Count(x => x.IsPrimary) != 1)
                return Result<string>.Failure("Exactly one primary asset is required", 400);

            foreach (var asset in dto.Assets)
            {
                if (string.IsNullOrWhiteSpace(asset.ItemName))
                    return Result<string>.Failure("ItemName is required", 400);

                if (asset.CategoryId <= 0)
                    return Result<string>.Failure("Invalid CategoryId", 400);

                if (asset.SubCategoryId <= 0)
                    return Result<string>.Failure("Invalid SubCategoryId", 400);

                if (asset.ConditionId <= 0)
                    return Result<string>.Failure("Invalid ConditionId", 400);

                if (string.IsNullOrWhiteSpace(asset.SerialNo))
                    return Result<string>.Failure("Serial number is required", 400);


                if (!asset.IsPurchaseDetailsSame)
                {
                    if (string.IsNullOrWhiteSpace(asset.Vendor))
                        return Result<string>.Failure("Vendor is required", 400);

                    if (!asset.PurchaseCost.HasValue || asset.PurchaseCost <= 0)
                        return Result<string>.Failure("PurchaseCost must be greater than 0", 400);

                    if (!asset.PurchaseDate.HasValue)
                        return Result<string>.Failure("PurchaseDate is required", 400);
                }
            }


            if (dto.AssignedTo.HasValue)
            {
                var userExists = await _unitOfWork.Users.ExistsAsync(dto.AssignedTo.Value);

                if (!userExists)
                    return Result<string>.Failure($"User not found with id {dto.AssignedTo.Value}", 404);

                var user = await _unitOfWork.Users.GetByIdAsync(dto.AssignedTo.Value);


                if (!string.IsNullOrEmpty(dto.Location))
                    user.Location = dto.Location;

                if (!string.IsNullOrEmpty(dto.TableNo))
                    user.TableNo = dto.TableNo;
            }


            if (!string.IsNullOrEmpty(dto.TableNo))
            {
                var isTableUsed = await _unitOfWork.Users.TableAlreadyAssignedAsync(dto.TableNo);

                if (isTableUsed)
                    return Result<string>.Failure($"Table {dto.TableNo} is already assigned to another user", 400);
            }


            if (dto.AssignedDate.HasValue && dto.ExpectedReturnDate.HasValue)
            {
                if (dto.ExpectedReturnDate < dto.AssignedDate)
                    return Result<string>.Failure("ExpectedReturnDate cannot be before AssignedDate", 400);
            }

            var serials = dto.Assets.Select(x => x.SerialNo).ToList();

            foreach (var serial in serials)
            {
                if (await _unitOfWork.Assets.SerialExistsAsync(serial))
                    return Result<string>.Failure($"Serial already exists: {serial}", 400);
            }



            var mainDto = dto.Assets.First(x => x.IsPrimary);


            var mainAsset = new Asset
            {
                Image = mainDto.Image,
                ItemName = mainDto.ItemName,
                StatusId = mainDto.StatusId,
                CategoryId = mainDto.CategoryId,
                SubCategoryId = mainDto.SubCategoryId,
                ConditionId = mainDto.ConditionId,
                Brand = mainDto.Brand,
                Model = mainDto.Model,
                SerialNo = mainDto.SerialNo,

                Vendor = mainDto.Vendor!,
                PurchaseCost = mainDto.PurchaseCost!.Value,
                PurchaseDate = mainDto.PurchaseDate!.Value,
                InvoiceNumber = mainDto.InvoiceNumber!,

                WarrantyExpiry = mainDto.WarrantyExpiry,
                AmcExpiry = mainDto.AmcExpiry,

                IsClient = isClient,

                AssignedTo = dto.AssignedTo,
                AssignDate = dto.AssignedDate ?? DateTime.UtcNow,
                ExpectedReturnDate = dto.ExpectedReturnDate
            };


            await _unitOfWork.Assets.AddRangeAsync(new List<Asset> { mainAsset });
            await _unitOfWork.SaveChangesAsync();


            var childAssets = new List<Asset>();

            foreach (var item in dto.Assets.Where(x => !x.IsPrimary))
            {
                var asset = new Asset
                {
                    Image = item.Image,
                    ItemName = item.ItemName,
                    StatusId = item.StatusId,
                    CategoryId = item.CategoryId,
                    SubCategoryId = item.SubCategoryId,
                    ConditionId = item.ConditionId,
                    Brand = item.Brand,
                    Model = item.Model,
                    SerialNo = item.SerialNo,


                    Vendor = item.IsPurchaseDetailsSame ? mainAsset.Vendor : item.Vendor!,
                    PurchaseCost = item.IsPurchaseDetailsSame ? mainAsset.PurchaseCost : item.PurchaseCost!.Value,
                    PurchaseDate = item.IsPurchaseDetailsSame ? mainAsset.PurchaseDate : item.PurchaseDate!.Value,
                    InvoiceNumber = item.IsPurchaseDetailsSame ? mainAsset.InvoiceNumber : item.InvoiceNumber!,

                    WarrantyExpiry = item.WarrantyExpiry,
                    AmcExpiry = item.AmcExpiry,

                    IsClient = isClient,

                    AssignedTo = dto.AssignedTo,
                    AssignDate = dto.AssignedDate ?? DateTime.UtcNow,
                    ExpectedReturnDate = dto.ExpectedReturnDate,


                    ParentAssetId = mainAsset.Id
                };

                childAssets.Add(asset);
            }


            if (childAssets.Any())
            {
                await _unitOfWork.Assets.AddRangeAsync(childAssets);
                await _unitOfWork.SaveChangesAsync();
            }
            return Result<string>.Success("Assets added successfully");
        }

        public async Task<Result<List<AssetResponseDto>>> GetAllAssetsAsync()
        {
            var assets = await _unitOfWork.Assets.GetAllAsync();


            var parentAssets = assets.Where(a => a.ParentAssetId == null).ToList();

            var result = parentAssets.Select(parent => new AssetResponseDto
            {
                Image = parent.Image,
                ItemName = parent.ItemName,
                StatusId = parent.StatusId,
                CategoryId = parent.CategoryId,
                SubCategoryId = parent.SubCategoryId,
                ConditionId = parent.ConditionId,
                Brand = parent.Brand,
                Model = parent.Model,
                SerialNo = parent.SerialNo,
                Vendor = parent.Vendor,
                PurchaseCost = parent.PurchaseCost,
                PurchaseDate = parent.PurchaseDate,
                InvoiceNumber = parent.InvoiceNumber,
                WarrantyExpiry = parent.WarrantyExpiry,
                AmcExpiry = parent.AmcExpiry,
                AssignedTo = parent.AssignedTo,
                AssignedDate = parent.AssignDate,
                ExpectedReturnDate = parent.ExpectedReturnDate,

                Location = parent.AssignedUser?.Location,
                TableNo = parent.AssignedUser?.TableNo,


                Children = assets
                    .Where(x => x.ParentAssetId == parent.Id)
                    .Select(child => new AssetResponseDto
                    {
                        Image = child.Image,
                        ItemName = child.ItemName,
                        StatusId = child.StatusId,
                        CategoryId = child.CategoryId,
                        SubCategoryId = child.SubCategoryId,
                        ConditionId = child.ConditionId,
                        Brand = child.Brand,
                        Model = child.Model,
                        SerialNo = child.SerialNo,
                        Vendor = child.Vendor,
                        PurchaseCost = child.PurchaseCost,
                        PurchaseDate = child.PurchaseDate,
                        InvoiceNumber = child.InvoiceNumber,
                        WarrantyExpiry = child.WarrantyExpiry,
                        AmcExpiry = child.AmcExpiry,
                        AssignedTo = child.AssignedTo,
                        AssignedDate = child.AssignDate,
                        ExpectedReturnDate = child.ExpectedReturnDate,

                        Location = child.AssignedUser?.Location,
                        TableNo = child.AssignedUser?.TableNo
                    }).ToList()
            }).ToList();

            return Result<List<AssetResponseDto>>.Success(result);
        }

        public async Task<Result<string>> UpdateAssetAsync(UpdateAssetDto dto)
        {
            var asset = await _unitOfWork.Assets.GetByIdWithChildrenAsync(dto.Id);

            if (asset == null)
                return Result<string>.Failure("Asset not found", 404);

            // Serial check
            if (await _unitOfWork.Assets.SerialExistsAsync(dto.SerialNo, dto.Id))
                return Result<string>.Failure("Serial already exists", 400);

            bool isParent = asset.ParentAssetId == null && asset.ChildAssets.Any();
            bool isChild = asset.ParentAssetId != null;

            // delete child from parent asset 
            if (dto.IsManualUnlink && isChild)
            {
                asset.ParentAssetId = null;
                asset.StatusId = 1; // Available
                asset.AssignedTo = null;
                asset.AssignDate = null;

                await _unitOfWork.SaveChangesAsync();
                return Result<string>.Success("Child unlinked successfully");
            }

            if (dto.IsFromParentContext)
            {
                // 🔹 CHILD → AVAILABLE (remove from parent)
                if (isChild && dto.StatusId == 1)
                {
                    asset.ParentAssetId = null;
                    asset.AssignedTo = null;
                    asset.AssignDate = null;

                    await _unitOfWork.SaveChangesAsync();
                    return Result<string>.Success("Child converted to independent asset");
                }

                // 🔹 CHILD ASSIGN VALIDATION
                if (isChild && dto.StatusId == 2)
                {
                    var parent = await _unitOfWork.Assets.GetByIdAsync(asset.ParentAssetId!.Value);

                    if (parent?.AssignedTo == null)
                        return Result<string>.Failure("Cannot assign child when parent is not assigned", 400);
                }

                // 🔹 PARENT LOGIC
                if (isParent)
                {
                    // Reassign parent → break children
                    if (asset.AssignedTo != dto.AssignedTo && dto.AssignedTo.HasValue)
                    {
                        foreach (var child in asset.ChildAssets)
                        {
                            child.ParentAssetId = null;
                        }
                    }

                    // Parent → Available
                    if (dto.StatusId == 1)
                    {
                        asset.AssignedTo = null;
                        asset.AssignDate = null;
                    }
                }

                // 🔹 NORMAL UPDATE
                MapBasicFields(asset, dto);

                asset.StatusId = dto.StatusId;
                asset.AssignedTo = dto.AssignedTo;
                asset.AssignDate = dto.AssignedDate;
                asset.ExpectedReturnDate = dto.ExpectedReturnDate;

                await _unitOfWork.SaveChangesAsync();
                return Result<string>.Success("Asset updated successfully");
            }
            else
            {
                // 🔴 DIRECT EDIT (child from list)

                if (dto.StatusId == 2)
                    return Result<string>.Failure("Direct assign not allowed", 400);

                MapBasicFields(asset, dto);

                asset.StatusId = dto.StatusId;

                if (dto.StatusId != 2)
                {
                    asset.AssignedTo = null;
                    asset.AssignDate = null;
                }

                await _unitOfWork.SaveChangesAsync();
                return Result<string>.Success("Asset updated successfully");
            }
        }

        private void MapBasicFields(Asset asset, UpdateAssetDto dto)
        {
            asset.Image = dto.Image;
            asset.ItemName = dto.ItemName;
            asset.CategoryId = dto.CategoryId;
            asset.SubCategoryId = dto.SubCategoryId;
            asset.ConditionId = dto.ConditionId;
            asset.Brand = dto.Brand;
            asset.Model = dto.Model;
            asset.SerialNo = dto.SerialNo;

            asset.Vendor = dto.Vendor;
            asset.PurchaseCost = dto.PurchaseCost;
            asset.PurchaseDate = dto.PurchaseDate;
            asset.InvoiceNumber = dto.InvoiceNumber;

            asset.WarrantyExpiry = dto.WarrantyExpiry;
            asset.AmcExpiry = dto.AmcExpiry;

            asset.UpdatedAt = DateTime.UtcNow;
        }

        public async Task<Result<string>> DeleteAssetAsync(int id)
        {
            var asset = await _unitOfWork.Assets.GetByIdWithChildrenAsync(id);

            if (asset == null)
                return Result<string>.Failure("Asset not found", 404);

            bool isParent = asset.ParentAssetId == null;
            bool isChild = asset.ParentAssetId != null;

            // ✅ SCENARIO 1: PARENT DELETE
            if (isParent)
            {
                if (asset.ChildAssets.Any())
                {
                    foreach (var child in asset.ChildAssets)
                    {
                        // 🔹 Unlink child (DO NOT change status)
                        child.ParentAssetId = null;
                    }
                }

                // 🔹 Soft delete parent
                asset.IsActive = false;
            }

            // ✅ SCENARIO 2: CHILD DELETE
            if (isChild)
            {
                // 🔹 Unlink from parent first
                asset.ParentAssetId = null;

                // 🔹 Then soft delete
                asset.IsActive = false;
            }

            

            await _unitOfWork.SaveChangesAsync();

            return Result<string>.Success("Asset deleted successfully");
        }

        public async Task<Result<List<UserDto>>> GetSuggestedUsersAsync()
        {
            var users = await _unitOfWork.Users.GetUsersWithOpenTicketsAsync();

            var result = users.Select(u => new UserDto
            {
                Id = u.Id,
                FullName = u.FullName,
                Email = u.Email
            }).ToList();

            return Result<List<UserDto>>.Success(result);
        }

        public async Task<Result<List<UserDto>>> SearchUsersAsync(string query)
        {
            var users = await _unitOfWork.Users.SearchAsync(query);

            var result = users.Select(u => new UserDto
            {
                Id = u.Id,
                FullName = u.FullName,
                Email = u.Email
            }).ToList();

            return Result<List<UserDto>>.Success(result);
        }

        public async Task<Result<string>> AssignAssetAsync(AssignAssetDto dto)
        {
            var asset = await _unitOfWork.Assets.GetByIdAsync(dto.AssetId);

            if (asset == null)
                return Result<string>.Failure("Asset not found", 404);

            // ✅ Only available assets can be assigned
            if (asset.StatusId != 1)
                return Result<string>.Failure("Only available assets can be assigned", 400);

            var user = await _unitOfWork.Users.GetByIdAsync(dto.UserId);

            if (user == null)
                return Result<string>.Failure("User not found", 404);

            // ✅ Table validation
            if (!string.IsNullOrEmpty(dto.TableNo))
            {
                var isTableUsed = await _unitOfWork.Users.TableAlreadyAssignedAsync(dto.TableNo);

                if (isTableUsed)
                    return Result<string>.Failure($"Table {dto.TableNo} already assigned", 400);

                user.TableNo = dto.TableNo;
            }

            // ✅ Update user location
            if (!string.IsNullOrEmpty(dto.Location))
                user.Location = dto.Location;

            // ✅ Assign asset
            asset.AssignedTo = dto.UserId;
            asset.AssignDate = dto.AssignedDate;
            asset.ExpectedReturnDate = dto.ExpectedReturnDate;

            asset.StatusId = 2; // Assigned

            await _unitOfWork.SaveChangesAsync();

            return Result<string>.Success("Asset assigned successfully");
        }

    }


}