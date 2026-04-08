using AutoMapper;
using IMS_Application.DTOs;
using IMS_Application.Interfaces;
using IMS_Application.Services.Interfaces;
using IMS_Domain.Entities;

namespace IMS_Application.Services
{
    public class TicketService : ITicketService
    {
        private readonly ITicketRepository _ticketRepository;
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;

        public TicketService(ITicketRepository ticketRepository, IUserRepository userRepository, IMapper mapper)
        {
            _ticketRepository = ticketRepository;
            _userRepository = userRepository;
            _mapper = mapper;
        }

        private TicketResponseDto MapToTicketResponseDto(Ticket ticket, Dictionary<int, User> usersDict)
        {
            var creator = usersDict.TryGetValue(ticket.CreatedBy, out var c) ? c : null;
            var creatorName = creator?.FullName ?? "Unknown";

            var latestAssign = ticket.TicketAssignments?
                .Where(a => a.status == "Active")
                .OrderByDescending(a => a.assigned_at)
                .FirstOrDefault();

            UserInfo? assignedToInfo = null;
            if (latestAssign != null)
            {
                if (usersDict.TryGetValue(latestAssign.assignedTo, out var assignee))
                {
                    assignedToInfo = new UserInfo { id = assignee.Id, name = assignee.FullName };
                }
                else
                {
                    assignedToInfo = new UserInfo { id = latestAssign.assignedTo, name = "Unassigned" };
                }
            }

            return new TicketResponseDto
            {
                ticket = new TicketInfo
                {
                    Id = $"TKT-{ticket.Id}",
                    title = ticket.Title,
                    description = ticket.Description,
                    TicketType = ticket.TicketType.ToString(),
                    TicketPriority = ticket.TicketPriority.ToString(),
                    status = ticket.Status.ToString(),
                    createdAt = ticket.CreatedAt.ToString("yyyy-MM-ddTHH:mm:ss"),
                    updatedAt = ticket.UpdatedAt.ToString("yyyy-MM-ddTHH:mm:ss"),
                    createdBy = new UserInfo { id = ticket.CreatedBy, name = creatorName },
                    assignedTo = assignedToInfo
                },
                comments = ticket.Comments
                    .OrderByDescending(c => c.CreatedAt)
                    .Select(c => new TicketCommentInfo
                    {
                        Id = c.Id,
                        UserId = c.UserId,
                        CommentText = c.CommentText,
                        CreatedAt = c.CreatedAt.ToString("yyyy-MM-ddTHH:mm:ss")
                    }).ToList()
            };
        }

        public async Task<List<TicketResponseDto>> GetAllTicketsAsync(int currentUserId)
        {
            var user = await _userRepository.GetByIdAsync(currentUserId);
            if (user == null)
            {
                throw new ArgumentException("User not found.");
            }

            if (user.Role == null)
            {
                throw new ArgumentException("User role not found.");
            }

            var tickets = await _ticketRepository.GetTicketsForUserAsync(currentUserId, user.Role.Name);

            var userIdsQuery = tickets.SelectMany(ticket =>
            {
                var ids = new List<int> { ticket.CreatedBy };
                var latestAssign = ticket.TicketAssignments?
                    .Where(a => a.status == "Active")
                    .OrderByDescending(a => a.assigned_at)
                    .FirstOrDefault();
                if (latestAssign != null)
                {
                    ids.Add(latestAssign.assignedTo);
                }
                return ids;
            });

            var userIds = new HashSet<int> { currentUserId };
            userIds.UnionWith(userIdsQuery);

            var userTasks = userIds.Select(id => _userRepository.GetByIdAsync(id));
            var usersList = await Task.WhenAll(userTasks);
            var usersDict = userIds.Zip(usersList, (id, u) => new { id, u })
                                   .Where(x => x.u != null)
                                   .ToDictionary(x => x.id, x => x.u!);

            return tickets.OrderBy(t => t.CreatedAt)
                          .Select(t => MapToTicketResponseDto(t, usersDict))
                          .ToList();
        }

        public async Task<List<TicketResponseDto>> SearchTicketsGroupedAsync(string q, int currentUserId)
        {
            var user = await _userRepository.GetByIdAsync(currentUserId);
            if (user == null)
            {
                throw new ArgumentException("User not found.");
            }

            if (user.Role == null)
            {
                throw new ArgumentException("User role not found.");
            }

            var tickets = await _ticketRepository.SearchTicketsAsync(q, currentUserId, user.Role.Name);
            Console.WriteLine($"[TICKET-SERVICE] Repo returned {tickets.Count} tickets");


            var userIds = new HashSet<int>();
            userIds.Add(currentUserId);
            foreach (var ticket in tickets)
            {
                userIds.Add(ticket.CreatedBy);
                var latestAssign = ticket.TicketAssignments?
                    .Where(a => a.status == "Active")
                    .OrderByDescending(a => a.assigned_at)
                    .FirstOrDefault();
                if (latestAssign != null)
                {
                    userIds.Add(latestAssign.assignedTo);
                }
            }

            var usersDict = new Dictionary<int, User>();
            foreach (var id in userIds)
            {
                var u = await _userRepository.GetByIdAsync(id);
                if (u != null) usersDict[id] = u;
            }

            var result = new List<TicketResponseDto>();
            foreach (var ticket in tickets.OrderBy(t => t.CreatedAt))
            {
                var creator = usersDict.TryGetValue(ticket.CreatedBy, out var c) ? c : null;
                var creatorName = creator?.FullName ?? "Unknown";

                var latestAssign = ticket.TicketAssignments?
                    .Where(a => a.status == "Active")
                    .OrderByDescending(a => a.assigned_at)
                    .FirstOrDefault();

                UserInfo? assignedToInfo = null;
                if (latestAssign != null)
                {
                    if (usersDict.TryGetValue(latestAssign.assignedTo, out var assignee))
                    {
                        assignedToInfo = new UserInfo { id = assignee.Id, name = assignee.FullName };
                    }
                    else
                    {
                        assignedToInfo = new UserInfo { id = latestAssign.assignedTo, name = "Unassigned" };
                    }
                }

                var dto = new TicketResponseDto
                {
                    ticket = new TicketInfo
                    {
                        Id = $"TKT-{ticket.Id:D4}",
                        title = ticket.Title,
                        description = ticket.Description,
                        TicketType = ticket.TicketType.ToString(),
                        TicketPriority = ticket.TicketPriority.ToString(),
                        status = ticket.Status.ToString(),
                        createdAt = ticket.CreatedAt.ToString("yyyy-MM-ddTHH:mm:ss"),
                        updatedAt = ticket.UpdatedAt.ToString("yyyy-MM-ddTHH:mm:ss"),
                        createdBy = new UserInfo { id = ticket.CreatedBy, name = creatorName },
                        assignedTo = assignedToInfo
                    },
                    comments = ticket.Comments
                        .OrderByDescending(c => c.CreatedAt)
                        .Select(c => new TicketCommentInfo
                        {
                            Id = c.Id,
                            UserId = c.UserId,
                            CommentText = c.CommentText,
                            CreatedAt = c.CreatedAt.ToString("yyyy-MM-ddTHH:mm:ss")
                        }).ToList()
                };
                result.Add(dto);
            }

            return result;
        }

        public async Task<TicketResponseDto> CreateTicketAsync(IMS_Application.DTOs.CreateTicketRequestDto dto, int createdBy)
        {

            var assignee = await _userRepository.GetByIdAsync(dto.assignedTo);
            if (assignee == null || assignee.Role.Name != "Support Engineer")
            {
                throw new ArgumentException("Assigned user must be a Support Engineer.");
            }

            var ticketId = await _ticketRepository.CreateTicketAsync(dto, createdBy);

            var ticket = await _ticketRepository.GetTicketByIdAsync(ticketId);
            if (ticket == null)
            {
                throw new InvalidOperationException("Failed to create ticket.");
            }

            var creator = await _userRepository.GetByIdAsync(ticket.CreatedBy);
            var creatorName = creator?.FullName ?? "Unknown";

            var latestAssign = ticket.TicketAssignments?
                .Where(a => a.status == "Active")
                .OrderByDescending(a => a.assigned_at)
                .FirstOrDefault();

            UserInfo? assignedToInfo = null;
            if (latestAssign != null)
            {
                var assigneeUser = await _userRepository.GetByIdAsync(latestAssign.assignedTo);
                assignedToInfo = assigneeUser != null
                    ? new UserInfo { id = assigneeUser.Id, name = assigneeUser.FullName }
                    : new UserInfo { id = 0, name = "Unassigned" };
            }

            var response = new TicketResponseDto
            {
                ticket = new TicketInfo
                {
                    Id = $"TKT-{ticket.Id}",
                    title = ticket.Title,
                    description = ticket.Description,
                    TicketType = ticket.TicketType.ToString(),
                    TicketPriority = ticket.TicketPriority.ToString(),
                    status = ticket.Status.ToString(),
                    createdAt = ticket.CreatedAt.ToString("yyyy-MM-ddTHH:mm:ss"),
                    updatedAt = ticket.UpdatedAt.ToString("yyyy-MM-ddTHH:mm:ss"),
                    createdBy = new UserInfo { id = ticket.CreatedBy, name = creatorName },
                    assignedTo = assignedToInfo
                },
                comments = new List<TicketCommentInfo>()
            };
            return response;

        }

        public async Task<TicketCommentResponseDto> AddCommentAsync(int ticketId, string commentText, int currentUserId)
        {
            var comment = await _ticketRepository.AddCommentAsync(ticketId, commentText, currentUserId);
            return _mapper.Map<TicketCommentResponseDto>(comment);
        }

        public async Task<UpdateTicketStatusResponseDto> UpdateStatusAsync(int ticketId, string status, int currentUserId)
        {
            await _ticketRepository.UpdateStatusAsync(ticketId, status, currentUserId);
            return new UpdateTicketStatusResponseDto
            {
                message = "Ticket status updated successfully",
                updatedStatus = status,
                updatedAt = DateTime.UtcNow
            };
        }

        public async Task<TicketResponseDto?> GetTicketByIdAsync(int ticketId, int currentUserId)
        {
            var ticket = await _ticketRepository.GetTicketByIdAsync(ticketId);
            if (ticket == null) return null;
            if (currentUserId != ticket.CreatedBy && !ticket.TicketAssignments.Any(a => a.status == "Active" && a.assignedTo == currentUserId))
            {
                throw new ArgumentException("You are not authorized to view this ticket.");
            }

            var creator = await _userRepository.GetByIdAsync(ticket.CreatedBy);
            var creatorName = creator?.FullName ?? "Unknown";

            var latestAssign = ticket.TicketAssignments?
                .Where(a => a.status == "Active")
                .OrderByDescending(a => a.assigned_at)
                .FirstOrDefault();

            UserInfo? assignedToInfo = null;
            if (latestAssign != null)
            {
                var assignee = await _userRepository.GetByIdAsync(latestAssign.assignedTo);
                if (assignee != null)
                    assignedToInfo = new UserInfo { id = assignee.Id, name = assignee.FullName };
                else
                    assignedToInfo = new UserInfo { id = 0, name = "Unassigned" };
            }

            var dto = new TicketResponseDto
            {
                ticket = new TicketInfo
                {
                    Id = $"TKT-{ticket.Id}",
                    title = ticket.Title,
                    description = ticket.Description,
                    TicketType = ticket.TicketType.ToString(),
                    TicketPriority = ticket.TicketPriority.ToString(),
                    status = ticket.Status.ToString(),
                    createdAt = ticket.CreatedAt.ToString("yyyy-MM-ddTHH:mm:ss"),
                    updatedAt = ticket.UpdatedAt.ToString("yyyy-MM-ddTHH:mm:ss"),
                    createdBy = new UserInfo { id = ticket.CreatedBy, name = creatorName },
                    assignedTo = assignedToInfo
                },
                comments = ticket.Comments
                    .OrderByDescending(c => c.CreatedAt)
                    .Select(c => new TicketCommentInfo
                    {
                        Id = c.Id,
                        UserId = c.UserId,
                        CommentText = c.CommentText,
                        CreatedAt = c.CreatedAt.ToString("yyyy-MM-ddTHH:mm:ss")
                    }).ToList()

            };
            return dto;
        }
    }
}

