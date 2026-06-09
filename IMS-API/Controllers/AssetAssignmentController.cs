using IMS_Application.DTOs;
using IMS_Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using IMS_API.Controllers.Base;

namespace IMS_API.Controllers
{
    [ApiController]
    [Route("api/asset-assignments")]
    [Authorize]
    public class AssetAssignmentController : BaseController
    {
        private readonly IAssetAssignmentService _service;

        public AssetAssignmentController(IAssetAssignmentService service)
        {
            _service = service;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] AssetAssignmentDto dto)
        {
            var userResult = GetCurrentUserId();
            if (!userResult.IsSuccess)
                return FromResult(userResult);

            return FromResult(await _service.AssignAssetAsync(dto, userResult.Data));
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            return FromResult(await _service.GetAllAsync());
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            return FromResult(await _service.GetByIdAsync(id));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] AssetAssignmentDto dto)
        {
            var userResult = GetCurrentUserId();
            if (!userResult.IsSuccess)
                return FromResult(userResult);

            return FromResult(await _service.UpdateAssetAsync(id, dto, userResult.Data));
        }

        [HttpPost("{id}/return")]
        public async Task<IActionResult> ReturnAsset(int id)
        {
            var userResult = GetCurrentUserId();
            if (!userResult.IsSuccess)
                return FromResult(userResult);

            return FromResult(await _service.ReturnAssetAsync(id, DateTime.UtcNow, userResult.Data));
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var userResult = GetCurrentUserId();
            if (!userResult.IsSuccess)
                return FromResult(userResult);

            return FromResult(await _service.DeleteAssetAsync(id, userResult.Data));
        }

        [HttpPost("create-and-assign")]
        public async Task<IActionResult> CreateAndAssign([FromBody] CreateAndAssignAssetDto dto)
        {
            var userResult = GetCurrentUserId();
            if (!userResult.IsSuccess)
                return FromResult(userResult);

            if (dto.EmployeeId <= 0)
                dto.EmployeeId = userResult.Data;

            return FromResult(await _service.CreateAndAssignAssetAsync(dto, userResult.Data));
        }
    }
}