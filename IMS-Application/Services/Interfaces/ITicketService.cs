using IMS_Application.DTOs;
using IMS_Domain.Entities;

namespace IMS_Application.Services.Interfaces
{
    public interface ITicketService
    {
        Task<TicketResponseDto> CreateTicketAsync(CreateTicketRequestDto dto, int createdBy);
        Task<TicketCommentResponseDto> AddCommentAsync(int ticketId, AddTicketCommentRequestDto dto, int currentUserId);
        Task<UpdateTicketStatusResponseDto> UpdateStatusAsync(int ticketId, UpdateTicketStatusRequestDto dto, int currentUserId);
        Task<TicketResponseDto?> GetTicketByIdAsync(int ticketId, int currentUserId);

        Task<List<TicketResponseDto>> SearchTicketsGroupedAsync(string q, int currentUserId);
        Task<List<TicketResponseDto>> GetAllTicketsAsync(int currentUserId);
    }
}
