using IMS_Application.DTOs;
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
                .Include(t => t.Attachments.OrderByDescending(a => a.UploadedAt))
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
                .Include(t => t.Attachments)
                .AsNoTracking();

            query = roleName switch
            {
                LogicStrings.AdminRole => query,

                LogicStrings.SupportEngineerRole => query.Where(t =>
                    t.TicketAssignments.Any(a => a.assignedTo == userId && a.status == LogicStrings.Active)),

                _ => query.Where(t => t.CreatedBy == userId)
            };

            query = query.OrderByDescending(t => t.CreatedAt);
            return await query.ToListAsync();
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

            return await query.OrderByDescending(t => t.CreatedAt).ToListAsync();
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
