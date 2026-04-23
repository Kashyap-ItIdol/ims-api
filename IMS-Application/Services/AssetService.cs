using AutoMapper;
using IMS_Application.Common.Constants;
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
        private readonly IMapper _mapper;

        public AssetService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Result<string>> AddAssetsAsync(AddAssetDto dto, bool isClient)
        {
            if (dto.Assets == null || !dto.Assets.Any())
                return Result<string>.Failure(ErrorMessages.AssetsListEmpty, 400);

            if (dto.Assets.Count(x => x.IsPrimary) != 1)
                return Result<string>.Failure(ErrorMessages.ExactlyOnePrimaryAssetRequired, 400);

            // DTO validation handled by FluentValidation

            var mainDto = dto.Assets.First(x => x.IsPrimary);


            if (dto.AssignedTo.HasValue && mainDto.StatusId != 2)
                return Result<string>.Failure(ErrorMessages.AssignedAssetMustBeAssigned, 400);

            if (dto.AssignedDate.HasValue && dto.ExpectedReturnDate.HasValue)
            {
                if (dto.ExpectedReturnDate < dto.AssignedDate)
                    return Result<string>.Failure(ErrorMessages.ExpectedReturnDateBeforeAssignedDate, 400);
            }

            var serials = dto.Assets.Select(x => x.SerialNo).ToList();

            foreach (var serial in serials)
            {
                if (await _unitOfWork.Assets.SerialExistsAsync(serial))
                    return Result<string>.Failure(string.Format(ErrorMessages.SerialAlreadyExistsFormatted, serial), 400);
            }

            if (dto.AssignedTo.HasValue)
            {
                var userExists = await _unitOfWork.Users.ExistsAsync(dto.AssignedTo.Value);

                if (!userExists)
                    return Result<string>.Failure(string.Format(ErrorMessages.UserNotFoundById, dto.AssignedTo.Value), 404);

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
                    return Result<string>.Failure(string.Format(ErrorMessages.TableAlreadyAssignedToUser, dto.TableNo), 400);
            }


            var mainAsset = _mapper.Map<Asset>(mainDto);
            mainAsset.IsClient = isClient;
            mainAsset.AssignedTo = dto.AssignedTo;
            mainAsset.AssignDate = dto.AssignedTo.HasValue ? (dto.AssignedDate ?? DateTime.UtcNow) : null;
            mainAsset.ExpectedReturnDate = dto.ExpectedReturnDate;
            mainAsset.CreatedAt = DateTime.UtcNow;
            mainAsset.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.Assets.AddRangeAsync(new List<Asset> { mainAsset });
            await _unitOfWork.SaveChangesAsync();

            var childAssets = new List<Asset>();

            foreach (var item in dto.Assets.Where(x => !x.IsPrimary))
            {
                var asset = _mapper.Map<Asset>(item);
                asset.StatusId = mainAsset.StatusId;
                asset.IsClient = isClient;
                asset.AssignedTo = dto.AssignedTo;
                asset.AssignDate = dto.AssignedTo.HasValue ? (dto.AssignedDate ?? DateTime.UtcNow) : null;
                asset.ExpectedReturnDate = dto.ExpectedReturnDate;
                asset.ParentAssetId = mainAsset.Id;
                asset.CreatedAt = DateTime.UtcNow;
                asset.UpdatedAt = DateTime.UtcNow;

                // Conditional purchase details
                if (item.IsPurchaseDetailsSame)
                {
                    asset.Vendor = mainAsset.Vendor;
                    asset.PurchaseCost = mainAsset.PurchaseCost;
                    asset.PurchaseDate = mainAsset.PurchaseDate;
                    asset.InvoiceNumber = mainAsset.InvoiceNumber;
                }
                // else mapper handles from item

                childAssets.Add(asset);
            }

            if (childAssets.Any())
            {
                await _unitOfWork.Assets.AddRangeAsync(childAssets);
                await _unitOfWork.SaveChangesAsync();
            }

            return Result<string>.Success(SuccessMessages.AssetsAddedSuccessfully);
        }

        public async Task<Result<List<AssetResponseDto>>> GetAllAssetsAsync()
        {
            var assets = await _unitOfWork.Assets.GetAllAsync();


            var parentAssets = assets.Where(a => a.ParentAssetId == null).ToList();

            var result = _mapper.Map<List<AssetResponseDto>>(parentAssets);

            // Manually set nested children using mapper
            foreach (var dto in result)
            {
                var childAssets = assets.Where(x => x.ParentAssetId == dto!.Id).ToList();
                dto!.Children = _mapper.Map<List<AssetResponseDto>>(childAssets);
            }

            return Result<List<AssetResponseDto>>.Success(result);
        }

        public async Task<Result<string>> UpdateAssetAsync(UpdateAssetDto dto)
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

                await _unitOfWork.SaveChangesAsync();
                return Result<string>.Success(SuccessMessages.ChildUnlinkedSuccessfully);
            }

            if (dto.IsFromParentContext)
            {

                if (isChild && dto.StatusId == 1)
                {

                    asset.AssignedTo = null;
                    asset.AssignDate = null;

                    await _unitOfWork.SaveChangesAsync();
                    return Result<string>.Success("Child marked as available (still linked to parent)");
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

                await _unitOfWork.SaveChangesAsync();
                return Result<string>.Success("Asset updated successfully");
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

                await _unitOfWork.SaveChangesAsync();
                return Result<string>.Success("Asset updated successfully");
            }
        }



        public async Task<Result<string>> DeleteAssetAsync(int id)
        {
            var asset = await _unitOfWork.Assets.GetByIdWithChildrenAsync(id);

            if (asset == null)
                return Result<string>.Failure(ErrorMessages.AssetNotFound, 404);

            bool isParent = asset.ParentAssetId == null;
            bool isChild = asset.ParentAssetId != null;

            // SCENARIO 1: PARENT DELETE
            if (isParent)
            {
                if (asset.ChildAssets.Any())
                {
                    foreach (var child in asset.ChildAssets)
                    {
                        // Unlink child (DO NOT change status)
                        child.ParentAssetId = null;
                    }
                }

                //Soft delete parent
                asset.IsActive = false;
            }

            // SCENARIO 2: CHILD DELETE
            if (isChild)
            {
                // Unlink from parent first
                asset.ParentAssetId = null;

                // Then soft delete
                asset.IsActive = false;
            }



            await _unitOfWork.SaveChangesAsync();

            return Result<string>.Success(SuccessMessages.AssetDeletedSuccessfully);
        }

        public async Task<Result<List<UserDto>>> GetSuggestedUsersAsync()
        {
            var users = await _unitOfWork.Users.GetUsersWithOpenTicketsAsync();

            var result = _mapper.Map<List<UserDto>>(users);

            return Result<List<UserDto>>.Success(result);
        }

        public async Task<Result<List<UserDto>>> SearchUsersAsync(string query)
        {
            var users = await _unitOfWork.Users.SearchAsync(query);

            var result = _mapper.Map<List<UserDto>>(users);

            return Result<List<UserDto>>.Success(result);
        }

        public async Task<Result<string>> AssignAssetAsync(AssignAssetDto dto)
        {
            var asset = await _unitOfWork.Assets.GetByIdAsync(dto.AssetId);

            if (asset == null)
                return Result<string>.Failure(ErrorMessages.AssetNotFound, 404);

            //  Only available assets can be assigned
            if (asset.StatusId != 1)
                return Result<string>.Failure("Only available assets can be assigned", 400);

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

            await _unitOfWork.SaveChangesAsync();

            return Result<string>.Success(SuccessMessages.AssetAssignedSuccessfully);
        }

        public async Task<Result<GetAssetByIdResponseDto>> GetAssetByIdAsync(int id)
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

            var historyList = await _unitOfWork.AssetHistories.GetByAssetIdAsync(asset.Id);

            response.Assignment.History = _mapper.Map<List<AssetHistoryDto>>(historyList);

            if (isParent)
            {
                response.Overview.Children = _mapper.Map<List<ChildAssetDto>>(asset.ChildAssets.Where(c => c.IsActive));
            }
            ;

            return Result<GetAssetByIdResponseDto>.Success(response);
        }


        public async Task<Result<string>> AttachChildAsync(AttachChildDto dto)
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


            child.ParentAssetId = parent.Id;

            if (parent.AssignedTo != null)
            {
                child.AssignedTo = parent.AssignedTo;
                child.AssignDate = parent.AssignDate;
                child.StatusId = 2;
            }

            await _unitOfWork.AssetHistories.AddAsync(new AssetHistory
            {
                AssetId = child.Id,
                Action = "Attached",
                Description = $"Attached to parent asset {parent.ItemName}"
            });

            await _unitOfWork.SaveChangesAsync();

            return Result<string>.Success(SuccessMessages.ChildAttachedSuccessfully);
        }

        public async Task<Result<string>> CreateAndAttachChildAsync(CreateChildAssetDto dto)
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


            await _unitOfWork.AssetHistories.AddAsync(new AssetHistory
            {
                AssetId = child.Id,
                Action = "Created & Attached",
                Description = $"Created and attached to {parent.ItemName}"
            });


            await _unitOfWork.SaveChangesAsync();

            return Result<string>.Success(SuccessMessages.ChildCreatedAndAttachedSuccessfully);
        }

        public async Task<Result<string>> DetachChildAsync(DetachChildDto dto)
        {
            var child = await _unitOfWork.Assets.GetByIdAsync(dto.ChildId);

            if (child == null || child.ParentAssetId == null)
                return Result<string>.Failure(ErrorMessages.InvalidChildAsset, 400);

            //  Detach
            child.ParentAssetId = null;

            //  Reset assignment
            child.AssignedTo = null;
            child.AssignDate = null;
            child.StatusId = 1; // Available

            //  History
            await _unitOfWork.AssetHistories.AddAsync(new AssetHistory
            {
                AssetId = child.Id,
                Action = "Detached",
                Description = "Removed from parent asset"
            });

            await _unitOfWork.SaveChangesAsync();

            return Result<string>.Success(SuccessMessages.ChildDetachedSuccessfully);
        }

        public async Task<Result<List<AssetListDto>>> FilterAssetsAsync(AssetFilterDto dto)
        {
            var assets = await _unitOfWork.Assets.FilterAsync(dto);

            var response = _mapper.Map<List<AssetListDto>>(assets);

            return Result<List<AssetListDto>>.Success(response);
        }
    }
}