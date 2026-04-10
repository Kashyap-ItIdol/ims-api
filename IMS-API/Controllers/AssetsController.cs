using IMS_API.Controllers.Base;
using IMS_Application.DTOs;
using IMS_Application.Services.Interfaces;
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
}