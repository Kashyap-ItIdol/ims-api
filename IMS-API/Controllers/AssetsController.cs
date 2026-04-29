using System.Security.Claims;
using IMS_API.Controllers.Base;
using IMS_Application.DTOs;
using IMS_Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;


[ApiController]
[Route("api/[controller]")]
public class AssetController : BaseController
{
    private readonly IAssetService _assetService;

    public AssetController(IAssetService assetService)
    {
        _assetService = assetService;
    }

    [Authorize(Roles = "Admin,Support Engineer")]
    [HttpPost("Add-Asset")]
    public async Task<IActionResult> AddInventoryAssets(AddAssetDto dto)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        var result = await _assetService.AddAssetsAsync(dto, userId);
        return FromResult(result);
    }

    [Authorize(Roles = "Admin,Support Engineer")]
    [HttpGet("get-all-Assets")]
    public async Task<IActionResult> GetAllAssets()
    {
        var result = await _assetService.GetAllAssetsAsync();

        if (!result.IsSuccess)
            return StatusCode(result.StatusCode, result);

        return FromResult(result);
    }

    [Authorize(Roles = "Admin,Support Engineer")]
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateAsset(int id, UpdateAssetDto dto)
    {
        if (id != dto.Id)
            return BadRequest("Id mismatch");

        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        var result = await _assetService.UpdateAssetAsync(dto, userId);
        return FromResult(result);
    }

    [Authorize(Roles = "Admin,Support Engineer")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAsset(int id)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        var result = await _assetService.DeleteAssetAsync(id, userId);
        return FromResult(result);
    }

    [HttpGet("suggest-users")]
    public async Task<IActionResult> GetSuggestedUsers()
    {
        var result = await _assetService.GetSuggestedUsersAsync();
        return FromResult(result);
    }

    [HttpGet("search-users")]
    public async Task<IActionResult> SearchUsers(string query)
    {
        var result = await _assetService.SearchUsersAsync(query);
        return FromResult(result);
    }

    [Authorize(Roles = "Admin,Support Engineer")]
    [HttpPost("assign-asset")]
    public async Task<IActionResult> AssignAsset(AssignAssetDto dto)
    {
        var result = await _assetService.AssignAssetAsync(dto);
        return FromResult(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetAssetById(int id)
    {
        var result = await _assetService.GetAssetByIdAsync(id);
        return FromResult(result);
    }

    [Authorize(Roles = "Admin,Support Engineer")]
    [HttpPost("attach-child")]
    public async Task<IActionResult> AttachChild(AttachChildDto dto)
    {
        var result = await _assetService.AttachChildAsync(dto);
        return FromResult(result);
    }

    [Authorize(Roles = "Admin,Support Engineer")]
    [HttpPost("create-child")]
    public async Task<IActionResult> CreateChild(CreateChildAssetDto dto)
    {
        var result = await _assetService.CreateAndAttachChildAsync(dto);
        return FromResult(result);
    }

    [Authorize(Roles = "Admin,Support Engineer")]
    [HttpPost("detach-child")]
    public async Task<IActionResult> DetachChild(DetachChildDto dto)
    {
        var result = await _assetService.DetachChildAsync(dto);
        return FromResult(result);
    }

    [HttpPost("filter")]
    public async Task<IActionResult> FilterAssets([FromBody] AssetFilterDto dto)
    {
        var result = await _assetService.FilterAssetsAsync(dto);
        return FromResult(result);
    }

    [Authorize(Roles = "Admin,Support Engineer")]
    [HttpPost("{id}/network")]
    public async Task<IActionResult> AddOrUpdateNetwork(int id, NetworkDetailsDto dto)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        var result = await _assetService.AddOrUpdateNetworkAsync(id, dto, userId);
        return FromResult(result);
    }

}

