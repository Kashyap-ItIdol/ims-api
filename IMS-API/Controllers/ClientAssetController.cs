using IMS_API.Controllers.Base;
using IMS_Application.Common.Models;
using IMS_Application.DTOs;
using IMS_Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using IMS_Domain.Entities;

namespace IMS_API.Controllers
{
    [ApiController]
    [Route("api/client-assets")]
    [Authorize]
    public class ClientAssetController : BaseController
    {
        private readonly IClientAssetService _service;

        public ClientAssetController(IClientAssetService service)
        {
            _service = service;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateClientAssetDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userResult = GetCurrentUserId();
            if (!userResult.IsSuccess)
            {
                return FromResult(userResult);
            }
            var result = await _service.Add(dto, userResult.Data);
            return FromResult(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _service.GetAll();
            return FromResult(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _service.GetById(id);
            return FromResult(result);
        }

        [HttpPatch("quick/{id}")]
        public async Task<IActionResult> QuickUpdate(int id, [FromBody] EditClientAssetQuickDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userResult = GetCurrentUserId();
            if (!userResult.IsSuccess)
            {
                return FromResult(userResult);
            }
            var result = await _service.QuickUpdate(id, dto);
            return FromResult(result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] EditClientAssetFullDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userResult = GetCurrentUserId();
            if (!userResult.IsSuccess)
            {
                return FromResult(userResult);
            }
            var result = await _service.FullUpdate(id, dto);
            return FromResult(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var userResult = GetCurrentUserId();
            if (!userResult.IsSuccess)
            {
                return FromResult(userResult);
            }
            var result = await _service.Delete(id);
            return FromResult(result);
        }

        [HttpPost("filter")]
        public async Task<IActionResult> Filter([FromBody] ClientAssetFilterDto filter)
        {
            var result = await _service.FilterAsync(filter);
            return FromResult(Result<IEnumerable<ClientAsset>>.Success(result));
        }

        [HttpPost("{id}/attachments/upload")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UploadAttachment(int id, [FromForm] IFormFile file)
        {
            var userResult = GetCurrentUserId();
            if (!userResult.IsSuccess)
            {
                return FromResult(userResult);
            }
            var result = await _service.UploadAttachmentAsync(id, file, userResult.Data);
            return FromResult(result);
        }

        [HttpGet("{id}/attachments")]
        public async Task<IActionResult> GetAttachments(int id)
        {
            var result = await _service.GetAttachmentsByAssetAsync(id);
            return FromResult(result);
        }

        [HttpDelete("attachments/{attachmentId}")]
        public async Task<IActionResult> DeleteAttachment(int attachmentId)
        {
            var userResult = GetCurrentUserId();
            if (!userResult.IsSuccess)
            {
                return FromResult(userResult);
            }
            var result = await _service.DeleteAttachmentAsync(attachmentId, userResult.Data);
            return FromResult(result);
        }

        [HttpGet("attachments/{attachmentId}/download")]
        public async Task<IActionResult> DownloadAttachment(int attachmentId)
        {
            var result = await _service.DownloadAttachmentAsync(attachmentId);
            
            if (!result.IsSuccess)
                return FromResult(result);
            
            (byte[] fileBytes, string _, string fileName) = result.Data;
            return File(fileBytes, "application/octet-stream", fileName);
        }

        [HttpGet("attachments/{attachmentId}/view")]
        public async Task<IActionResult> ViewAttachment(int attachmentId)
        {
            var result = await _service.ViewAttachmentAsync(attachmentId);
            
            if (!result.IsSuccess)
                return FromResult(result);
            
            (byte[] fileBytes, string contentType, string _) = result.Data;
            return File(fileBytes, contentType);
        }
    }
}