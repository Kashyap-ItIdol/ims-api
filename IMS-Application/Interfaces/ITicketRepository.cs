using IMS_Domain.Entities;

namespace IMS_Application.Interfaces
{
    public interface ITicketRepository : IRepository<Ticket>
    {
        Task<List<Ticket>> GetTicketsForUserAsync(int userId, string roleName);
        Task AddTicketAsync(Ticket ticket);
        Task<Ticket> GetTicketByIdAsync(int ticketId);
        Task AddCommentAsync(TicketComment comment);
        Task<TicketComment?> GetCommentByIdAsync(int commentId);
        Task UpdateCommentAsync(TicketComment comment);
        Task DeleteCommentAsync(TicketComment comment);
        Task AddCommentLikeAsync(TicketCommentLike like);
        Task<TicketCommentLike?> GetCommentLikeAsync(int commentId, int userId);
        Task UpdateCommentLikeAsync(TicketCommentLike like);
        Task<TicketCommentReaction?> GetCommentReactionAsync(int commentId, int userId);
        Task AddCommentReactionAsync(TicketCommentReaction reaction);
        Task UpdateCommentReactionAsync(TicketCommentReaction reaction);
        Task AddTicketStatusHistoryAsync(TicketStatusHistory history);
        Task UpdateTicketStatusAsync(Ticket ticket, Status newStatus, int changedBy);
    }
}
