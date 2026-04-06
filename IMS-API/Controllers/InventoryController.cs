using IMS_Application.DTOs;
using IMS_Application.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace IMS_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class InventoryController : ControllerBase
    {
        private readonly IInventoryService _inventoryService;

        public InventoryController(IInventoryService inventoryService)
        {
            _inventoryService = inventoryService;
        }

        [HttpPost]
        public async Task<IActionResult> Create(InventoryCreateDto dto)
        {
            var id = await _inventoryService.CreateInventoryAsync(dto);

            return Ok(new
            {
                message = "Inventory created successfully",
                inventoryId = id
            });
        }
    }
}