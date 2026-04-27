using AutoMapper;
using IMS_Application.Common.Constants;
using IMS_Application.Common.Models;
using IMS_Application.DTOs;
using IMS_Application.Interfaces;
using IMS_Application.Services.Interfaces;
using IMS_Domain.Entities;
using Microsoft.Extensions.Logging;

namespace IMS_Application.Services
{
    public class TicketService : ITicketService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<TicketService> _logger;

        public TicketService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<TicketService> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        private async Task<Dictionary<int, User>> GetUsersForTicketAsync(Ticket ticket, int currentUserId)
        {
            var userIds = new HashSet<int> { currentUserId, ticket.CreatedBy };
            var latestAssign = ticket.TicketAssignments?
                .Where(a => a.status == LogicStrings.Active)
                .OrderByDescending(a => a.assigned_at)
                .FirstOrDefault();
            if (latestAssign != null)
            {
                userIds.Add(latestAssign.assignedTo);
            }

            return await _unitOfWork.Users.GetUsersByIdsAsync(userIds);
        }

        private TicketResponseDto MapToTicketResponseDto(Ticket ticket, Dictionary<int, User> usersDict)
        {
            var creator = usersDict.TryGetValue(ticket.CreatedBy, out var c) ? c : null;
            var creatorName = creator?.FullName ?? LogicStrings.Unknown;

            var latestAssign = ticket.TicketAssignments?
                .Where(a => a.status == LogicStrings.Active)
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
                    assignedToInfo = new UserInfo { id = latestAssign.assignedTo, name = LogicStrings.Unassigned };
                }
            }

            var ticketInfo = _mapper.Map<TicketInfo>(ticket);
            ticketInfo.createdBy = new UserInfo { id = ticket.CreatedBy, name = creatorName };
            ticketInfo.assignedTo = assignedToInfo;

            var comments = _mapper.Map<List<TicketCommentInfo>>(ticket.Comments
                .OrderByDescending(c => c.CreatedAt)
                .ToList());

            return new TicketResponseDto
            {
                ticket = ticketInfo,
                comments = comments
            };
        }

        public async Task<Result<TicketResponseDto>> CreateTicketAsync(CreateTicketRequestDto dto, int createdBy)
        {
            try
            {

                TicketType ticketType;
                if (dto.CategoryId.HasValue && dto.SubCategoryId.HasValue)
                {
                    ticketType = TicketType.Hardware;
                }
                else if (!Enum.TryParse<TicketType>(dto.TicketType, true, out ticketType))
                {
                    return Result<TicketResponseDto>.Failure(ErrorMessages.InvalidTicketType, 400);
                }

                if (!Enum.TryParse<TicketPriority>(dto.Priority, true, out var ticketPriority))
                    return Result<TicketResponseDto>.Failure(ErrorMessages.InvalidTicketPriority, 400);

                if (dto.CategoryId.HasValue)
                {
                    var category = await _unitOfWork.Categories.GetByIdAsync(dto.CategoryId.Value);
                    if (category == null)
                        return Result<TicketResponseDto>.Failure(ErrorMessages.CategoryOrSubCategoryNotFound, 400);
                }

                if (dto.SubCategoryId.HasValue)
                {
                    var subCategory = await _unitOfWork.SubCategories.GetByIdAsync(dto.SubCategoryId.Value);
                    if (subCategory == null)
                        return Result<TicketResponseDto>.Failure(ErrorMessages.CategoryOrSubCategoryNotFound, 400);
                }

                var assignee = await _unitOfWork.Users.GetByIdAsync(dto.assignedTo);
                if (assignee == null || assignee.Role.Name != LogicStrings.SupportEngineerRole)
                    return Result<TicketResponseDto>.Failure(ErrorMessages.InvalidTicketAssignee, 400);

                var ticket = _mapper.Map<Ticket>(dto);
                ticket.TicketType = ticketType;
                ticket.CreatedBy = createdBy;
                ticket.Status = Status.Open;
                ticket.CreatedAt = DateTime.UtcNow;
                ticket.UpdatedAt = DateTime.UtcNow;

                var assignment = new TicketAssignment
                {
                    assignedTo = dto.assignedTo,
                    assigned_by = createdBy,
                    assigned_at = DateTime.UtcNow,
                    status = LogicStrings.Active
                };
                ticket.TicketAssignments.Add(assignment);

                await _unitOfWork.Tickets.AddTicketAsync(ticket);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Ticket {TicketId} created successfully by user {UserId}", ticket.Id, createdBy);

                var usersDict = await GetUsersForTicketAsync(ticket, createdBy);
                var response = MapToTicketResponseDto(ticket, usersDict);

                return Result<TicketResponseDto>.Success(response, SuccessMessages.TicketCreated);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating ticket for user {UserId}", createdBy);
                return Result<TicketResponseDto>.Failure(ErrorMessages.ServerError, 500);
            }
        }

        public async Task<Result<TicketCommentResponseDto>> AddCommentAsync(int ticketId, string commentText, int currentUserId)
        {
            try
            {
                var comment = new TicketComment
                {
                    TicketId = ticketId,
                    UserId = currentUserId,
                    CommentText = commentText,
                    CreatedAt = DateTime.UtcNow
                };
                await _unitOfWork.Tickets.AddCommentAsync(comment);
                await _unitOfWork.SaveChangesAsync();
                var dto = _mapper.Map<TicketCommentResponseDto>(comment);
                return Result<TicketCommentResponseDto>.Success(dto, SuccessMessages.CommentCreated);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding comment to ticket {TicketId} by user {UserId}", ticketId, currentUserId);
                return Result<TicketCommentResponseDto>.Failure(ErrorMessages.ServerError, 500);
            }
        }

        public async Task<Result<UpdateTicketStatusResponseDto>> UpdateStatusAsync(int ticketId, string status, int currentUserId)
        {
            if (string.IsNullOrWhiteSpace(status) || !Enum.TryParse<Status>(status, true, out var newStatus))
                return Result<UpdateTicketStatusResponseDto>.Failure(ErrorMessages.InvalidTicketStatus, 400);

            try
            {
                var ticket = await _unitOfWork.Tickets.GetTicketByIdAsync(ticketId);
                if (ticket == null)
                    return Result<UpdateTicketStatusResponseDto>.Failure(ErrorMessages.TicketNotFound, 404);

                var oldStatus = ticket.Status;
                if (oldStatus != newStatus)
                {
                    var history = new TicketStatusHistory
                    {
                        TicketId = ticket.Id,
                        OldStatusId = (int)oldStatus,
                        NewStatusId = (int)newStatus,
                        ChangedBy = currentUserId,
                        ChangedAt = DateTime.UtcNow
                    };
                    await _unitOfWork.Tickets.AddTicketStatusHistoryAsync(history);
                }

                ticket.Status = newStatus;
                ticket.UpdatedAt = DateTime.UtcNow;
                _unitOfWork.Tickets.Update(ticket);
                await _unitOfWork.SaveChangesAsync();

                var dto = new UpdateTicketStatusResponseDto
                {
                    message = SuccessMessages.StatusUpdated,
                    updatedStatus = status,
                    updatedAt = DateTime.UtcNow
                };
                return Result<UpdateTicketStatusResponseDto>.Success(dto, SuccessMessages.StatusUpdated);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating status of ticket {TicketId} to {Status} by user {UserId}", ticketId, status, currentUserId);
                return Result<UpdateTicketStatusResponseDto>.Failure(ErrorMessages.ServerError, 500);
            }
        }

        public async Task<Result<List<TicketResponseDto>>> GetAllTicketsAsync(int currentUserId)
        {
            try
            {
                var user = await _unitOfWork.Users.GetByIdAsync(currentUserId);
                if (user == null)
                    return Result<List<TicketResponseDto>>.Failure(ErrorMessages.UserNotFoundError, 404);

                if (user.Role == null)
                    return Result<List<TicketResponseDto>>.Failure(ErrorMessages.RoleNotFoundError, 400);

                var tickets = await _unitOfWork.Tickets.GetTicketsForUserAsync(currentUserId, user.Role.Name);

                var allUsersDict = new Dictionary<int, User>();
                foreach (var ticket in tickets)
                {
                    var users = await GetUsersForTicketAsync(ticket, currentUserId);
                    foreach (var kvp in users)
                    {
                        allUsersDict[kvp.Key] = kvp.Value;
                    }
                }

                var dtos = tickets.OrderBy(t => t.CreatedAt)
                              .Select(t => MapToTicketResponseDto(t, allUsersDict))
                              .ToList();

                return Result<List<TicketResponseDto>>.Success(dtos, SuccessMessages.AllTickets);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving tickets for user {UserId}", currentUserId);
                return Result<List<TicketResponseDto>>.Failure(ErrorMessages.ServerError, 500);
            }
        }

        public async Task<Result<TicketResponseDto>> GetTicketByIdAsync(int ticketId, int currentUserId)
        {
            try
            {
                var ticket = await _unitOfWork.Tickets.GetTicketByIdAsync(ticketId);
                if (ticket == null)
                    return Result<TicketResponseDto>.Failure(ErrorMessages.TicketNotFound, 404);

                if (currentUserId != ticket.CreatedBy && !ticket.TicketAssignments.Any(a => a.status == LogicStrings.Active && a.assignedTo == currentUserId))
                    return Result<TicketResponseDto>.Failure(ErrorMessages.UnauthorizedTicketView, 403);

                var usersDict = await GetUsersForTicketAsync(ticket, currentUserId);
                var dto = MapToTicketResponseDto(ticket, usersDict);

                return Result<TicketResponseDto>.Success(dto, SuccessMessages.AllTickets);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting ticket {TicketId} for user {UserId}", ticketId, currentUserId);
                return Result<TicketResponseDto>.Failure(ErrorMessages.ServerError, 500);
            }
        }
        public async Task<Result<List<TicketResponseDto>>> SearchTicketsGroupedAsync(string q, int currentUserId)
        {
            try
            {
                var user = await _unitOfWork.Users.GetByIdAsync(currentUserId);
                if (user == null)
                    return Result<List<TicketResponseDto>>.Failure(ErrorMessages.UserNotFoundError, 404);

                if (user.Role == null)
                    return Result<List<TicketResponseDto>>.Failure(ErrorMessages.RoleNotFoundError, 400);

                var tickets = await _unitOfWork.Tickets.GetTicketsForUserAsync(currentUserId, user.Role.Name);

                _logger.LogInformation("[TICKET-SERVICE] Repo returned {TicketCount} tickets", tickets.Count);

                if (!string.IsNullOrWhiteSpace(q))
                {
                    var query = q.Trim();

                    int.TryParse(query, out var queryInt);
                    DateTime.TryParse(query, out var queryDate);
                    Enum.TryParse<TicketType>(query, true, out var queryTicketType);
                    Enum.TryParse<TicketPriority>(query, true, out var queryPriority);

                    var categories = (await _unitOfWork.Categories.GetAllAsync()).ToDictionary(c => c.Id, c => c.Name);
                    var subCategories = (await _unitOfWork.SubCategories.GetAllAsync()).ToDictionary(s => s.Id, s => s.Name);

                    tickets = tickets.Where(t =>
                        (queryInt > 0 && t.Id == queryInt) ||
                        (!string.IsNullOrEmpty(t.Title) && t.Title.Contains(query, StringComparison.OrdinalIgnoreCase)) ||
                        (!string.IsNullOrEmpty(t.Description) && t.Description.Contains(query, StringComparison.OrdinalIgnoreCase)) ||
                        (queryTicketType != default && t.TicketType == queryTicketType) ||
                        t.TicketType.ToString().Contains(query, StringComparison.OrdinalIgnoreCase) ||
                        (queryPriority != default && t.TicketPriority == queryPriority) ||
                        t.TicketPriority.ToString().Contains(query, StringComparison.OrdinalIgnoreCase) ||
                        (queryInt > 0 && t.AssetId == queryInt) ||
                        (t.CategoryId.HasValue && (
                            (queryInt > 0 && t.CategoryId == queryInt) ||
                            (categories.TryGetValue(t.CategoryId.Value, out var catName) && catName.Contains(query, StringComparison.OrdinalIgnoreCase))
                        )) ||
                        (t.SubCategoryId.HasValue && (
                            (queryInt > 0 && t.SubCategoryId == queryInt) ||
                            (subCategories.TryGetValue(t.SubCategoryId.Value, out var subCatName) && subCatName.Contains(query, StringComparison.OrdinalIgnoreCase))
                        )) ||
                        (queryInt > 0 && t.TicketAssignments.Any(a => a.assignedTo == queryInt)) ||
                        (queryDate != default && t.CreatedAt.Date == queryDate.Date)
                    ).ToList();
                }

                var allUsersDict = new Dictionary<int, User>();
                foreach (var ticket in tickets)
                {
                    var users = await GetUsersForTicketAsync(ticket, currentUserId);
                    foreach (var kvp in users)
                    {
                        allUsersDict[kvp.Key] = kvp.Value;
                    }
                }

                var dtos = tickets.OrderBy(t => t.CreatedAt)
                    .Select(t => MapToTicketResponseDto(t, allUsersDict))
                    .ToList();

                return Result<List<TicketResponseDto>>.Success(dtos, SuccessMessages.AllTickets);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching tickets for user {UserId} with query {Query}", currentUserId, q);
                return Result<List<TicketResponseDto>>.Failure(ErrorMessages.ServerError, 500);
            }
        }
    }
}
