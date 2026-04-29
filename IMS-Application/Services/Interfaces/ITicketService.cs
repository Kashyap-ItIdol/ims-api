using IMS_Application.Common.Models;
using IMS_Application.DTOs;

namespace IMS_Application.Services.Interfaces
{
    public interface ITicketService
    {
        Task<Result<TicketResponseDto>> CreateTicketAsync(CreateTicketRequestDto dto, int createdBy);
        Task<Result<TicketCommentResponseDto>> AddCommentAsync(int ticketId, string commentText, int currentUserId);
        Task<Result<UpdateTicketStatusResponseDto>> UpdateStatusAsync(int ticketId, string status, int currentUserId);
        Task<Result<List<TicketResponseDto>>> GetAllTicketsAsync(int currentUserId);
        Task<Result<TicketResponseDto>> GetTicketByIdAsync(int ticketId, int currentUserId);
        Task<Result<List<TicketResponseDto>>> SearchTicketsGroupedAsync(string q, int currentUserId);

    }
}
