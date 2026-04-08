using IMS_Domain.Entities;
using IMS_Application.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IMS_Application.Interfaces
{
    public interface ITicketRepository
    {
        Task<List<Ticket>> GetTicketsForUserAsync(int userId, string roleName);
        Task<int> CreateTicketAsync(CreateTicketRequestDto dto, int createdBy);
        Task<Ticket> GetTicketByIdAsync(int ticketId);
        Task<List<Ticket>> SearchTicketsAsync(string query, int userId, string roleName);
        Task<TicketComment> AddCommentAsync(int ticketId, string commentText, int userId);
        Task UpdateStatusAsync(int ticketId, string status, int userId);
    }
}
