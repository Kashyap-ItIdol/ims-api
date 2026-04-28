# TODO: GetAssetById Enhancement & History Tracking

## Completed Steps
- [x] Analyzed codebase and gathered requirements
- [x] Got user confirmation for the plan

## Implementation Steps

### Step 1: Repository Layer - Add combined history fetch
- [x] Edit `IAssetRepository.cs`: Add `GetHistoryByAssetIdsAsync(List<int> ids)`
- [x] Edit `AssetRepository.cs`: Implement `GetHistoryByAssetIdsAsync`

### Step 2: DTO Update
- [x] Edit `AssetHistoryDto.cs`: Add `AssetId` and `AssetName` to identify whose history it is

### Step 3: Service Layer - Update `GetAssetByIdAsync`
- [x] Edit `AssetService.cs`: If parent, fetch combined parent + children history, sorted desc

### Step 4: Add Network Support
- [x] Edit `INetworkDetailsRepository.cs`: Add `AddOrUpdateAsync`
- [x] Edit `NetworkDetailsRepository.cs`: Implement `AddOrUpdateAsync`
- [x] Edit `IAssetService.cs`: Add `AddOrUpdateNetworkAsync`
- [x] Edit `AssetService.cs`: Implement `AddOrUpdateNetworkAsync` with history tracking
- [x] Edit `AssetsController.cs`: Add `[HttpPost("{id}/network")]` endpoint
- [x] Edit `SuccessMessages.cs`: Add network update success message

### Step 5: History Tracking - Add operations
- [x] Edit `AssetService.cs` - `AddAssetsAsync`: Add history entries for main + child assets
- [x] Edit `AssetService.cs` - `UpdateAssetAsync`: Add history entry
- [x] Edit `AssetService.cs` - `AssignAssetAsync`: Add history entry
- [x] Edit `AssetService.cs` - `DeleteAssetAsync`: Add history entry

### Step 6: Mapping Update
- [x] Edit `MappingProfile.cs`: Update `AssetHistoryDto` mapping and add `NetworkDetailsDto -> NetworkDetail`

### Step 7: Build & Verify
- [x] Run `dotnet build` to verify compilation - PASSED

## Summary of Changes

### 1. GetAssetById now shows parent + children history
When a parent asset is selected, the API now fetches history for both the parent and all its active children, merges them, and sorts by `CreatedAt` descending. The `AssetHistoryDto` includes `AssetId` and `AssetName` to identify which asset each history entry belongs to.

### 2. Add/Update Network Details (Optional)
New endpoint: `POST /api/asset/{id}/network`
- Adds network details if none exist
- Updates existing network details if already present
- Records "Network Added" or "Network Updated" in asset history

### 3. History Tracking Added
History entries are now automatically created for:
- **AddAssetsAsync**: "Created" for main asset and each child asset
- **UpdateAssetAsync**: "Updated" with context description
- **AssignAssetAsync**: "Assigned" with user name
- **DeleteAssetAsync**: "Deleted" for parent, "Detached" for children when parent deleted
- **AddOrUpdateNetworkAsync**: "Network Added" or "Network Updated"
