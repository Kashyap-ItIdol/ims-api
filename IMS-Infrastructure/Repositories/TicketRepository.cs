using IMS_Application.Interfaces;
using IMS_Domain.Entities;
using IMS_Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

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
                .Include(t => t.TicketAssignments.OrderByDescending(a => a.assigned_at))
                .Include(t => t.TicketStatusHistories.OrderByDescending(h => h.ChangedAt))
                .FirstOrDefaultAsync(t => t.Id == ticketId);
        }

        public async Task<List<Ticket>> GetTicketsForUserAsync(int userId, string roleName)
        {
            var query = _dbSet
                .Include(t => t.TicketAssignments)
                .Include(t => t.Comments)
                .Where(t => t.CreatedBy == userId || t.TicketAssignments.Any(a => a.assignedTo == userId))
                .OrderByDescending(t => t.CreatedAt)
                .AsNoTracking();

            return await query.ToListAsync();
        }

        public async Task<List<Ticket>> SearchTicketsAsync(string query, int userId, string roleName)
        {
            var ticketsQuery = _dbSet
                .AsNoTracking()
                .Include(t => t.TicketAssignments)
                .Include(t => t.Comments)
                .Where(t => t.CreatedBy == userId || t.TicketAssignments.Any(a => a.assignedTo == userId));

            if (!string.IsNullOrEmpty(query))
            {
                ticketsQuery = ticketsQuery.Where(t => t.Title.Contains(query, StringComparison.OrdinalIgnoreCase));
            }

            return await ticketsQuery.OrderByDescending(t => t.CreatedAt).ToListAsync();
        }
    }
}
