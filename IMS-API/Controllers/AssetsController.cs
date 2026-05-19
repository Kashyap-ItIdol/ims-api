using IMS_API.Controllers.Base;
using IMS_Application.DTOs;
using IMS_Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
//[Authorize(Roles = "Admin,Support Engineer")]
public class AssetController : BaseController
{
    private readonly IAssetService _assetService;

    public AssetController(IAssetService assetService)
    {
        _assetService = assetService;
    }
    [HttpPost("Add-Asset")]
    public async Task<IActionResult> AddInventoryAssets(AddAssetDto dto)
    {
        var userResult = GetCurrentUserId();
        if (!userResult.IsSuccess)
            return FromResult(userResult);

        return FromResult(await _assetService.AddAssetsAsync(dto, userResult.Data));
    }

    [Authorize(Roles = "Admin,Support Engineer,Employee")]
    [HttpGet("get-all-Assets")]
    public async Task<IActionResult> GetAllAssets()
    {
        var userResult = GetCurrentUserId();
        if (!userResult.IsSuccess)
            return FromResult(userResult);

        var role = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
        var result = await _assetService.GetAllAssetsAsync(userResult.Data, role ?? string.Empty);

        if (!result.IsSuccess)
            return StatusCode(result.StatusCode, result);

        return FromResult(result);
    }

    [Authorize(Roles = "Admin,Support Engineer,Employee")]
    [HttpGet("get-all-Assets/export")]
    public async Task<IActionResult> ExportAllAssetsCsv()
    {
        var userResult = GetCurrentUserId();
        if (!userResult.IsSuccess)
            return FromResult(userResult);

        var result = await _assetService.ExportAllAssetsCsvAsync();
        if (!result.IsSuccess)
            return StatusCode(result.StatusCode, result);

        var fileName = $"assets_{DateTime.UtcNow:yyyyMMdd_HHmmss}.csv";
        return File(result.Data, "text/csv", fileName);
    }

    [Authorize(Roles = "Admin,Support Engineer")]
    [HttpPost("import")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> ImportAssets([FromForm] ImportAssetsRequestDto dto)
    {
        var userResult = GetCurrentUserId();
        if (!userResult.IsSuccess)
            return FromResult(userResult);

        return FromResult(await _assetService.ImportAssetsCsvAsync(dto, userResult.Data));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateAsset(int id, UpdateAssetDto dto)
    {
        if (id != dto.Id)
            return BadRequest("Id mismatch");

        var userResult = GetCurrentUserId();
        if (!userResult.IsSuccess)
            return FromResult(userResult);

        return FromResult(await _assetService.UpdateAssetAsync(dto, userResult.Data));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAsset(int id)
    {
        var userResult = GetCurrentUserId();
        if (!userResult.IsSuccess)
            return FromResult(userResult);

        return FromResult(await _assetService.DeleteAssetAsync(id, userResult.Data));
    }

    [HttpGet("suggest-users")]
    public async Task<IActionResult> GetSuggestedUsers()
    {
        return FromResult(await _assetService.GetSuggestedUsersAsync());
    }

    [HttpGet("search-users")]
    public async Task<IActionResult> SearchUsers([FromQuery] string query)
    {
        return FromResult(await _assetService.SearchUsersAsync(query));
    }

    [HttpPost("assign-asset")]
    public async Task<IActionResult> AssignAsset(AssignAssetDto dto)
    {
        return FromResult(await _assetService.AssignAssetAsync(dto));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetAssetById(int id)
    {
        return FromResult(await _assetService.GetAssetByIdAsync(id));
    }

    [HttpPost("attach-child")]
    public async Task<IActionResult> AttachChild(AttachChildDto dto)
    {
        return FromResult(await _assetService.AttachChildAsync(dto));
    }

    [HttpPost("create-child")]
    public async Task<IActionResult> CreateChild(CreateChildAssetDto dto)
    {
        return FromResult(await _assetService.CreateAndAttachChildAsync(dto));
    }

    [HttpPost("detach-child")]
    public async Task<IActionResult> DetachChild(DetachChildDto dto)
    {
        return FromResult(await _assetService.DetachChildAsync(dto));
    }

    [HttpPost("filter")]
    public async Task<IActionResult> FilterAssets([FromBody] AssetFilterDto dto)
    {
        return FromResult(await _assetService.FilterAssetsAsync(dto));
    }

    [HttpPost("{id}/network")]
    public async Task<IActionResult> AddOrUpdateNetwork(int id, NetworkDetailsDto dto)
    {
        var userResult = GetCurrentUserId();
        if (!userResult.IsSuccess)
            return FromResult(userResult);

        return FromResult(await _assetService.AddOrUpdateNetworkAsync(id, dto, userResult.Data));
    }

}

