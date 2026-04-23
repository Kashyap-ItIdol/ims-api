using IMS_API.Controllers.Base;
using IMS_Application.DTOs;
using IMS_Application.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using IMS_Application.Common.Constants;


[ApiController]
[Route("api/[controller]")]
public class AssetController : BaseController
{
    private readonly IAssetService _assetService;

    public AssetController(IAssetService assetService)
    {
        _assetService = assetService;
    }

    [HttpPost("inventory")]
    public async Task<IActionResult> AddInventoryAssets(AddAssetDto dto)
    {
        var result = await _assetService.AddAssetsAsync(dto, false);
        return FromResult(result);
    }

    [HttpPost("client")]
    public async Task<IActionResult> AddClientAssets(AddAssetDto dto)
    {
        var result = await _assetService.AddAssetsAsync(dto, true);
        return FromResult(result);
    }

    [HttpGet("get-all")]
    public async Task<IActionResult> GetAllAssets()
    {
        var result = await _assetService.GetAllAssetsAsync();

        if (!result.IsSuccess)
            return StatusCode(result.StatusCode, result);

        return Ok(result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateAsset(int id, UpdateAssetDto dto)
    {
        if (id != dto.Id)
            return BadRequest("Id mismatch");

        var result = await _assetService.UpdateAssetAsync(dto);
        return FromResult(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAsset(int id)
    {
        var result = await _assetService.DeleteAssetAsync(id);
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


    [HttpPost("assign")]
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

    [HttpPost("attach-child")]
    public async Task<IActionResult> AttachChild(AttachChildDto dto)
    {
        var result = await _assetService.AttachChildAsync(dto);
        return FromResult(result);
    }

    [HttpPost("create-child")]
    public async Task<IActionResult> CreateChild(CreateChildAssetDto dto)
    {
        var result = await _assetService.CreateAndAttachChildAsync(dto);
        return FromResult(result);
    }

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



}