using IMS_API.Controllers.Base;
using IMS_Application.Common.Models;
using IMS_Application.DTOs;
using IMS_Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
            var userResult = GetCurrentUserId();
            if (!userResult.IsSuccess)
                return FromResult(userResult);

            return FromResult(await _service.Add(dto, userResult.Data));
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            return FromResult(await _service.GetAll());
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            return FromResult(await _service.GetById(id));
        }

        [HttpPatch("quick/{id}")]
        public async Task<IActionResult> QuickUpdate(int id, [FromBody] EditClientAssetQuickDto dto)
        {
            var userResult = GetCurrentUserId();
            if (!userResult.IsSuccess)
                return FromResult(userResult);

            return FromResult(await _service.QuickUpdate(id, dto, userResult.Data));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] EditClientAssetFullDto dto)
        {
            var userResult = GetCurrentUserId();
            if (!userResult.IsSuccess)
                return FromResult(userResult);

            return FromResult(await _service.FullUpdate(id, dto, userResult.Data));
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var userResult = GetCurrentUserId();
            if (!userResult.IsSuccess)
                return FromResult(userResult);

            return FromResult(await _service.Delete(id));
        }

        [HttpPost("filter")]
        public async Task<IActionResult> Filter([FromBody] ClientAssetFilterDto filter)
        {
            var result = await _service.FilterAsync(filter);
            return FromResult(Result<IEnumerable<ClientAsset>>.Success(result));
        }

        [HttpPost("{id}/attachments")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UploadAttachment(int id, [FromForm] UploadAttachmentRequest request)
        {
            var userResult = GetCurrentUserId();
            if (!userResult.IsSuccess)
                return FromResult(userResult);

            if (request.File == null || request.File.Length == 0)
                return BadRequest("No file uploaded or file is empty.");

            return FromResult(await _service.UploadAttachmentAsync(id, request.File, userResult.Data));
        }

        [HttpGet("{id}/attachments")]
        public async Task<IActionResult> GetAttachments(int id)
        {
            return FromResult(await _service.GetAttachmentsByAssetAsync(id));
        }

        [HttpDelete("attachments/{attachmentId}")]
        public async Task<IActionResult> DeleteAttachment(int attachmentId)
        {
            var userResult = GetCurrentUserId();
            if (!userResult.IsSuccess)
                return FromResult(userResult);

            return FromResult(await _service.DeleteAttachmentAsync(attachmentId, userResult.Data));
        }

        [HttpGet("attachments/{attachmentId}/download")]
        public async Task<IActionResult> DownloadAttachment(int attachmentId)
        {
            var result = await _service.DownloadAttachmentAsync(attachmentId);

            if (!result.IsSuccess)
                return FromResult(result);

            (byte[] fileBytes, _, string fileName) = result.Data;
            return File(fileBytes, "application/octet-stream", fileName);
        }

        [HttpGet("attachments/{attachmentId}/view")]
        public async Task<IActionResult> ViewAttachment(int attachmentId)
        {
            var result = await _service.ViewAttachmentAsync(attachmentId);

            if (!result.IsSuccess)
                return FromResult(result);

            (byte[] fileBytes, string contentType, _) = result.Data;
            return File(fileBytes, contentType);
        }
    }
}
