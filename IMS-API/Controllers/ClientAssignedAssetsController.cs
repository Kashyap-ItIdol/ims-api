using System.Collections.Generic;
using System.Threading.Tasks;
using IMS_API.Controllers.Base;
using IMS_Application.DTOs;

using IMS_Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IMS_API.Controllers
{
    [ApiController]
    [Route("api/client-assigned-assets")]
    [Authorize]
    public class ClientAssignedAssetsController : BaseController
    {
        private readonly IAssignedAssetService _service;

        public ClientAssignedAssetsController(IAssignedAssetService service)
        {
            _service = service;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateClientAssignedAssetDto dto)

        {

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userResult = GetCurrentUserId();
            if (!userResult.IsSuccess)
                return FromResult(userResult);

            var result = await _service.CreateAsync(dto, userResult.Data);

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
        public async Task<IActionResult> Update(int id, [FromBody] UpdateClientAssignedAssetDto dto)
        {


            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userResult = GetCurrentUserId();
            if (!userResult.IsSuccess)
                return FromResult(userResult);

            var result = await _service.UpdateAsync(id, dto, userResult.Data);

            return FromResult(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var userResult = GetCurrentUserId();
            if (!userResult.IsSuccess)
                return FromResult(userResult);

            var result = await _service.DeleteAsync(id, userResult.Data);
            return FromResult(result);
        }
    }
}

