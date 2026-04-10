using IMS_Application.Common.Models;
using IMS_Application.DTOs;
using IMS_Application.Interfaces;
using IMS_Application.Services.Interfaces;
using IMS_Domain.Entities;

namespace IMS_Application.Services
{
    public class AssetService : IAssetService
    {
        private readonly IUnitOfWork _unitOfWork;

        public AssetService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
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

                // Purchase validation
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

            //  AssignedTo validation
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

            // Table validation
            if (!string.IsNullOrEmpty(dto.TableNo))
            {
                var isTableUsed = await _unitOfWork.Users.TableAlreadyAssignedAsync(dto.TableNo);

                if (isTableUsed)
                    return Result<string>.Failure($"Table {dto.TableNo} is already assigned to another user", 400);
            }

            //  Date validation
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

       
            var main = dto.Assets.First(x => x.IsPrimary);

            var assets = new List<Asset>();

            foreach (var item in dto.Assets)
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

                    Vendor = item.IsPurchaseDetailsSame ? main.Vendor! : item.Vendor!,
                    PurchaseCost = item.IsPurchaseDetailsSame ? main.PurchaseCost!.Value : item.PurchaseCost!.Value,
                    PurchaseDate = item.IsPurchaseDetailsSame ? main.PurchaseDate!.Value : item.PurchaseDate!.Value,
                    InvoiceNumber = item.IsPurchaseDetailsSame ? main.InvoiceNumber! : item.InvoiceNumber!,

                    WarrantyExpiry = item.WarrantyExpiry,
                    AmcExpiry = item.AmcExpiry,

                    IsClient = isClient,

                    AssignedTo = dto.AssignedTo,
                    AssignDate = dto.AssignedDate ?? DateTime.UtcNow,
                    ExpectedReturnDate = dto.ExpectedReturnDate
                };

                assets.Add(asset);
            }

            await _unitOfWork.Assets.AddRangeAsync(assets);
            await _unitOfWork.SaveChangesAsync();

            return Result<string>.Success("Assets added successfully");
        }

        public async Task<Result<List<AssetResponseDto>>> GetAllAssetsAsync()
        {
            var assets = await _unitOfWork.Assets.GetAllAsync();

            var result = assets.Select(a => new AssetResponseDto
            {
                Image = a.Image,
                ItemName = a.ItemName,
                StatusId = a.StatusId,
                CategoryId = a.CategoryId,
                SubCategoryId = a.SubCategoryId,
                ConditionId = a.ConditionId,
                Brand = a.Brand,
                Model = a.Model,
                SerialNo = a.SerialNo,
                Vendor = a.Vendor,
                PurchaseCost = a.PurchaseCost,
                PurchaseDate = a.PurchaseDate,
                InvoiceNumber = a.InvoiceNumber,
                WarrantyExpiry = a.WarrantyExpiry,
                AmcExpiry = a.AmcExpiry,
                AssignedTo = a.AssignedTo,
                AssignedDate = a.AssignDate,
                ExpectedReturnDate = a.ExpectedReturnDate,

                // Map the Location and TableNo from the related User entity
                Location = a.AssignedUser != null ? a.AssignedUser.Location : null,  // Fetch Location from User
                TableNo = a.AssignedUser != null ? a.AssignedUser.TableNo : null     // Fetch TableNo from User
            }).ToList();

            return Result<List<AssetResponseDto>>.Success(result);
        }
    }


}