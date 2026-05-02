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

        Task<Result<bool>> DeleteTicketAsync(int ticketId, int deletedBy);
        Task<Result<TicketResponseDto>> UpdateTicketAsync(int id, UpdateTicketDto dto, int updatedBy);
        Task<Result<List<TicketResponseDto>>> FilterTicketsAsync(TicketFilterDto filter, int currentUserId);
        
        Task<Result<List<TicketAttachmentResponseDto>>> UploadFilesAsync(TicketAttachmentRequestDto dto, int userId, int ticketId);
        Task<Result<TicketAttachmentResponseDto>> GetAttachmentAsync(int attachmentId);
    }
}
