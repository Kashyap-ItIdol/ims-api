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

                if (!Enum.TryParse<TicketType>(dto.TicketType, true, out var ticketType))
                    return Result<TicketResponseDto>.Failure(ErrorMessages.InvalidTicketType, 400);

                if (!Enum.TryParse<TicketPriority>(dto.Priority, true, out var ticketPriority))
                    return Result<TicketResponseDto>.Failure(ErrorMessages.InvalidTicketPriority, 400);

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
                return Result<TicketResponseDto>.Success(dto, SuccessMessages.TicketFetched);
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
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting ticket {TicketId} by user {DeletedBy}", ticketId, deletedBy);
                return Result<bool>.Failure(ErrorMessages.ServerError, 500);
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

                var tickets = await _unitOfWork.Tickets.SearchTicketsAsync(q, currentUserId, user.Role.Name);

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

                if (!string.IsNullOrWhiteSpace(dto.TicketTitle))
                    ticket.Title = dto.TicketTitle.Trim();

                if (dto.Description != null)
                    ticket.Description = dto.Description;

                if (dto.AssetId.HasValue)
                    ticket.AssetId = dto.AssetId.Value;

                if (dto.CategoryId.HasValue)
                    ticket.CategoryId = dto.CategoryId.Value;

                if (dto.SubCategoryId.HasValue)
                    ticket.SubCategoryId = dto.SubCategoryId.Value;

                if (dto.TicketType != null && dto.TicketType.Any())
                {
                    var ticketTypeString = dto.TicketType.First();
                    if (Enum.TryParse<TicketType>(ticketTypeString, true, out var ticketType))
                        ticket.TicketType = ticketType;
                }

                if (dto.TicketPriority != null && dto.TicketPriority.Any())
                {
                    var priorityString = dto.TicketPriority.First();
                    if (Enum.TryParse<TicketPriority>(priorityString, true, out var priority))
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
            catch (OperationCanceledException)
            {
                throw;
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
            catch (OperationCanceledException)
            {
                throw;
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
