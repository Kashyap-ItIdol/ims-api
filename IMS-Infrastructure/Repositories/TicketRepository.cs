using AutoMapper;
using AutoMapper.QueryableExtensions;
using IMS_Application.DTOs;
using IMS_Application.Interfaces;
using IMS_Domain.Entities;
using Microsoft.EntityFrameworkCore;
using IMS_Infrastructure.Data;

namespace IMS_Infrastructure.Repositories
{
    public class TicketRepository : ITicketRepository
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public TicketRepository(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task AddTicketAsync(CreateTicketRequestDto request, int currentUserId)
        {
            
            TicketType ticketType;
            if (!Enum.TryParse<TicketType>(request.TicketType, true, out ticketType))
                throw new ArgumentException($"Invalid TypeName: {request.TicketType}"); 

            TicketPriority ticketPriority;
            if (!Enum.TryParse<TicketPriority>(request.Priority, true, out ticketPriority))
                throw new ArgumentException($"Invalid PriorityName: {request.Priority}"); 

            var now = DateTime.UtcNow;
            var ticket = new Ticket
            {
                Title = request.Title,
                Description = request.Description ?? string.Empty,
                TicketType = ticketType,
                TicketPriority = ticketPriority,
                CreatedBy = currentUserId,
                AssetId = request.AssetId,
                CreatedAt = now,
                UpdatedAt = now,
                Status = Status.Open // Default
            };
            _context.Tickets.Add(ticket);
            await _context.SaveChangesAsync();

            var assignment = new TicketAssignment
            {
                TicketId = ticket.Id,
                assignedTo = request.assignedTo, 
                assigned_by = currentUserId,
                assigned_at = now,
                status = "Active"
            };
            _context.TicketAssignments.Add(assignment);
            await _context.SaveChangesAsync();
        }



        public async Task UpdateTicketStatusAsync(UpdateTicketStatusRequestDto request, int updatedBy)
        {
            var ticket = await _context.Tickets
                .Include(t => t.TicketAssignments)
                .FirstOrDefaultAsync(t => t.Id == request.TicketId);
            if (ticket == null)
            {
                throw new ArgumentException("Ticket not found.");
            }

            if (updatedBy != ticket.CreatedBy && 
                !ticket.TicketAssignments.Any(a => a.assignedTo == updatedBy && a.status == "Active"))
            {
                throw new ArgumentException("You are not authorized to update this ticket status.");
            }

            Status newStatus;
            if (!Enum.TryParse<Status>(request.Status, true, out newStatus))
                throw new ArgumentException($"Invalid status: {request.Status}.");

            var history = new TicketStatusHistory
            {
                TicketId = request.TicketId,
                OldStatusId = (int)ticket.Status,
                NewStatusId = (int)newStatus,
                ChangedBy = updatedBy,
                ChangedAt = DateTime.UtcNow
            };
            _context.TicketStatusHistories.Add(history);

            ticket.Status = newStatus;
            ticket.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
        }

        public async Task<List<Ticket>> GetAllTicketsAsync()
        {
            return await _context.Tickets
                .Include(t => t.TicketAssignments)
                .Include(t => t.Comments)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<Ticket>> GetTicketsByUserIdAsync(int userId)
        {
            return await _context.Tickets
                .Include(t => t.TicketAssignments)
                .Include(t => t.Comments)
                .Where(t => t.CreatedBy == userId || t.TicketAssignments.Any(a => a.assignedTo == userId))
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();
        }

        // public async Task<List<Ticket>> SearchTicketsAsync(IMS_Application.DTOs.TicketFilterDto filter, int pageNumber, int pageSize)
        // {
        //     var query = _context.Tickets
        //         .Include(t => t.TicketAssignments)
        //         .Include(t => t.Comments)
        //         .AsQueryable();

        //     if (!string.IsNullOrEmpty(filter.Title))
        //     {
        //         query = query.Where(t => t.Title.Contains(filter.Title));
        //     }

        //     // CategoryId filter disabled - Ticket lacks CategoryId property
        //     // if (filter.CategoryId.HasValue)
        //     // {
        //     //     query = query.Where(t => t.CategoryId == filter.CategoryId.Value);
        //     // }

        //     if (filter.StatusId.HasValue)
        //     {
        //         Status status = filter.StatusId.Value switch
        //         {
        //             1 => Status.Open,
        //             2 => Status.InProgress,
        //             3 => Status.Solved,
        //             4 => Status.Closed,
        //             _ => Status.Open
        //         };
        //         query = query.Where(t => t.Status == status);
        //     }

        //     if (filter.PriorityId.HasValue)
        //     {
        //         // Assume Priority entity mapping if exists
        //         query = query.Where(t => t.TicketPriority == (TicketPriority)(filter.PriorityId.Value - 1));
        //     }

        //     if (filter.FromDate.HasValue)
        //     {
        //         query = query.Where(t => t.CreatedAt >= filter.FromDate.Value);
        //     }

        //     if (filter.ToDate.HasValue)
        //     {
        //         query = query.Where(t => t.CreatedAt <= filter.ToDate.Value);
        //     }

        //     return await query
        //         .OrderByDescending(t => t.CreatedAt)
        //         .Skip((pageNumber - 1) * pageSize)
        //         .Take(pageSize)
        //         .ToListAsync();
        // }

        // Keep existing methods for compatibility
        public async Task<int> CreateTicketAsync(CreateTicketRequestDto dto, int createdBy)
        {
            await AddTicketAsync(dto, createdBy);
            
            return await _context.Tickets.MaxAsync(t => (int?)t.Id) ?? 0;
        }

        public async Task<TicketComment> AddCommentAsync(int ticketId, AddTicketCommentRequestDto dto, int currentUserId)
        {
            if (string.IsNullOrWhiteSpace(dto.CommentText))
            {
                throw new ArgumentException("Comment text cannot be empty.");
            }

            var ticket = await _context.Tickets
                .Include(t => t.TicketAssignments)
                .FirstOrDefaultAsync(t => t.Id == ticketId);
            if (ticket == null)
            {
                throw new ArgumentException("Ticket not found.");
            }

            if (currentUserId != ticket.CreatedBy && 
                !ticket.TicketAssignments.Any(a => a.assignedTo == currentUserId && a.status == "Active"))
            {
                throw new ArgumentException("You are not authorized to comment on this ticket.");
            }

            var comment = new TicketComment
            {
                TicketId = ticketId,
                UserId = currentUserId,
                CommentText = dto.CommentText,
                CreatedAt = DateTime.UtcNow
            };

            _context.TicketComments.Add(comment);
            await _context.SaveChangesAsync();

            return comment;
        }

        public async Task UpdateStatusAsync(int ticketId, UpdateTicketStatusRequestDto dto, int currentUserId)
        {
            var ticket = await _context.Tickets
                .Include(t => t.TicketAssignments)
                .FirstOrDefaultAsync(t => t.Id == ticketId);
            if (ticket == null)
            {
                throw new ArgumentException("Ticket not found.");
            }

            if (currentUserId != ticket.CreatedBy && 
                !ticket.TicketAssignments.Any(a => a.assignedTo == currentUserId && a.status == "Active"))
            {
                throw new ArgumentException("You are not authorized to update this ticket status.");
            }

            Status newStatus;
            if (!Enum.TryParse<Status>(dto.Status, true, out newStatus))
                throw new ArgumentException($"Invalid status: {dto.Status}.");

            var history = new TicketStatusHistory
            {
                TicketId = ticketId,
                OldStatusId = (int)ticket.Status,
                NewStatusId = (int)newStatus,
                ChangedBy = currentUserId,
                ChangedAt = DateTime.UtcNow
            };
            _context.TicketStatusHistories.Add(history);

            ticket.Status = newStatus;
            ticket.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
        }

        public async Task<Ticket> GetTicketByIdAsync(int ticketId)
        {
            var ticket = await _context.Tickets
                .Include(t => t.Comments.OrderByDescending(c => c.CreatedAt))
                .Include(t => t.TicketAssignments.OrderByDescending(a => a.assigned_at))
                .Include(t => t.TicketStatusHistories.OrderByDescending(h => h.ChangedAt))
                .FirstOrDefaultAsync(t => t.Id == ticketId);

            return ticket ?? throw new ArgumentException("Ticket not found");
        }

        public async Task<List<Ticket>> GetTicketsForUserAsync(int userId, string roleName)
        {
            if (roleName == "Admin")
            {
                return await GetAllTicketsAsync();
            }
            else if (roleName == "Support Engineer")
            {
                return await _context.Tickets
                    .Include(t => t.TicketAssignments)
                    .Include(t => t.Comments)
                    .Where(t => t.TicketAssignments.Any(a => a.assignedTo == userId && a.status == "Active"))
                    .OrderByDescending(t => t.CreatedAt)
                    .ToListAsync();
            }
            else
            {
                return await GetTicketsByUserIdAsync(userId);
            }
        }

        public async Task<List<Ticket>> SearchTicketsAsync(string query, int userId, string roleName)
        {
            var tickets = await GetTicketsForUserAsync(userId, roleName);
            if (!string.IsNullOrEmpty(query))
            {
                tickets = tickets.Where(t => t.Title.Contains(query, StringComparison.OrdinalIgnoreCase)).ToList();
            }
            return tickets;
        }

    }
}
