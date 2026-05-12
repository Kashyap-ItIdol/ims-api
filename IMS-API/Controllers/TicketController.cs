using IMS_API.Controllers.Base;
using IMS_Application.DTOs;
using IMS_Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IMS_API.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    public class TicketController : BaseController
    {
        private readonly ITicketService _ticketService;

        public TicketController(ITicketService ticketService)
        {
            _ticketService = ticketService;
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

    }
}
