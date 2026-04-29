
using IMS_Domain.Entities;

namespace IMS_Application.Interfaces
{
    public interface ITicketRepository : IRepository<Ticket>
    {
        Task<List<Ticket>> GetTicketsForUserAsync(int userId, string roleName);
        Task AddTicketAsync(Ticket ticket);
        Task<Ticket> GetTicketByIdAsync(int ticketId);
        Task<List<Ticket>> SearchTicketsAsync(string query, int userId, string roleName);
        Task AddCommentAsync(TicketComment comment);
        Task AddTicketStatusHistoryAsync(TicketStatusHistory history);
        Task UpdateTicketStatusAsync(Ticket ticket, Status newStatus, int changedBy);
    }
}
