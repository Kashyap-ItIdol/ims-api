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
            catch (OperationCanceledException)
            {
                throw;
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
            catch (OperationCanceledException)
            {
                throw;
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
            catch (OperationCanceledException)
            {
                throw;
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
            catch (OperationCanceledException)
            {
                throw;
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
            catch (OperationCanceledException)
            {
                throw;
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
            catch (OperationCanceledException)
            {
                throw;
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
            catch (OperationCanceledException)
            {
                throw;
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

                var user = await _unitOfWork.Users.GetByIdAsync(currentUserId);
                if (user == null)
                    return Result<TicketResponseDto>.Failure(ErrorMessages.UserNotFoundError, 404);

                if (user.Role == null)
                    return Result<TicketResponseDto>.Failure(ErrorMessages.RoleNotFoundError, 400);

                bool hasAccess = user.Role.Name switch
                {
                    LogicStrings.AdminRole => true,
                    LogicStrings.SupportEngineerRole => ticket.TicketAssignments.Any(a => a.status == LogicStrings.Active && a.assignedTo == currentUserId),
                    _ => ticket.CreatedBy == currentUserId
                };

                if (!hasAccess)
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
