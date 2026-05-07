using IMS_Application.Interfaces;
using IMS_Domain.Entities;
using IMS_Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using IMS_Application.Common.Constants;

namespace IMS_Infrastructure.Repositories
{
    public class TicketRepository : Repository<Ticket>, ITicketRepository
    {
        public TicketRepository(AppDbContext context) : base(context)
        {
        }

        public async Task AddTicketAsync(Ticket ticket)
        {
            await _dbSet.AddAsync(ticket);
        }

        public async Task AddCommentAsync(TicketComment comment)
        {
            await _context.TicketComments.AddAsync(comment);
        }

        public async Task<TicketComment?> GetCommentByIdAsync(int commentId)
        {
            return await _context.TicketComments
                .Include(c => c.Likes)
                .Include(c => c.Reactions)
                .Include(c => c.Replies)
                .FirstOrDefaultAsync(c => c.Id == commentId);
        }

        public Task UpdateCommentAsync(TicketComment comment)
        {
            _context.TicketComments.Update(comment);
            return Task.CompletedTask;
        }

        public Task DeleteCommentAsync(TicketComment comment)
        {
            _context.TicketComments.Update(comment);
            return Task.CompletedTask;
        }

        public async Task AddCommentLikeAsync(TicketCommentLike like)
        {
            await _context.TicketCommentLikes.AddAsync(like);
        }

        public async Task<TicketCommentLike?> GetCommentLikeAsync(int commentId, int userId)
        {
            return await _context.TicketCommentLikes
                .FirstOrDefaultAsync(l => l.CommentId == commentId && l.UserId == userId);
        }

        public Task UpdateCommentLikeAsync(TicketCommentLike like)
        {
            _context.TicketCommentLikes.Update(like);
            return Task.CompletedTask;
        }

        public async Task<TicketCommentReaction?> GetCommentReactionAsync(int commentId, int userId)
        {
            return await _context.TicketCommentReactions
                .FirstOrDefaultAsync(r => r.CommentId == commentId && r.UserId == userId);
        }

        public async Task AddCommentReactionAsync(TicketCommentReaction reaction)
        {
            await _context.TicketCommentReactions.AddAsync(reaction);
        }

        public Task UpdateCommentReactionAsync(TicketCommentReaction reaction)
        {
            _context.TicketCommentReactions.Update(reaction);
            return Task.CompletedTask;
        }

        public async Task AddTicketStatusHistoryAsync(TicketStatusHistory history)
        {
            await _context.TicketStatusHistories.AddAsync(history);
        }

        public async Task UpdateTicketStatusAsync(Ticket ticket, Status newStatus, int changedBy)
        {
            ticket.Status = newStatus;
            ticket.UpdatedAt = DateTime.UtcNow;
            _dbSet.Update(ticket);
        }

        public async Task<Ticket?> GetTicketByIdAsync(int ticketId)
        {
            return await _dbSet
                .AsNoTracking()
                .Include(t => t.Comments.OrderByDescending(c => c.CreatedAt))
                    .ThenInclude(c => c.Likes)
                .Include(t => t.Comments.OrderByDescending(c => c.CreatedAt))
                    .ThenInclude(c => c.Reactions)
                .Include(t => t.Comments.OrderByDescending(c => c.CreatedAt))
                    .ThenInclude(c => c.Replies)
                    .ThenInclude(r => r.Likes)
                .Include(t => t.Comments.OrderByDescending(c => c.CreatedAt))
                    .ThenInclude(c => c.Replies)
                    .ThenInclude(r => r.Reactions)
                .Include(t => t.TicketAssignments.OrderByDescending(a => a.assigned_at))
                .Include(t => t.TicketStatusHistories.OrderByDescending(h => h.ChangedAt))
                .FirstOrDefaultAsync(t => t.Id == ticketId);
        }

        public async Task<List<Ticket>> GetTicketsForUserAsync(int userId, string roleName)
        {
            var query = _dbSet
                .Include(t => t.TicketAssignments)
                .Include(t => t.Comments)
                    .ThenInclude(c => c.Likes)
                .Include(t => t.Comments)
                    .ThenInclude(c => c.Reactions)
                .Include(t => t.Comments)
                    .ThenInclude(c => c.Replies)
                    .ThenInclude(r => r.Likes)
                .Include(t => t.Comments)
                    .ThenInclude(c => c.Replies)
                    .ThenInclude(r => r.Reactions)
                .AsNoTracking();

            query = roleName switch
            {
                LogicStrings.AdminRole => query,
                LogicStrings.SupportEngineerRole => query.Where(t => t.TicketAssignments.Any(a => a.assignedTo == userId && a.status == LogicStrings.Active)),
                _ => query.Where(t => t.CreatedBy == userId)
            };

            return await query.OrderByDescending(t => t.CreatedAt).ToListAsync();
        }
    }
}
