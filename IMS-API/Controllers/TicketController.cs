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

            var userId = userIdResult.Data!;
            var result = await _ticketService.CreateTicketAsync(dto, userId);
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

            var userId = userIdResult.Data!;
            var result = await _ticketService.AddCommentAsync(ticketId, commentText, userId);
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

            var userId = userIdResult.Data!;
            var result = await _ticketService.UpdateStatusAsync(ticketId, status, userId);
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

            var userId = userIdResult.Data!;

            var result = await _ticketService.GetAllTicketsAsync(userId);
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

            var userId = (int)userIdResult.Data!;
            var result = await _ticketService.GetTicketByIdAsync(ticketId, userId);
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


    }
}

