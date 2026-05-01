using IMS_API.Controllers.Base;
using IMS_Application.Common.Constants;
using IMS_Application.Common.Models;
using IMS_Application.DTOs;
using IMS_Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;

namespace IMS_API.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    public class TicketController : BaseController
    {
        private readonly ITicketService _ticketService;
        private readonly IWebHostEnvironment _env;

        public TicketController(ITicketService ticketService, IWebHostEnvironment env)
        {
            _ticketService = ticketService;
            _env = env;
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateTicket([FromBody] CreateTicketRequestDto dto)
        {
            var userIdResult = GetCurrentUserId();
            if (!userIdResult.IsSuccess)
            {
                return FromResult(userIdResult);
            }

            var result = await _ticketService.CreateTicketAsync(dto, userIdResult.Data);
            return FromResult(result);
        }

        [HttpPost("{ticketId}/comments")]
        public async Task<IActionResult> AddComment(int ticketId, [FromQuery] string commentText)
        {
            var userIdResult = GetCurrentUserId();
            if (!userIdResult.IsSuccess)
            {
                return FromResult(userIdResult);
            }

            if (string.IsNullOrWhiteSpace(commentText))
            {
                return FromResult(Result<object>.Failure(ErrorMessages.CommentRequires, 400));
            }

            var result = await _ticketService.AddCommentAsync(ticketId, commentText, userIdResult.Data);
            return FromResult(result);
        }

        [HttpPatch("{ticketId}/status")]
        public async Task<IActionResult> UpdateStatus(int ticketId, [FromQuery] string status)
        {
            var userIdResult = GetCurrentUserId();
            if (!userIdResult.IsSuccess)
            {
                return FromResult(userIdResult);
            }

            var result = await _ticketService.UpdateStatusAsync(ticketId, status, userIdResult.Data);
            return FromResult(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetTickets()
        {
            var userIdResult = GetCurrentUserId();
            if (!userIdResult.IsSuccess)
            {
                return FromResult(userIdResult);
            }

            var result = await _ticketService.GetAllTicketsAsync(userIdResult.Data);
            return FromResult(result);
        }
        [HttpGet("{ticketId}")]
        public async Task<IActionResult> GetTicket(int ticketId)
        {
            var userIdResult = GetCurrentUserId();
            if (!userIdResult.IsSuccess)
            {
                return FromResult(userIdResult);
            }

            var result = await _ticketService.GetTicketByIdAsync(ticketId, userIdResult.Data);
            return FromResult(result);
        }


        //[HttpGet("search")]
        //public async Task<IActionResult> SearchTickets([FromQuery] string? q)
        //{
        //    try
        //    {
        //        if (string.IsNullOrWhiteSpace(q))
        //        {
        //            var errorResponse = ApiResponse<object>.APIResponse(400, "Search query parameter 'q' is required.", null, false);
        //            return BadRequest(errorResponse);
        //        }

        //        var userIdClaim = User.FindFirst("userId")?.Value;
        //        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int currentUserId))
        //        {
        //            var errorResponse = ApiResponse<object>.APIResponse(400, "Invalid user context.", null, false);
        //            return BadRequest(errorResponse);
        //        }

        //        var result = await _ticketService.SearchTicketsGroupedAsync(q, currentUserId);

        //        var successResponse = ApiResponse<object>.APIResponse(200,
        //            "Tickets searched successfully",
        //            result,
        //            true
        //        );

        //        return Ok(successResponse);
        //    }
        //    catch (ArgumentException ex)
        //    {
        //        var errorResponse = ApiResponse<object>.APIResponse(400, ex.Message, null, false);
        //        return BadRequest(errorResponse);
        //    }
        //    catch (Exception ex)
        //    {
        //        var errorResponse = ApiResponse<object>.APIResponse(500, "Internal server error.", null, false);
        //        return BadRequest(errorResponse);
        //    }
        //}

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTicket(int id)
        {
            var userIdResult = GetCurrentUserId();
            if (!userIdResult.IsSuccess)
            {
                return FromResult(userIdResult);
            }

            var result = await _ticketService.DeleteTicketAsync(id, userIdResult.Data);
            return FromResult(result);
        }

        [HttpPatch("{id}")]
        public async Task<IActionResult> UpdateTicket(int id, [FromBody] UpdateTicketDto dto)
        {
            var userIdResult = GetCurrentUserId();
            if (!userIdResult.IsSuccess)
            {
                return FromResult(userIdResult);
            }

            var result = await _ticketService.UpdateTicketAsync(id, dto, userIdResult.Data);
            return FromResult(result);
        }

        [Authorize]
        [HttpPost("FilterTickets")]
        public async Task<IActionResult> FilterTickets([FromBody] TicketFilterDto filter)
        {
            var userIdResult = GetCurrentUserId();
            if (!userIdResult.IsSuccess)
            {
                return FromResult(userIdResult);
            }

            var result = await _ticketService.FilterTicketsAsync(filter ?? new TicketFilterDto(), userIdResult.Data);
            return FromResult(result);
        }

        [HttpPost("{ticketId}/attachments")]
        [Authorize(Roles = "Admin,Employee")]
        public async Task<IActionResult> UploadAttachments(int ticketId, [FromForm] TicketAttachmentRequestDto dto)
        {
            var userIdResult = GetCurrentUserId();
            if (!userIdResult.IsSuccess)
            {
                return FromResult(userIdResult);
            }

            var result = await _ticketService.UploadFilesAsync(dto, userIdResult.Data, ticketId);
            return FromResult(result);
        }


        [HttpGet("attachments/{attachmentId}")]
        [Authorize(Roles = "Admin,Employee")]
        public async Task<IActionResult> GetAttachment(int attachmentId)
        {
            var result = await _ticketService.GetAttachmentAsync(attachmentId);
            return FromResult(result);
        }

        [HttpGet("attachments/{attachmentId}/download")]
        [Authorize(Roles = "Admin,Employee")]
        public async Task<IActionResult> DownloadAttachment(int attachmentId)
        {
            var result = await _ticketService.GetAttachmentAsync(attachmentId);
            if (!result.IsSuccess)
            {
                return FromResult(result);
            }
            var fullPath = Path.Combine(_env.WebRootPath, result.Data!.FilePath.TrimStart('/'));
            var fileName = Path.GetFileName(fullPath);
            var provider = new FileExtensionContentTypeProvider();
            provider.TryGetContentType(fullPath, out var contentType);
            if (string.IsNullOrEmpty(contentType))
            {
                contentType = "application/octet-stream";
            }
            return File(System.IO.File.OpenRead(fullPath), contentType, fileName, enableRangeProcessing: true);
        }

        [HttpGet("attachments/{attachmentId}/view")]
        [Authorize(Roles = "Admin,Employee")]
        public async Task<IActionResult> ViewAttachment(int attachmentId)
        {
            var result = await _ticketService.GetAttachmentAsync(attachmentId);
            if (!result.IsSuccess)
            {
                return FromResult(result);
            }
            var fullPath = Path.Combine(_env.WebRootPath, result.Data!.FilePath.TrimStart('/'));
            var provider = new FileExtensionContentTypeProvider();
            provider.TryGetContentType(fullPath, out var contentType);
            if (string.IsNullOrEmpty(contentType))
            {
                contentType = "application/octet-stream";
            }
            return PhysicalFile(fullPath, contentType, enableRangeProcessing: false);
        }
    }
}
