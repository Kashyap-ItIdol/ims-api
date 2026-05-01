using IMS_API.Controllers.Base;
using IMS_Application.Common.Constants;
using IMS_Application.Common.Models;
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

            var result = await _ticketService.AddCommentAsync(ticketId, commentText, userIdResult.Data);
            return FromResult(result);
        }

        [HttpPost("{ticketId}/comments/{parentCommentId}/replies")]
        public async Task<IActionResult> AddReply(int ticketId, int parentCommentId, [FromQuery] string commentText)
        {
            var userIdResult = GetCurrentUserId();
            if (!userIdResult.IsSuccess)
            {
                return FromResult(userIdResult);
            }

            var result = await _ticketService.AddReplyAsync(ticketId, parentCommentId, commentText, userIdResult.Data);
            return FromResult(result);
        }

        [HttpPatch("comments/{commentId}")]
        public async Task<IActionResult> EditComment(int commentId, [FromQuery] string commentText)
        {
            var userIdResult = GetCurrentUserId();
            if (!userIdResult.IsSuccess)
            {
                return FromResult(userIdResult);
            }

            var result = await _ticketService.EditCommentAsync(commentId, commentText, userIdResult.Data);
            return FromResult(result);
        }

        [HttpDelete("comments/{commentId}")]
        public async Task<IActionResult> DeleteComment(int commentId)
        {
            var userIdResult = GetCurrentUserId();
            if (!userIdResult.IsSuccess)
            {
                return FromResult(userIdResult);
            }

            var result = await _ticketService.DeleteCommentAsync(commentId, userIdResult.Data);
            return FromResult(result);
        }

        [HttpPost("comments/{commentId}/like")]
        public async Task<IActionResult> LikeComment(int commentId)
        {
            var userIdResult = GetCurrentUserId();
            if (!userIdResult.IsSuccess)
            {
                return FromResult(userIdResult);
            }

            var result = await _ticketService.LikeCommentAsync(commentId, userIdResult.Data);
            return FromResult(result);
        }

        [HttpDelete("comments/{commentId}/like")]
        public async Task<IActionResult> UnlikeComment(int commentId)
        {
            var userIdResult = GetCurrentUserId();
            if (!userIdResult.IsSuccess)
            {
                return FromResult(userIdResult);
            }

            var result = await _ticketService.UnlikeCommentAsync(commentId, userIdResult.Data);
            return FromResult(result);
        }

        [HttpPost("comments/{commentId}/reactions")]
        public async Task<IActionResult> AddReaction(int commentId, [FromQuery] string reactionType)
        {
            var userIdResult = GetCurrentUserId();
            if (!userIdResult.IsSuccess)
            {
                return FromResult(userIdResult);
            }

            var result = await _ticketService.AddReactionAsync(commentId, reactionType, userIdResult.Data);
            return FromResult(result);
        }

        [HttpDelete("comments/{commentId}/reactions")]
        public async Task<IActionResult> RemoveReaction(int commentId)
        {
            var userIdResult = GetCurrentUserId();
            if (!userIdResult.IsSuccess)
            {
                return FromResult(userIdResult);
            }

            var result = await _ticketService.RemoveReactionAsync(commentId, userIdResult.Data);
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


        [HttpGet("search")]
        public async Task<IActionResult> SearchTickets([FromQuery] string? q)
        {
            var userIdResult = GetCurrentUserId();
            if (!userIdResult.IsSuccess)
            {
                return FromResult(userIdResult);
            }

            var result = await _ticketService.SearchTicketsGroupedAsync(q, userIdResult.Data);
            return FromResult(result);
        }
    }
}
