using IMS_API.Controllers.Base;
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
        public async Task<IActionResult> CreateTicket(CreateTicketRequestDto dto)
        {
            var userIdResult = GetCurrentUserId();
            if (!userIdResult.IsSuccess)
            {
                return FromResult(userIdResult);
            }

            return FromResult(await _ticketService.CreateTicketAsync(dto, userIdResult.Data));
        }

        [HttpPost("{ticketId}/comments")]
        public async Task<IActionResult> AddComment(int ticketId, [FromQuery] string commentText)
        {
            var userIdResult = GetCurrentUserId();
            if (!userIdResult.IsSuccess)
            {
                return FromResult(userIdResult);
            }

            return FromResult(await _ticketService.AddCommentAsync(ticketId, commentText, userIdResult.Data));
        }

        [HttpPost("{ticketId}/comments/{parentCommentId}/replies")]
        public async Task<IActionResult> AddReply(int ticketId, int parentCommentId, [FromQuery] string commentText)
        {
            var userIdResult = GetCurrentUserId();
            if (!userIdResult.IsSuccess)
            {
                return FromResult(userIdResult);
            }
            return FromResult(await _ticketService.AddReplyAsync(ticketId, parentCommentId, commentText, userIdResult.Data));
        }

        [HttpPatch("comments/{commentId}")]
        public async Task<IActionResult> EditComment(int commentId, [FromQuery] string commentText)
        {
            var userIdResult = GetCurrentUserId();
            if (!userIdResult.IsSuccess)
            {
                return FromResult(userIdResult);
            }
            return FromResult(await _ticketService.EditCommentAsync(commentId, commentText, userIdResult.Data));
        }

        [HttpDelete("comments/{commentId}")]
        public async Task<IActionResult> DeleteComment(int commentId)
        {
            var userIdResult = GetCurrentUserId();
            if (!userIdResult.IsSuccess)
            {
                return FromResult(userIdResult);
            }
            return FromResult(await _ticketService.DeleteCommentAsync(commentId, userIdResult.Data));
        }

        [HttpPost("comments/{commentId}/like")]
        public async Task<IActionResult> LikeComment(int commentId)
        {
            var userIdResult = GetCurrentUserId();
            if (!userIdResult.IsSuccess)
            {
                return FromResult(userIdResult);
            }
            return FromResult(await _ticketService.LikeCommentAsync(commentId, userIdResult.Data));
        }

        [HttpDelete("comments/{commentId}/like")]
        public async Task<IActionResult> UnlikeComment(int commentId)
        {
            var userIdResult = GetCurrentUserId();
            if (!userIdResult.IsSuccess)
            {
                return FromResult(userIdResult);
            }
            return FromResult(await _ticketService.UnlikeCommentAsync(commentId, userIdResult.Data));
        }

        [HttpPost("comments/{commentId}/reactions")]
        public async Task<IActionResult> AddReaction(int commentId, [FromQuery] string reactionType)
        {
            var userIdResult = GetCurrentUserId();
            if (!userIdResult.IsSuccess)
            {
                return FromResult(userIdResult);
            }
            return FromResult(await _ticketService.AddReactionAsync(commentId, reactionType, userIdResult.Data));
        }

        [HttpDelete("comments/{commentId}/reactions")]
        public async Task<IActionResult> RemoveReaction(int commentId)
        {
            var userIdResult = GetCurrentUserId();
            if (!userIdResult.IsSuccess)
            {
                return FromResult(userIdResult);
            }
            return FromResult(await _ticketService.RemoveReactionAsync(commentId, userIdResult.Data));
        }

        [HttpPatch("{ticketId}/status")]
        public async Task<IActionResult> UpdateStatus(int ticketId, [FromQuery] string status)
        {
            var userIdResult = GetCurrentUserId();
            if (!userIdResult.IsSuccess)
            {
                return FromResult(userIdResult);
            }
            return FromResult(await _ticketService.UpdateStatusAsync(ticketId, status, userIdResult.Data));
        }

        [HttpGet]
        public async Task<IActionResult> GetTickets([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            var userIdResult = GetCurrentUserId();
            if (!userIdResult.IsSuccess)
            {
                return FromResult(userIdResult);
            }
            return FromResult(await _ticketService.GetAllTicketsAsync(userIdResult.Data, pageNumber, pageSize));
        }

        [HttpGet("{ticketId}")]
        public async Task<IActionResult> GetTicket(int ticketId)
        {
            var userIdResult = GetCurrentUserId();
            if (!userIdResult.IsSuccess)
            {
                return FromResult(userIdResult);
            }
            return FromResult(await _ticketService.GetTicketByIdAsync(ticketId, userIdResult.Data));
        }

        [HttpGet("search")]
        public async Task<IActionResult> SearchTickets([FromQuery] string? q)
        {
            var userIdResult = GetCurrentUserId();
            if (!userIdResult.IsSuccess)
            {
                return FromResult(userIdResult);
            }
            return FromResult(await _ticketService.SearchTicketsGroupedAsync(q, userIdResult.Data));
        }

        [HttpGet("thismonth")]
        public async Task<IActionResult> GetCalendarFilteredTickets([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10, [FromQuery] string? dateFilter = null, [FromQuery] DateOnly? startDate = null, [FromQuery] DateOnly? endDate = null)

        {

            var userIdResult = GetCurrentUserId();
            if (!userIdResult.IsSuccess)
            {
                return FromResult(userIdResult);
            }
            return FromResult(await _ticketService.GetCalendarFilteredTicketsAsync(userIdResult.Data, pageNumber, pageSize, dateFilter, startDate, endDate));
        }
        [HttpGet("support-engineers")]
        public async Task<IActionResult> GetSupportEngineers([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            var userIdResult = GetCurrentUserId();
            if (!userIdResult.IsSuccess)
            {
                return FromResult(userIdResult);
            }
            return FromResult(await _ticketService.GetSupportEngineersAsync(pageNumber, pageSize));
        }
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
