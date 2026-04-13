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

        public async Task<Result<string>> DeleteAssetAsync(int id)
        {
            var asset = await _unitOfWork.Assets.GetByIdAsync(id);

            if (asset == null)
                return Result<string>.Failure("Asset not found", 404);


            var hasChildren = await _unitOfWork.Assets.HasChildrenAsync(id);

            if (hasChildren)
                return Result<string>.Failure("Cannot delete parent asset. Remove child assets first.", 400);

            _unitOfWork.Assets.SoftDelete(asset);
            await _unitOfWork.SaveChangesAsync();

            return Result<string>.Success("Asset deleted successfully");
        }

        public async Task<Result<string>> UpdateAssetAsync(UpdateAssetDto dto)
        {
            if (dto.MainAsset == null)
                return Result<string>.Failure("Main asset data required", 400);


            var asset = await _unitOfWork.Assets.GetByIdAsync(dto.Id);

            if (asset == null)
                return Result<string>.Failure("Asset not found", 404);


            Asset root = asset.ParentAssetId == null
                ? asset
                : await _unitOfWork.Assets.GetByIdAsync(asset.ParentAssetId.Value);

            if (root == null)
                return Result<string>.Failure("Parent asset not found", 404);


            var serialExists = await _unitOfWork.Assets.SerialExistsAsync(dto.MainAsset.SerialNo);
            if (serialExists && root.SerialNo != dto.MainAsset.SerialNo)
                return Result<string>.Failure("Serial already exists", 400);


            root.ItemName = dto.MainAsset.ItemName;
            root.Brand = dto.MainAsset.Brand;
            root.Model = dto.MainAsset.Model;
            root.SerialNo = dto.MainAsset.SerialNo;
            root.StatusId = dto.MainAsset.StatusId;
            root.CategoryId = dto.MainAsset.CategoryId;
            root.SubCategoryId = dto.MainAsset.SubCategoryId;
            root.ConditionId = dto.MainAsset.ConditionId;

            root.Vendor = dto.MainAsset.Vendor!;
            root.PurchaseCost = dto.MainAsset.PurchaseCost!.Value;
            root.PurchaseDate = dto.MainAsset.PurchaseDate!.Value;
            root.InvoiceNumber = dto.MainAsset.InvoiceNumber!;

            root.WarrantyExpiry = dto.MainAsset.WarrantyExpiry;
            root.AmcExpiry = dto.MainAsset.AmcExpiry;


            if (dto.AssignedTo.HasValue)
            {
                var userExists = await _unitOfWork.Users.ExistsAsync(dto.AssignedTo.Value);

                if (!userExists)
                    return Result<string>.Failure($"User not found with id {dto.AssignedTo.Value}", 404);

                var user = await _unitOfWork.Users.GetByIdAsync(dto.AssignedTo.Value);


                if (dto.AssignedDate.HasValue && dto.ExpectedReturnDate.HasValue)
                {
                    if (dto.ExpectedReturnDate < dto.AssignedDate)
                        return Result<string>.Failure("ExpectedReturnDate cannot be before AssignedDate", 400);
                }

                root.AssignedTo = dto.AssignedTo;
                root.AssignDate = dto.AssignedDate ?? DateTime.UtcNow;
                root.ExpectedReturnDate = dto.ExpectedReturnDate;

                if (!string.IsNullOrEmpty(dto.Location))
                    user.Location = dto.Location;

                if (!string.IsNullOrEmpty(dto.TableNo))
                {
                    var isTableUsed = await _unitOfWork.Users.TableAlreadyAssignedAsync(dto.TableNo);

                    if (isTableUsed && user.TableNo != dto.TableNo)
                        return Result<string>.Failure($"Table {dto.TableNo} already assigned", 400);

                    user.TableNo = dto.TableNo;
                }
            }
            else
            {
                root.AssignedTo = null;
                root.AssignDate = null;
                root.ExpectedReturnDate = null;
            }


            foreach (var child in dto.Children)
            {
                var action = child.Action.ToLower();


                if (action == "delete" && child.Id.HasValue)
                {
                    var existing = await _unitOfWork.Assets.GetByIdAsync(child.Id.Value);

                    if (existing == null || existing.ParentAssetId != root.Id)
                        return Result<string>.Failure("Invalid child asset", 400);

                    existing.ParentAssetId = null;
                }


                else if (action == "update" && child.Id.HasValue && child.Data != null)
                {
                    var existing = await _unitOfWork.Assets.GetByIdAsync(child.Id.Value);

                    if (existing == null || existing.ParentAssetId != root.Id)
                        return Result<string>.Failure("Invalid child asset", 400);

                    var exists = await _unitOfWork.Assets.SerialExistsAsync(child.Data.SerialNo);
                    if (exists && existing.SerialNo != child.Data.SerialNo)
                        return Result<string>.Failure("Serial already exists", 400);

                    existing.ItemName = child.Data.ItemName;
                    existing.Brand = child.Data.Brand;
                    existing.Model = child.Data.Model;
                    existing.SerialNo = child.Data.SerialNo;

                    existing.StatusId = child.Data.StatusId;
                    existing.CategoryId = child.Data.CategoryId;
                    existing.SubCategoryId = child.Data.SubCategoryId;
                    existing.ConditionId = child.Data.ConditionId;

                    existing.Vendor = child.Data.IsPurchaseDetailsSame ? root.Vendor : child.Data.Vendor!;
                    existing.PurchaseCost = child.Data.IsPurchaseDetailsSame ? root.PurchaseCost : child.Data.PurchaseCost!.Value;
                    existing.PurchaseDate = child.Data.IsPurchaseDetailsSame ? root.PurchaseDate : child.Data.PurchaseDate!.Value;
                    existing.InvoiceNumber = child.Data.IsPurchaseDetailsSame ? root.InvoiceNumber : child.Data.InvoiceNumber!;
                }


                else if (action == "add" && child.Data != null)
                {
                    var exists = await _unitOfWork.Assets.SerialExistsAsync(child.Data.SerialNo);
                    if (exists)
                        return Result<string>.Failure($"Serial already exists: {child.Data.SerialNo}", 400);

                    var newChild = new Asset
                    {
                        ItemName = child.Data.ItemName,
                        Brand = child.Data.Brand,
                        Model = child.Data.Model,
                        SerialNo = child.Data.SerialNo,

                        StatusId = child.Data.StatusId,
                        CategoryId = child.Data.CategoryId,
                        SubCategoryId = child.Data.SubCategoryId,
                        ConditionId = child.Data.ConditionId,

                        Vendor = child.Data.IsPurchaseDetailsSame ? root.Vendor : child.Data.Vendor!,
                        PurchaseCost = child.Data.IsPurchaseDetailsSame ? root.PurchaseCost : child.Data.PurchaseCost!.Value,
                        PurchaseDate = child.Data.IsPurchaseDetailsSame ? root.PurchaseDate : child.Data.PurchaseDate!.Value,
                        InvoiceNumber = child.Data.IsPurchaseDetailsSame ? root.InvoiceNumber : child.Data.InvoiceNumber!,

                        ParentAssetId = root.Id
                    };

                    await _unitOfWork.Assets.AddRangeAsync(new List<Asset> { newChild });
                }


                else if (action == "attach" && child.Id.HasValue)
                {
                    var existing = await _unitOfWork.Assets.GetByIdAsync(child.Id.Value);

                    if (existing == null)
                        return Result<string>.Failure("Asset not found", 404);

                    if (existing.Id == root.Id)
                        return Result<string>.Failure("Cannot attach asset to itself", 400);

                    if (existing.ParentAssetId != null)
                        return Result<string>.Failure("Asset already attached", 400);

                    existing.ParentAssetId = root.Id;
                }
            }

            var allAssets = await _unitOfWork.Assets.GetAllAsync();

            var children = allAssets.Where(x => x.ParentAssetId == root.Id).ToList();

            foreach (var child in children)
            {
                child.AssignedTo = root.AssignedTo;
                child.AssignDate = root.AssignDate;
                child.ExpectedReturnDate = root.ExpectedReturnDate;
            }

            await _unitOfWork.SaveChangesAsync();

            return Result<string>.Success("Asset updated successfully");
        }

        //public async Task<Result<string>> AssignAssetAsync(AssignAssetDto dto)
        //{
        //    var asset = await _unitOfWork.Assets.GetByIdAsync(dto.AssetId);

        //    if (asset == null)
        //        return Result<string>.Failure("Asset not found", 404);

        //    if (asset.AssignedTo != null)
        //        return Result<string>.Failure("Asset already assigned", 400);

        //    var user = await _unitOfWork.Users.GetByIdAsync(dto.EmployeeId);

        //    if (user == null)
        //        return Result<string>.Failure("User not found", 404);

        //    if (dto.AssignedDate == default)
        //        dto.AssignedDate = DateTime.UtcNow;

        //    if (dto.ExpectedReturnDate.HasValue &&
        //        dto.ExpectedReturnDate < dto.AssignedDate)
        //    {
        //        return Result<string>.Failure("Invalid return date", 400);
        //    }

        //    if (!string.IsNullOrEmpty(dto.TableNo))
        //    {
        //        var isUsed = await _unitOfWork.Users.TableAlreadyAssignedAsync(dto.TableNo);

        //        if (isUsed && user.TableNo != dto.TableNo)
        //            return Result<string>.Failure("Table already assigned", 400);

        //        user.TableNo = dto.TableNo;
        //    }

        //    if (!string.IsNullOrEmpty(dto.Location))
        //        user.Location = dto.Location;

        //    // ✅ Assign
        //    asset.AssignedTo = dto.EmployeeId;
        //    asset.AssignDate = dto.AssignedDate;
        //    asset.ExpectedReturnDate = dto.ExpectedReturnDate;

        //    // ✅ Assign children
        //    var allAssets = await _unitOfWork.Assets.GetAllAsync();

        //    var children = allAssets.Where(x => x.ParentAssetId == asset.Id).ToList();

        //    foreach (var child in children)
        //    {
        //        child.AssignedTo = dto.EmployeeId;
        //        child.AssignDate = dto.AssignedDate;
        //        child.ExpectedReturnDate = dto.ExpectedReturnDate;
        //    }

        //    await _unitOfWork.SaveChangesAsync();

        //    return Result<string>.Success("Asset assigned successfully");
        //}

        //public async Task<Result<List<User>>> GetSuggestedEmployeesAsync()
        //{
        //    var userIds = await _unitOfWork.Tickets.GetRecentUserIdsAsync(5);

        //    var users = new List<User>();

        //    foreach (var id in userIds)
        //    {
        //        var user = await _unitOfWork.Users.GetByIdAsync(id);
        //        if (user != null)
        //            users.Add(user);
        //    }

        //    return Result<List<User>>.Success(users);
        //}


        //public async Task<Result<List<User>>> SearchEmployeesAsync(string query)
        //{
        //    var users = await _unitOfWork.Users.SearchAsync(query);
        //    return Result<List<User>>.Success(users);
        //}







    }


}