using IMS_Application.DTOs;
using IMS_Application.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class AssetController : ControllerBase
{
    private readonly IAssetService _assetService;

    public AssetController(IAssetService assetService)
    {
        _assetService = assetService;
    }

    // 🔵 Inventory Assets
    [HttpPost("inventory")]
    public async Task<IActionResult> AddInventoryAssets(AddAssetDto dto)
    {
        await _assetService.AddAssetsAsync(dto, false);
        return Ok("Inventory assets added");
    }

    // 🟣 Client Assets
    [HttpPost("client")]
    public async Task<IActionResult> AddClientAssets(AddAssetDto dto)
    {
        await _assetService.AddAssetsAsync(dto, true);
        return Ok("Client assets added");
    }
}