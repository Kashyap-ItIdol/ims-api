using IMS_Application.Common.Constants;
using IMS_Application.Common.Models;
using IMS_Application.DTOs;
using IMS_Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using IMS_API.Controllers.Base;

namespace IMS_API.Controllers
{
    [Route("api/[controller]")]
    [Authorize] // Require auth to get CreatedBy from claims
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
            try
            {
                var userIdClaim = User.FindFirst("userId")?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int createdBy))
                {
                    var errorResponse = Result<object>.Failure(ErrorMessages.InvalidCredentials, 400);
                    return BadRequest(errorResponse);
                }

                var result = await _ticketService.CreateTicketAsync(dto, createdBy);

                var successResponse = Result<TicketResponseDto>.Success(null,
                   SuccessMessages.TicketCreated
                   
                );

                return FromResult(successResponse);
            }
            catch (ArgumentException ex)
            {
                var errorResponse = Result<object>.Failure(ex.Message, 400);
                return BadRequest(errorResponse);
            }
            catch (Exception ex)
            {
                var errorResponse = Result<object>.Failure(ErrorMessages.ServerError, 500);
                return BadRequest(errorResponse);
            }
        }

        [HttpPost("{ticketId}/comments")]
        public async Task<IActionResult> AddComment(int ticketId, [FromBody] AddTicketCommentRequestDto dto)
        {
            try
            {
                var userIdClaim = User.FindFirst("userId")?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int currentUserId))
                {
                    var errorResponse = Result<object>.Failure(ErrorMessages.InvalidCredentials,400);
                    return BadRequest(errorResponse);
                }

                if (string.IsNullOrWhiteSpace(dto.CommentText))
                {
                    var errorResponse = Result<object>.Failure(ErrorMessages.CommentRequires, 400);
                    return BadRequest(errorResponse);
                }

                var result = await _ticketService.AddCommentAsync(ticketId, dto, currentUserId);

                var successResponse = Result<TicketCommentResponseDto>.Success(result,
                    SuccessMessages.CommentCreated
                );

                return FromResult(successResponse);

            }
            catch (ArgumentException ex)
            {
                var errorResponse = Result<object>.Failure( ex.Message, 400);
                return ticketId == 0 ? NotFound(errorResponse) : BadRequest(errorResponse);
            }
            catch (Exception ex)
            {
                var errorResponse = Result<object>.Failure(ErrorMessages.ServerError, 500);
                return BadRequest(errorResponse);
            }
        }

        [HttpPatch("{ticketId}/status")]
        public async Task<IActionResult> UpdateStatus(int ticketId, [FromBody] UpdateTicketStatusRequestDto dto)
        {
            try
            {
                var userIdClaim = User.FindFirst("userId")?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int currentUserId))
                {
                    var errorResponse = Result<object>.Failure(ErrorMessages.InvalidCredentials, 400); 
                    return BadRequest(errorResponse);
                }

                var result = await _ticketService.UpdateStatusAsync(ticketId, dto, currentUserId);

                var successResponse = Result<UpdateTicketStatusResponseDto>.Success(result,
                   SuccessMessages.StatusUpdated
                );

                return FromResult(successResponse);
            }
            catch (ArgumentException ex)
            {
                var errorResponse = Result<object>.Failure( ex.Message, 400);
                return BadRequest(errorResponse);
            }
            catch (Exception ex)
            {
                var errorResponse = Result<object>.Failure(ErrorMessages.ServerError, 500);
                return BadRequest(errorResponse);
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetTickets()
        {
            try
            {
                var userIdClaim = User.FindFirst("userId")?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int currentUserId))
                {
                    var errorResponse = Result<object>.Failure(ErrorMessages.InvalidCredentials, 400);
                    return BadRequest(errorResponse);
                }

                var result = await _ticketService.GetAllTicketsAsync(currentUserId);

                var successResponse = Result<object>.Success(result,
                    SuccessMessages.AllTickets
                );

                return FromResult(successResponse);
            }
            catch (ArgumentException ex)
            {
                var errorResponse = Result<object>.Failure(ex.Message, 400);
                return BadRequest(errorResponse);
            }
            catch (Exception ex)
            {
                var errorResponse = Result<object>.Failure(ErrorMessages.ServerError, 500);
                return BadRequest(errorResponse);
            }
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

        [HttpGet("{ticketId}")]
        public async Task<IActionResult> GetTicket(int ticketId)
        {
            try
            {
                var userIdClaim = User.FindFirst("userId")?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int currentUserId))
                {
                    var errorResponse = Result<object>.Failure(ErrorMessages.InvalidCredentials, 400); 
                    return BadRequest(errorResponse);
                }

                var result = await _ticketService.GetTicketByIdAsync(ticketId, currentUserId);
                if (result == null)
                {
                    var errorResponse = Result<object>.Failure(ErrorMessages.TicketIdNotExist, 400);
                    return NotFound(errorResponse);
                }

                var successResponse = Result<object>.Success(result,
                   SuccessMessages.AllTickets
                );

                return FromResult(successResponse);
            }
            catch (ArgumentException ex)
            {
                var errorResponse = Result<object>.Failure(ex.Message, 400);
                return BadRequest(errorResponse);
            }
            catch (Exception ex)
            {
                var errorResponse = Result<object>.Failure(ErrorMessages.ServerError, 500);
                return BadRequest(errorResponse);
            }
        }
    }
}
