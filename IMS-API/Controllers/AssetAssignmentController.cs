using IMS_Application.DTOs;
using IMS_Application.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
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
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userResult = GetCurrentUserId();
            if (!userResult.IsSuccess)
                return FromResult(userResult);

            var result = await _service.AssignAssetAsync(dto, userResult.Data);
            return FromResult(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _service.GetAllAsync();
            return FromResult(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _service.GetByIdAsync(id);
            return FromResult(result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] AssetAssignmentDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userResult = GetCurrentUserId();
            if (!userResult.IsSuccess)
                return FromResult(userResult);

            var result = await _service.UpdateAssetAsync(id, dto, userResult.Data);
            return FromResult(result);
        }

        [HttpPost("{id}/return")]
        public async Task<IActionResult> ReturnAsset(int id)
        {
            var userResult = GetCurrentUserId();
            if (!userResult.IsSuccess)
                return FromResult(userResult);

            var result = await _service.ReturnAssetAsync(id, DateTime.UtcNow, userResult.Data);
            return FromResult(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var userResult = GetCurrentUserId();
            if (!userResult.IsSuccess)
                return FromResult(userResult);

            var result = await _service.DeleteAssetAsync(id, userResult.Data);
            return FromResult(result);
        }

        [HttpPost("create-and-assign")]
        public async Task<IActionResult> CreateAndAssign([FromBody] CreateAndAssignAssetDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userResult = GetCurrentUserId();
            if (!userResult.IsSuccess)
                return FromResult(userResult);

            if (dto.EmployeeId <= 0)
                dto.EmployeeId = userResult.Data;

            var result = await _service.CreateAndAssignAssetAsync(dto, userResult.Data);
            return FromResult(result);
        }
    }
}
