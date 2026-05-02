using IMS_Application.DTOs;
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

        public async Task<Ticket> GetTicketByIdAsync(int ticketId)
        {
            return await _dbSet
                .AsNoTracking()
                .Include(t => t.Comments.OrderByDescending(c => c.CreatedAt))
                .Include(t => t.TicketAssignments.OrderByDescending(a => a.assigned_at))
                .Include(t => t.TicketStatusHistories.OrderByDescending(h => h.ChangedAt))
                .Include(t => t.Attachments.OrderByDescending(a => a.UploadedAt))
                .Include(t => t.Category)
                .Include(t => t.SubCategory)
                .FirstOrDefaultAsync(t => t.Id == ticketId);
        }

        public async Task<List<Ticket>> GetTicketsForUserAsync(int userId, string roleName)
        {
            var query = _dbSet
                .Include(t => t.TicketAssignments)
                .Include(t => t.Comments)
                .Include(t => t.Attachments)
                .Include(t => t.Category)
                .Include(t => t.SubCategory)
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
                .Include(t => t.Attachments)
                .Include(t => t.Category)
                .Include(t => t.SubCategory)
                .Where(t => t.CreatedBy == userId || t.TicketAssignments.Any(a => a.assignedTo == userId));

            if (!string.IsNullOrEmpty(query))
            {
                ticketsQuery = ticketsQuery.Where(t => t.Title.Contains(query, StringComparison.OrdinalIgnoreCase));
            }
            ticketsQuery = ticketsQuery.OrderByDescending(t => t.CreatedAt);
            return await ticketsQuery.ToListAsync();
        }

        public async Task<bool> DeleteTicketAsync(int ticketId, int deletedBy)
        {
            var ticket = await _dbSet
                .Include(t => t.Comments)
                .Include(t => t.Attachments)
                .Include(t => t.TicketAssignments)
                .FirstOrDefaultAsync(t => t.Id == ticketId && !t.IsDeleted);

            if (ticket == null)
                return false;

            ticket.IsDeleted = true;
            ticket.DeletedBy = deletedBy;
            ticket.DeletedAt = DateTime.UtcNow;

            _dbSet.Update(ticket);
            return true;
        }

        public async Task<Ticket> UpdateTicketAsync(int ticketId, UpdateTicketDto dto)
        {
            var ticket = await _dbSet
                .Include(t => t.Category)
                .Include(t => t.SubCategory)
                .Include(t => t.Attachments)
                .FirstOrDefaultAsync(t => t.Id == ticketId && !t.IsDeleted);

            if (ticket == null)
                return null;

            if (!string.IsNullOrWhiteSpace(dto.TicketTitle))
                ticket.Title = dto.TicketTitle.Trim();

            if (dto.Description != null)
                ticket.Description = dto.Description;

            if (!string.IsNullOrWhiteSpace(dto.TicketType) &&
                Enum.TryParse<TicketType>(dto.TicketType, true, out var ticketType))
            {
                ticket.TicketType = ticketType;
            }

            if (!string.IsNullOrWhiteSpace(dto.TicketPriority) &&
                Enum.TryParse<TicketPriority>(dto.TicketPriority, true, out var priority))
            {
                ticket.TicketPriority = priority;
            }

            if (dto.AssetId.HasValue)
                ticket.AssetId = dto.AssetId.Value;

            if (dto.CategoryId.HasValue)
                ticket.CategoryId = dto.CategoryId.Value;

            if (dto.SubCategoryId.HasValue)
                ticket.SubCategoryId = dto.SubCategoryId.Value;

            ticket.UpdatedAt = DateTime.UtcNow;

            _dbSet.Update(ticket);
            return ticket;
        }

        public async Task<List<Ticket>> FilterTicketsAsync(TicketFilterDto filter)
        {
            var query = _dbSet
                .Include(t => t.Category)
                .Include(t => t.SubCategory)
                .Include(t => t.Attachments)
                .AsNoTracking().Where(x => !x.IsDeleted);

            if (filter.Status != null && filter.Status.Any())
            {
                var statuses = filter.Status
                    .Where(s => Enum.TryParse<Status>(s, true, out _))
                    .Select(s => Enum.Parse<Status>(s, true))
                    .ToList();

                if (statuses.Any())
                    query = query.Where(x => statuses.Contains(x.Status));
            }

            if (filter.TicketPriority != null && filter.TicketPriority.Any())
            {
                var priorities = filter.TicketPriority
                    .Where(p => Enum.TryParse<TicketPriority>(p, true, out _))
                    .Select(p => Enum.Parse<TicketPriority>(p, true))
                    .ToList();

                if (priorities.Any())
                    query = query.Where(x => priorities.Contains(x.TicketPriority));
            }

            if (filter.TicketType != null && filter.TicketType.Any())
            {
                var types = filter.TicketType
                    .Where(t => Enum.TryParse<TicketType>(t, true, out _))
                    .Select(t => Enum.Parse<TicketType>(t, true))
                    .ToList();

                if (types.Any())
                    query = query.Where(x => types.Contains(x.TicketType));
            }

            if (filter.FromDate.HasValue)
                query = query.Where(t => t.CreatedAt.Date >= filter.FromDate.Value.Date);

            if (filter.ToDate.HasValue)
                query = query.Where(t => t.CreatedAt.Date <= filter.ToDate.Value.Date);

            query = query.OrderByDescending(x => x.UpdatedAt);
            return await query.ToListAsync();
        }
        public async Task AddAttachmentAsync(TicketAttachment attachment)
        {
            await _context.TicketAttachments.AddAsync(attachment);
        }

        public async Task<TicketAttachment?> GetAttachmentByIdAsync(int attachmentId)
        {
            return await _context.TicketAttachments
                .FirstOrDefaultAsync(a => a.Id == attachmentId);
        }


    }
}
