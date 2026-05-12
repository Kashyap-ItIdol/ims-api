using AutoMapper;
using IMS_Application.Common.Constants;
using IMS_Application.Common.Models;
using IMS_Application.DTOs;
using IMS_Application.Interfaces;
using IMS_Application.Services.Interfaces;
using IMS_Domain.Entities;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;

namespace IMS_Application.Services
{
    public class TicketService : ITicketService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<TicketService> _logger;
        private readonly IWebHostEnvironment _env;

        public TicketService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<TicketService> logger, IWebHostEnvironment env)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
            _env = env;
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

            var latestAssign = ticket.TicketAssignments?
                .Where(a => a.status == LogicStrings.Active)
                .OrderByDescending(a => a.assigned_at)
                .FirstOrDefault();

            UserInfo? assignedToInfo = null;
            if (latestAssign != null)
            {
                if (usersDict.TryGetValue(latestAssign.assignedTo, out var assignee))
                {
                    assignedToInfo = _mapper.Map<UserInfo>(assignee);
                }
                else
                {
                    assignedToInfo = new UserInfo { id = latestAssign.assignedTo, name = LogicStrings.Unassigned };
                }
            }

            var ticketInfo = _mapper.Map<TicketInfo>(ticket);
            ticketInfo.createdBy = creator != null
                ? _mapper.Map<UserInfo>(creator)
                : new UserInfo { id = ticket.CreatedBy, name = LogicStrings.Unknown };
            ticketInfo.assignedTo = assignedToInfo;

            var allComments = new List<TicketComment>();
            foreach (var comment in ticket.Comments.Where(c => c.ParentCommentId == null).OrderByDescending(c => c.CreatedAt))
            {
                allComments.Add(comment);
                if (comment.Replies != null)
                {
                    allComments.AddRange(comment.Replies.OrderByDescending(r => r.CreatedAt));
                }
            }

            var comments = _mapper.Map<List<TicketCommentInfo>>(allComments);

            var attachments = _mapper.Map<List<TicketAttachmentInfo>>(ticket.Attachments
                .OrderByDescending(a => a.UploadedAt)
                .ToList());

            return new TicketResponseDto
            {
                ticket = ticketInfo,
                comments = comments,
                attachments = attachments
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
                if (assignee == null || assignee.Role?.Name != LogicStrings.SupportEngineerRole)
                    return Result<TicketResponseDto>.Failure(ErrorMessages.InvalidTicketAssignee, 400);

                if (dto.CategoryId.HasValue)
                {
                    var category = await _unitOfWork.Categories.GetByIdAsync(dto.CategoryId.Value);
                    if (category == null)
                        return Result<TicketResponseDto>.Failure(ErrorMessages.CategoryNotFound, 400);
                }

                if (dto.SubCategoryId.HasValue)
                {
                    var subCategory = await _unitOfWork.SubCategories.GetByIdAsync(dto.SubCategoryId.Value);
                    if (subCategory == null)
                        return Result<TicketResponseDto>.Failure(ErrorMessages.SubCategoryNotFound, 400);

                    if (!dto.CategoryId.HasValue || dto.CategoryId.Value != subCategory.CategoryId)
                        return Result<TicketResponseDto>.Failure(ErrorMessages.SubCategoryCategoryIdInvalid, 400);
                }

                var ticket = _mapper.Map<Ticket>(dto);
                ticket.TicketType = ticketType;
                ticket.CreatedBy = createdBy;
                ticket.Status = Status.Open;
                ticket.CreatedAt = DateTime.UtcNow;
                ticket.UpdatedAt = DateTime.UtcNow;

                if (Enum.TryParse<TicketType>(dto.TicketType, true, out var parsedTicketType))
                    ticket.TicketType = parsedTicketType;

                if (Enum.TryParse<TicketPriority>(dto.Priority, true, out var parsedTicketPriority))
                    ticket.TicketPriority = parsedTicketPriority;

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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating ticket for user {UserId}", createdBy);
                return Result<TicketResponseDto>.Failure(ErrorMessages.ServerError, 500);
            }
        }

        public async Task<Result<TicketCommentResponseDto>> AddCommentAsync(int ticketId, string commentText, int currentUserId)
        {
            if (string.IsNullOrWhiteSpace(commentText))
                return Result<TicketCommentResponseDto>.Failure(ErrorMessages.CommentRequires, 400);

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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding comment to ticket {TicketId} by user {UserId}", ticketId, currentUserId);
                return Result<TicketCommentResponseDto>.Failure(ErrorMessages.ServerError, 500);
            }
        }

        public async Task<Result<TicketCommentResponseDto>> AddReplyAsync(int ticketId, int parentCommentId, string commentText, int currentUserId)
        {
            if (string.IsNullOrWhiteSpace(commentText))
                return Result<TicketCommentResponseDto>.Failure(ErrorMessages.CommentRequires, 400);

            try
            {
                var parentComment = await _unitOfWork.Tickets.GetCommentByIdAsync(parentCommentId);
                if (parentComment == null)
                    return Result<TicketCommentResponseDto>.Failure(ErrorMessages.CommentNotFound, 404);

                if (parentComment.TicketId != ticketId)
                    return Result<TicketCommentResponseDto>.Failure(ErrorMessages.TicketNotFound, 404);

                var reply = new TicketComment
                {
                    TicketId = ticketId,
                    UserId = currentUserId,
                    ParentCommentId = parentCommentId,
                    CommentText = commentText,
                    CreatedAt = DateTime.UtcNow
                };
                await _unitOfWork.Tickets.AddCommentAsync(reply);
                await _unitOfWork.SaveChangesAsync();
                var dto = _mapper.Map<TicketCommentResponseDto>(reply);
                return Result<TicketCommentResponseDto>.Success(dto, SuccessMessages.ReplyCreated);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding reply to comment {ParentCommentId} on ticket {TicketId} by user {UserId}", parentCommentId, ticketId, currentUserId);
                return Result<TicketCommentResponseDto>.Failure(ErrorMessages.ServerError, 500);
            }
        }

        public async Task<Result<TicketCommentResponseDto>> EditCommentAsync(int commentId, string commentText, int currentUserId)
        {
            if (string.IsNullOrWhiteSpace(commentText))
                return Result<TicketCommentResponseDto>.Failure(ErrorMessages.CommentRequires, 400);

            try
            {
                var comment = await _unitOfWork.Tickets.GetCommentByIdAsync(commentId);
                if (comment == null)
                    return Result<TicketCommentResponseDto>.Failure(ErrorMessages.CommentNotFound, 404);

                if (comment.UserId != currentUserId)
                    return Result<TicketCommentResponseDto>.Failure(ErrorMessages.UnauthorizedCommentEdit, 403);

                comment.CommentText = commentText;
                comment.UpdatedAt = DateTime.UtcNow;
                await _unitOfWork.Tickets.UpdateCommentAsync(comment);
                await _unitOfWork.SaveChangesAsync();
                var dto = _mapper.Map<TicketCommentResponseDto>(comment);
                return Result<TicketCommentResponseDto>.Success(dto, SuccessMessages.CommentUpdated);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error editing comment {CommentId} by user {UserId}", commentId, currentUserId);
                return Result<TicketCommentResponseDto>.Failure(ErrorMessages.ServerError, 500);
            }
        }

        public async Task<Result<CommentLikeResponseDto>> DeleteCommentAsync(int commentId, int currentUserId)
        {
            try
            {
                var comment = await _unitOfWork.Tickets.GetCommentByIdAsync(commentId);
                if (comment == null)
                    return Result<CommentLikeResponseDto>.Failure(ErrorMessages.CommentNotFound, 404);

                if (comment.UserId != currentUserId)
                    return Result<CommentLikeResponseDto>.Failure(ErrorMessages.UnauthorizedCommentDelete, 403);

                comment.IsDeleted = true;
                comment.DeletedAt = DateTime.UtcNow;
                comment.DeletedBy = currentUserId;

                foreach (var reply in comment.Replies)
                {
                    reply.IsDeleted = true;
                    reply.DeletedAt = DateTime.UtcNow;
                    reply.DeletedBy = currentUserId;
                }

                await _unitOfWork.Tickets.DeleteCommentAsync(comment);
                await _unitOfWork.SaveChangesAsync();

                var dto = new CommentLikeResponseDto
                {
                    Id = commentId,
                    CommentId = comment.TicketId,
                    UserId = currentUserId,
                    CreatedAt = comment.DeletedAt?.ToString("o") ?? string.Empty
                };
                return Result<CommentLikeResponseDto>.Success(dto, SuccessMessages.CommentDeleted);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting comment {CommentId} by user {UserId}", commentId, currentUserId);
                return Result<CommentLikeResponseDto>.Failure(ErrorMessages.ServerError, 500);
            }
        }

        public async Task<Result<CommentLikeResponseDto>> LikeCommentAsync(int commentId, int currentUserId)
        {
            try
            {
                var comment = await _unitOfWork.Tickets.GetCommentByIdAsync(commentId);
                if (comment == null)
                    return Result<CommentLikeResponseDto>.Failure(ErrorMessages.CommentNotFound, 404);

                var existingLike = await _unitOfWork.Tickets.GetCommentLikeAsync(commentId, currentUserId);
                if (existingLike != null)
                    return Result<CommentLikeResponseDto>.Failure(ErrorMessages.CommentAlreadyLiked, 400);

                var like = new TicketCommentLike
                {
                    CommentId = commentId,
                    UserId = currentUserId,
                    CreatedAt = DateTime.UtcNow
                };
                await _unitOfWork.Tickets.AddCommentLikeAsync(like);
                await _unitOfWork.SaveChangesAsync();
                var dto = _mapper.Map<CommentLikeResponseDto>(like);
                return Result<CommentLikeResponseDto>.Success(dto, SuccessMessages.CommentLiked);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error liking comment {CommentId} by user {UserId}", commentId, currentUserId);
                return Result<CommentLikeResponseDto>.Failure(ErrorMessages.ServerError, 500);
            }
        }

        public async Task<Result<CommentLikeResponseDto>> UnlikeCommentAsync(int commentId, int currentUserId)
        {
            try
            {
                var comment = await _unitOfWork.Tickets.GetCommentByIdAsync(commentId);
                if (comment == null)
                    return Result<CommentLikeResponseDto>.Failure(ErrorMessages.CommentNotFound, 404);

                var like = await _unitOfWork.Tickets.GetCommentLikeAsync(commentId, currentUserId);
                if (like == null)
                    return Result<CommentLikeResponseDto>.Failure(ErrorMessages.LikeNotFound, 404);

                like.IsDeleted = true;
                like.DeletedAt = DateTime.UtcNow;
                like.DeletedBy = currentUserId;

                await _unitOfWork.Tickets.UpdateCommentLikeAsync(like);
                await _unitOfWork.SaveChangesAsync();

                var dto = new CommentLikeResponseDto
                {
                    Id = like.Id,
                    CommentId = commentId,
                    UserId = currentUserId,
                    CreatedAt = like.DeletedAt?.ToString("o") ?? string.Empty
                };
                return Result<CommentLikeResponseDto>.Success(dto, SuccessMessages.CommentUnliked);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error unliking comment {CommentId} by user {UserId}", commentId, currentUserId);
                return Result<CommentLikeResponseDto>.Failure(ErrorMessages.ServerError, 500);
            }
        }

        public async Task<Result<CommentReactionResponseDto>> AddReactionAsync(int commentId, string reactionType, int currentUserId)
        {
            if (string.IsNullOrWhiteSpace(reactionType))
                return Result<CommentReactionResponseDto>.Failure(ErrorMessages.InvalidReactionType, 400);

            try
            {
                var comment = await _unitOfWork.Tickets.GetCommentByIdAsync(commentId);
                if (comment == null)
                    return Result<CommentReactionResponseDto>.Failure(ErrorMessages.CommentNotFound, 404);

                var existingReaction = await _unitOfWork.Tickets.GetCommentReactionAsync(commentId, currentUserId);
                if (existingReaction != null)
                {
                    existingReaction.ReactionType = reactionType;
                    existingReaction.CreatedAt = DateTime.UtcNow;
                    await _unitOfWork.Tickets.UpdateCommentReactionAsync(existingReaction);
                    await _unitOfWork.SaveChangesAsync();
                    var updatedDto = _mapper.Map<CommentReactionResponseDto>(existingReaction);
                    return Result<CommentReactionResponseDto>.Success(updatedDto, SuccessMessages.ReactionAdded);
                }

                var reaction = new TicketCommentReaction
                {
                    CommentId = commentId,
                    UserId = currentUserId,
                    ReactionType = reactionType,
                    CreatedAt = DateTime.UtcNow
                };
                await _unitOfWork.Tickets.AddCommentReactionAsync(reaction);
                await _unitOfWork.SaveChangesAsync();
                var dto = _mapper.Map<CommentReactionResponseDto>(reaction);
                return Result<CommentReactionResponseDto>.Success(dto, SuccessMessages.ReactionAdded);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding reaction to comment {CommentId} by user {UserId}", commentId, currentUserId);
                return Result<CommentReactionResponseDto>.Failure(ErrorMessages.ServerError, 500);
            }
        }

        public async Task<Result<CommentReactionResponseDto>> RemoveReactionAsync(int commentId, int currentUserId)
        {
            try
            {
                var comment = await _unitOfWork.Tickets.GetCommentByIdAsync(commentId);
                if (comment == null)
                    return Result<CommentReactionResponseDto>.Failure(ErrorMessages.CommentNotFound, 404);

                var reaction = await _unitOfWork.Tickets.GetCommentReactionAsync(commentId, currentUserId);
                if (reaction == null)
                    return Result<CommentReactionResponseDto>.Failure(ErrorMessages.ReactionNotFound, 404);

                reaction.IsDeleted = true;
                reaction.DeletedAt = DateTime.UtcNow;
                reaction.DeletedBy = currentUserId;

                await _unitOfWork.Tickets.UpdateCommentReactionAsync(reaction);
                await _unitOfWork.SaveChangesAsync();

                var dto = new CommentReactionResponseDto
                {
                    Id = reaction.Id,
                    CommentId = commentId,
                    UserId = currentUserId,
                    ReactionType = reaction.ReactionType,
                    CreatedAt = reaction.DeletedAt?.ToString("o") ?? string.Empty
                };
                return Result<CommentReactionResponseDto>.Success(dto, SuccessMessages.ReactionRemoved);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing reaction from comment {CommentId} by user {UserId}", commentId, currentUserId);
                return Result<CommentReactionResponseDto>.Failure(ErrorMessages.ServerError, 500);
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

                await _unitOfWork.Tickets.UpdateTicketStatusAsync(ticket, newStatus, currentUserId);
                await _unitOfWork.SaveChangesAsync();

                var dto = _mapper.Map<UpdateTicketStatusResponseDto>(ticket);
                return Result<UpdateTicketStatusResponseDto>.Success(dto, SuccessMessages.StatusUpdated);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating status of ticket {TicketId} to {Status} by user {UserId}", ticketId, status, currentUserId);
                return Result<UpdateTicketStatusResponseDto>.Failure(ErrorMessages.ServerError, 500);
            }
        }

        public async Task<Result<PagedResult<TicketResponseDto>>> GetAllTicketsAsync(int currentUserId, int pageNumber, int pageSize)
        {
            if (pageNumber < 1 || pageSize < 1)
                return Result<PagedResult<TicketResponseDto>>.Failure(ErrorMessages.InvalidPagination, 400);

            try
            {
                var user = await _unitOfWork.Users.GetByIdAsync(currentUserId);
                if (user == null)
                    return Result<PagedResult<TicketResponseDto>>.Failure(ErrorMessages.UserNotFoundError, 404);

                if (user.Role == null)
                    return Result<PagedResult<TicketResponseDto>>.Failure(ErrorMessages.RoleNotFoundError, 400);

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

                var orderedTickets = tickets.OrderByDescending(t => t.CreatedAt).ToList();
                var totalCount = orderedTickets.Count;
                var pagedTickets = orderedTickets
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                var dtos = pagedTickets
                              .Select(t => MapToTicketResponseDto(t, allUsersDict))
                              .ToList();

                var pagedResult = new PagedResult<TicketResponseDto>
                {
                    Items = dtos,
                    TotalCount = totalCount,
                    PageNumber = pageNumber,
                    PageSize = pageSize
                };

                return Result<PagedResult<TicketResponseDto>>.Success(pagedResult, SuccessMessages.AllTickets);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving tickets for user {UserId}", currentUserId);
                return Result<PagedResult<TicketResponseDto>>.Failure(ErrorMessages.ServerError, 500);
            }
        }

        public async Task<Result<TicketResponseDto>> GetTicketByIdAsync(int ticketId, int currentUserId)
        {
            try
            {
                var ticket = await _unitOfWork.Tickets.GetTicketByIdAsync(ticketId);
                if (ticket == null)
                    return Result<TicketResponseDto>.Failure(ErrorMessages.TicketNotFound, 404);

                var user = await _unitOfWork.Users.GetByIdAsync(currentUserId);
                if (user == null)
                    return Result<TicketResponseDto>.Failure(ErrorMessages.UserNotFoundError, 404);

                bool hasAccess = user.Role?.Name switch
                {
                    LogicStrings.AdminRole => true,
                    LogicStrings.SupportEngineerRole => ticket.TicketAssignments.Any(a => a.status == LogicStrings.Active && a.assignedTo == currentUserId),
                    _ => ticket.CreatedBy == currentUserId
                };

                if (!hasAccess)
                    return Result<TicketResponseDto>.Failure(ErrorMessages.UnauthorizedTicketView, 403);

                var usersDict = await GetUsersForTicketAsync(ticket, currentUserId);
                var dto = MapToTicketResponseDto(ticket, usersDict);
                return Result<TicketResponseDto>.Success(dto, SuccessMessages.TicketFetched);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting ticket {TicketId} for user {UserId}", ticketId, currentUserId);
                return Result<TicketResponseDto>.Failure(ErrorMessages.ServerError, 500);
            }
        }

        public async Task<Result<PagedResult<TicketResponseDto>>> GetCalendarFilteredTicketsAsync(int currentUserId, int pageNumber, int pageSize, string? dateFilter, DateOnly? startDate, DateOnly? endDate)
        {

            if (pageNumber < 1 || pageSize < 1)

                return Result<PagedResult<TicketResponseDto>>.Failure(ErrorMessages.InvalidPagination, 400);

            try
            {
                var user = await _unitOfWork.Users.GetByIdAsync(currentUserId);
                if (user == null)
                    return Result<PagedResult<TicketResponseDto>>.Failure(ErrorMessages.UserNotFoundError, 404);

                if (user.Role == null)
                    return Result<PagedResult<TicketResponseDto>>.Failure(ErrorMessages.RoleNotFoundError, 400);

                var tickets = await _unitOfWork.Tickets.GetTicketsForUserAsync(currentUserId, user.Role.Name);

                if (startDate.HasValue || endDate.HasValue)
                {
                    if (!startDate.HasValue || !endDate.HasValue)
                        return Result<PagedResult<TicketResponseDto>>.Failure(ErrorMessages.InvalidDateRange, 400);

                    var startUtc = startDate.Value.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
                    var endUtcExclusive = endDate.Value.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc).AddDays(1);


                    if (startUtc > endUtcExclusive)
                        return Result<PagedResult<TicketResponseDto>>.Failure(ErrorMessages.InvalidDateRange, 400);

                    tickets = tickets.Where(t => t.CreatedAt >= startUtc && t.CreatedAt < endUtcExclusive).ToList();
                }
                else if (!string.IsNullOrEmpty(dateFilter))
                {
                    var validFilters = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "today", "yesterday", "lastWeek", "lastMonth", "currentMonth", "all" };
                    if (!validFilters.Contains(dateFilter))
                        return Result<PagedResult<TicketResponseDto>>.Failure(ErrorMessages.InvalidDateFilter, 400);

                    var today = DateTime.Today.ToUniversalTime();
                    DateTime filterStart = DateTime.MinValue;
                    DateTime filterEnd = DateTime.MaxValue;
                    switch (dateFilter.ToLowerInvariant())
                    {
                        case "today":
                            filterStart = today;
                            filterEnd = today.AddDays(1);
                            break;
                        case "yesterday":
                            filterStart = today.AddDays(-1);
                            filterEnd = today;
                            break;
                        case "lastweek":
                            filterStart = today.AddDays(-7);
                            filterEnd = today;
                            break;
                        case "lastmonth":
                            filterStart = new DateTime(today.Year, today.Month, 1, 0, 0, 0, DateTimeKind.Utc).AddMonths(-1);
                            filterEnd = new DateTime(today.Year, today.Month, 1, 0, 0, 0, DateTimeKind.Utc);
                            break;
                        case "currentmonth":
                            filterStart = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1, 0, 0, 0, DateTimeKind.Utc);
                            filterEnd = filterStart.AddMonths(1);
                            break;
                        case "all":
                            // no filter
                            break;
                    }
                    tickets = tickets.Where(t => t.CreatedAt >= filterStart && t.CreatedAt < filterEnd).ToList();
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

                var orderedTickets = tickets.OrderByDescending(t => t.CreatedAt).ToList();
                var totalCount = orderedTickets.Count;
                var pagedTickets = orderedTickets
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                var dtos = pagedTickets
                    .Select(t => MapToTicketResponseDto(t, allUsersDict))
                    .ToList();

                var pagedResult = new PagedResult<TicketResponseDto>
                {
                    Items = dtos,
                    TotalCount = totalCount,
                    PageNumber = pageNumber,
                    PageSize = pageSize
                };

                return Result<PagedResult<TicketResponseDto>>.Success(pagedResult, SuccessMessages.AllTickets);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving calendar filtered tickets for user {UserId}", currentUserId);
                return Result<PagedResult<TicketResponseDto>>.Failure(ErrorMessages.ServerError, 500);
            }
        }

        public async Task<Result<bool>> DeleteTicketAsync(int ticketId, int deletedBy)

        {
            try
            {
                var ticket = await _unitOfWork.Tickets.GetTicketByIdAsync(ticketId);
                if (ticket == null || ticket.IsDeleted)
                    return Result<bool>.Failure(ErrorMessages.TicketNotFound, 404);

                var latestAssign = ticket.TicketAssignments?
                    .Where(a => a.status == LogicStrings.Active)
                    .OrderByDescending(a => a.assigned_at)
                    .FirstOrDefault();

                if (deletedBy != ticket.CreatedBy && (latestAssign?.assignedTo != deletedBy))
                    return Result<bool>.Failure(ErrorMessages.Unauthorized, 403);

                ticket.IsDeleted = true;
                ticket.DeletedBy = deletedBy;
                ticket.DeletedAt = DateTime.UtcNow;

                _unitOfWork.Tickets.Update(ticket);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Ticket {TicketId} successfully deleted by user {UserId}", ticketId, deletedBy);

                return Result<bool>.Success(true, SuccessMessages.TicketDeleted);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting ticket {TicketId} by user {DeletedBy}", ticketId, deletedBy);
                return Result<bool>.Failure(ErrorMessages.ServerError, 500);
            }
        }

        public async Task<Result<List<TicketResponseDto>>> SearchTicketsGroupedAsync(string q, int currentUserId)
        {
            if (string.IsNullOrWhiteSpace(q))
                return Result<List<TicketResponseDto>>.Failure(ErrorMessages.SearchQueryRequired, 400);

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

                var dtos = tickets.OrderByDescending(t => t.CreatedAt)
                    .Select(t => MapToTicketResponseDto(t, allUsersDict))
                    .ToList();

                return Result<List<TicketResponseDto>>.Success(dtos, SuccessMessages.AllTickets);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching tickets for user {UserId} with query {Query}", currentUserId, q);
                return Result<List<TicketResponseDto>>.Failure(ErrorMessages.ServerError, 500);
            }
        }

        public async Task<Result<TicketResponseDto>> UpdateTicketAsync(int id, UpdateTicketDto dto, int updatedBy)
        {
            try
            {
                var ticket = await _unitOfWork.Tickets.GetTicketByIdAsync(id);
                if (ticket == null || ticket.IsDeleted)
                    return Result<TicketResponseDto>.Failure(ErrorMessages.TicketNotFound, 404);

                _mapper.Map(dto, ticket);

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

                ticket.UpdatedAt = DateTime.UtcNow;

                _unitOfWork.Tickets.Update(ticket);
                await _unitOfWork.SaveChangesAsync();

                var usersDict = await GetUsersForTicketAsync(ticket, ticket.CreatedBy);
                var response = MapToTicketResponseDto(ticket, usersDict);

                _logger.LogInformation("Ticket {TicketId} updated successfully by {UserId}", id, updatedBy);

                return Result<TicketResponseDto>.Success(response, SuccessMessages.TicketUpdated);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating ticket {TicketId}", id);
                return Result<TicketResponseDto>.Failure(ErrorMessages.ServerError, 500);
            }
        }
        private bool IsTicketVisibleToUser(Ticket ticket, int userId)
        {
            return userId == ticket.CreatedBy ||
                   ticket.TicketAssignments.Any(a => a.status == LogicStrings.Active && a.assignedTo == userId);
        }

        public async Task<Result<List<TicketResponseDto>>> FilterTicketsAsync(TicketFilterDto filter, int currentUserId)
        {
            try
            {
                var user = await _unitOfWork.Users.GetByIdAsync(currentUserId);
                if (user == null)
                    return Result<List<TicketResponseDto>>.Failure(ErrorMessages.UserNotFoundError, 404);

                var tickets = await _unitOfWork.Tickets.FilterTicketsAsync(filter);

                if (!tickets.Any())
                    return Result<List<TicketResponseDto>>.Success(new List<TicketResponseDto>(), ErrorMessages.InvalidMatch);

                var allUserIds = new HashSet<int>();
                foreach (var ticket in tickets)
                {
                    allUserIds.Add(ticket.CreatedBy);
                    var latestAssign = ticket.TicketAssignments?
                        .Where(a => a.status == LogicStrings.Active)
                        .OrderByDescending(a => a.assigned_at)
                        .FirstOrDefault();
                    if (latestAssign != null)
                        allUserIds.Add(latestAssign.assignedTo);
                }

                var allUsersDict = await _unitOfWork.Users.GetUsersByIdsAsync(allUserIds);

                var visibleTickets = tickets
                    .Where(t => IsTicketVisibleToUser(t, currentUserId))
                    .OrderByDescending(t => t.UpdatedAt)
                    .ToList();

                var dtos = visibleTickets.Select(t => MapToTicketResponseDto(t, allUsersDict)).ToList();

                _logger.LogInformation("Filtered {Count} visible tickets for user {UserId} with filter {@Filter}",
                    dtos.Count, currentUserId, filter);

                return Result<List<TicketResponseDto>>.Success(dtos, SuccessMessages.TicketFetched);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error filtering tickets for user {UserId} with filter {@Filter}", currentUserId, filter);
                return Result<List<TicketResponseDto>>.Failure(ErrorMessages.ServerError, 500);
            }
        }

        public async Task<Result<List<TicketAttachmentResponseDto>>> UploadFilesAsync(TicketAttachmentRequestDto dto, int userId, int ticketId)
        {
            try
            {
                var ticket = await _unitOfWork.Tickets.GetTicketByIdAsync(ticketId);
                if (ticket == null)
                {
                    _logger.LogWarning("Ticket {TicketId} not found for attachment upload", ticketId);
                    return Result<List<TicketAttachmentResponseDto>>.Failure(ErrorMessages.TicketNotFound, 404);
                }

                if (dto.Files == null || !dto.Files.Any())
                {
                    return Result<List<TicketAttachmentResponseDto>>.Failure(ErrorMessages.FileNotFound, 400);
                }

                var folderPath = Path.Combine(_env.WebRootPath, "uploads", ticketId.ToString());

                if (!Directory.Exists(folderPath))
                    Directory.CreateDirectory(folderPath);

                var attachments = new List<TicketAttachment>();

                foreach (var file in dto.Files)
                {
                    if (file.Length == 0)
                        continue;

                    var ext = Path.GetExtension(file.FileName);
                    var fileName = $"{Guid.NewGuid()}{ext}";
                    var fullPath = Path.Combine(folderPath, fileName);

                    using (var stream = new FileStream(fullPath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }

                    attachments.Add(new TicketAttachment
                    {
                        TicketId = ticketId,
                        UserId = userId,
                        FilePath = $"/uploads/{ticketId}/{fileName}",
                        UploadedAt = DateTime.UtcNow
                    });
                }

                if (!attachments.Any())
                {
                    return Result<List<TicketAttachmentResponseDto>>.Failure(ErrorMessages.FileNotFound, 400);
                }

                foreach (var attachment in attachments)
                {
                    await _unitOfWork.TicketAttachments.AddAsync(attachment);
                }
                await _unitOfWork.SaveChangesAsync();

                var attachmentDtos = _mapper.Map<List<TicketAttachmentResponseDto>>(attachments);

                _logger.LogInformation("{Count} attachments uploaded successfully for ticket {TicketId} by user {UserId}",
                    attachments.Count, ticketId, userId);

                return Result<List<TicketAttachmentResponseDto>>.Success(attachmentDtos, SuccessMessages.AttachmentUploaded);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading attachments for ticket {TicketId} by user {UserId}", ticketId, userId);
                return Result<List<TicketAttachmentResponseDto>>.Failure(ErrorMessages.ServerError, 500);
            }
        }

        public async Task<Result<TicketAttachmentResponseDto>> GetAttachmentAsync(int attachmentId)
        {
            try
            {
                var attachment = await _unitOfWork.TicketAttachments.GetByIdAsync(attachmentId);
                if (attachment == null)
                {
                    _logger.LogWarning("Attachment {AttachmentId} not found", attachmentId);
                    return Result<TicketAttachmentResponseDto>.Failure(ErrorMessages.FileNotFound, 404);
                }

                var fullPath = Path.Combine(_env.WebRootPath, attachment.FilePath.TrimStart('/'));
                if (!File.Exists(fullPath))
                {
                    _logger.LogWarning("File not found at {FullPath} for attachment {AttachmentId}", fullPath, attachmentId);
                    return Result<TicketAttachmentResponseDto>.Failure(ErrorMessages.PhysicalFileNotFound, 404);
                }

                var dto = _mapper.Map<TicketAttachmentResponseDto>(attachment);
                return Result<TicketAttachmentResponseDto>.Success(dto, SuccessMessages.RetrievedSuccessfully);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving attachment {AttachmentId}", attachmentId);
                return Result<TicketAttachmentResponseDto>.Failure(ErrorMessages.ServerError, 500);
            }
        }
    }
}
