using IMS_Application.DTOs;
using IMS_Application.Interfaces;
using IMS_Application.Services.Interfaces;
using IMS_Domain.Entities;

namespace IMS_Application.Services
{
    public class AssetService : IAssetService
    {
        private readonly IAssetRepository _repo;

        public AssetService(IAssetRepository repo)
        {
            _repo = repo;
        }

        public async Task AddAssetsAsync(AddAssetDto dto, bool isClient)
        {
            var assets = new List<Asset>();

            // 🔥 Get main asset
            var main = dto.Assets.FirstOrDefault(x => x.IsPrimary);
            if (main == null)
                throw new Exception("Main asset missing");

            foreach (var item in dto.Assets)
            {
                // 🔥 Duplicate check
                if (await _repo.SerialExistsAsync(item.SerialNo))
                    throw new Exception($"Serial exists: {item.SerialNo}");

                // 🔥 Purchase logic
                var vendor = item.IsPurchaseDetailsSame ? main.Vendor : item.Vendor;
                var cost = item.IsPurchaseDetailsSame ? main.PurchaseCost : item.PurchaseCost;
                var date = item.IsPurchaseDetailsSame ? main.PurchaseDate : item.PurchaseDate;
                var invoice = item.IsPurchaseDetailsSame ? main.InvoiceNumber : item.InvoiceNumber;

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

                    Vendor = vendor!,
                    PurchaseCost = cost ?? 0,
                    PurchaseDate = date ?? DateTime.UtcNow,
                    InvoiceNumber = invoice!,
                    WarrantyExpiry = item.WarrantyExpiry,
                    AmcExpiry = item.AmcExpiry,

                    // 🔥 IMPORTANT
                    IsClient = isClient,

                    AssignedTo = dto.AssignedTo,
                    AssignDate = dto.AssignedDate ?? DateTime.UtcNow,
                    ExpectedReturnDate = dto.ExpectedReturnDate
                };

                assets.Add(asset);
            }

            await _repo.AddRangeAsync(assets);
            await _repo.SaveChangesAsync();
        }
    }
}
