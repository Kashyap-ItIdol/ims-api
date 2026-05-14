using AutoMapper;
using IMS_Application.Common.Constants;
using IMS_Application.Common.Models;
using IMS_Application.DTOs;
using IMS_Application.Interfaces;
using IMS_Application.Services.Interfaces;
using IMS_Domain.Constants;
using IMS_Domain.Entities;
using Microsoft.Extensions.Logging;


namespace IMS_Application.Services
{
    public class UserService : IUserService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<UserService> _logger;
        private readonly IEmailService _emailService;

        public UserService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<UserService> logger, IEmailService emailService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
            _emailService = emailService;
        }

        public async Task<Result<string>> CreateUserAsync(CreateUserDto dto, int currentUserId)
        {
            try
            {
                var email = dto.Email.Trim().ToLower();

                if (await _unitOfWork.Users.UserExistsAsync(email))
                    return Result<string>.Failure(ErrorMessages.UserAlreadyExists, 400);
                if (dto.RoleId == RoleConstants.Admin)
                    return Result<string>.Failure(ErrorMessages.CannotAssignAdminRole, 400);

                var user = _mapper.Map<User>(dto);

                user.Email = email;
                var defaultPassword = Guid.NewGuid().ToString("N")[..12];
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(defaultPassword);
                user.IsActive = true;
                user.IsDeleted = false;
                user.CreatedAt = DateTime.UtcNow;
                user.CreatedBy = currentUserId;

                await _unitOfWork.Users.AddAsync(user);
                await _unitOfWork.SaveChangesAsync();

                try
                {
                    var roleTitle = user.RoleId == RoleConstants.SupportEngineer ? "Support Engineer" : "Employee";
                    var emailResult = await _emailService.SendNewUserPasswordAsync(user.Email, user.FullName, defaultPassword, roleTitle);
                    if (!emailResult.IsSuccess)
                        _logger.LogWarning("New user password email failed for {Email}.", user.Email);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "New user password email failed for {Email}.", user.Email);
                }

                return Result<string>.Success(SuccessMessages.RegisterSuccess);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating user");
                return Result<string>.Failure(ErrorMessages.UnexpectedError, 500);
            }
        }

        public async Task<Result<string>> UpdateUserAsync(UpdateUserDto dto, int currentUserId)
        {
            try
            {
                var user = await _unitOfWork.Users.GetByIdAsync(dto.Id);

                if (user == null)
                    return Result<string>.Failure(ErrorMessages.UserNotFound, 404);

                if (user.RoleId != RoleConstants.Employee &&
                    user.RoleId != RoleConstants.SupportEngineer)
                {
                    return Result<string>.Failure(ErrorMessages.OnlyEmployeeOrSupportCanUpdate, 400);
                }

                if (dto.RoleId == RoleConstants.Admin)
                    return Result<string>.Failure(ErrorMessages.CannotAssignAdminRole, 400);

                var email = dto.Email?.Trim().ToLower();
                if (!string.IsNullOrEmpty(email) && email != user.Email)
                {
                    if (await _unitOfWork.Users.UserExistsAsync(email))
                        return Result<string>.Failure(ErrorMessages.UserAlreadyExists, 400);
                }

                _mapper.Map(dto, user);

                if (!string.IsNullOrEmpty(email))
                    user.Email = email;

                user.UpdatedAt = DateTime.UtcNow;
                user.UpdatedBy = currentUserId;

                _unitOfWork.Users.Update(user);
                await _unitOfWork.SaveChangesAsync();

                return Result<string>.Success(SuccessMessages.UserUpdatedSuccessfully);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user with id {UserId}", dto.Id);
                return Result<string>.Failure(ErrorMessages.UnexpectedError, 500);
            }
        }

        public async Task<Result<List<UserResponseDto>>> GetAllUsersAsync()
        {
            try
            {
                var users = await _unitOfWork.Users.GetAllWithRolesAsync();

                var result = users.Select((u, index) => new UserResponseDto
                {
                    Id = u.Id,
                    EmpCode = $"EMP-{(index + 1).ToString("D3")}",
                    FullName = u.FullName,
                    Email = u.Email,
                    Role = u.Role.Name,
                    Department = u.Department != null ? u.Department.Name : null,
                    IsActive = u.IsActive
                }).ToList();

                return Result<List<UserResponseDto>>.Success(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving users");
                return Result<List<UserResponseDto>>.Failure(ErrorMessages.UnexpectedError, 500);
            }
        }

        public async Task<Result<string>> DeleteUserAsync(int id, int currentUserId)
        {
            try
            {
                var user = await _unitOfWork.Users.GetByIdAsync(id);

                if (user == null)
                    return Result<string>.Failure(ErrorMessages.UserNotFound, 404);

                if (user.RoleId != RoleConstants.Employee &&
                    user.RoleId != RoleConstants.SupportEngineer)
                {
                    return Result<string>.Failure(ErrorMessages.OnlyEmployeeOrSupportCanDelete, 400);
                }

                user.IsDeleted = true;
                user.DeletedAt = DateTime.UtcNow;
                user.DeletedBy = currentUserId;

                _unitOfWork.Users.Update(user);
                await _unitOfWork.SaveChangesAsync();

                return Result<string>.Success(SuccessMessages.UserDeletedSuccessfully);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting user with id {UserId}", id);
                return Result<string>.Failure(ErrorMessages.UnexpectedError, 500);
            }
        }
        public async Task<Result<UserOverviewResponseDto>> GetUserOverviewByIdAsync(int id)
        {
            try
            {
                var user = await _unitOfWork.Users.GetByIdAsync(id);
                if (user == null)
                    return Result<UserOverviewResponseDto>.Failure(ErrorMessages.UserNotFound, 404);

                var userDto = _mapper.Map<UserResponseDto>(user);

                var assignedAssetIdsFromAssignments = (await _unitOfWork.AssetAssignments.GetAllAsync())
                    .Where(a => a.EmployeeId == id)
                    .Select(a => a.AssetId)
                    .Distinct()
                    .ToList();

                var assignedAssetIdsFromAssetTable = (await _unitOfWork.Assets.GetAllAsync())
                    .Where(a => a.AssignedTo == id)
                    .Select(a => a.Id)
                    .Distinct()
                    .ToList();

                var assignedAssetIds = assignedAssetIdsFromAssignments
                    .Concat(assignedAssetIdsFromAssetTable)
                    .Distinct()
                    .ToList();

                var assignedAssets = (await _unitOfWork.Assets.GetAllAsync())
                    .Where(a => assignedAssetIds.Contains(a.Id))
                    .ToList();

                var assetDtos = _mapper.Map<List<AssetResponseDto>>(assignedAssets);

                var roleName = user.Role?.Name ?? string.Empty;
                var tickets = await _unitOfWork.Tickets.GetTicketsForUserAsync(id, roleName);

                tickets = tickets.Where(t => t.CreatedBy == id).ToList();

                var ticketDtos = new List<TicketResponseDto>();
                foreach (var ticket in tickets)
                {
                    var creator = new UserInfo
                    {
                        id = ticket.CreatedBy,
                        name = user.FullName
                    };

                    var latestAssign = ticket.TicketAssignments?
                        .Where(a => a.status == IMS_Application.Common.Constants.LogicStrings.Active)
                        .OrderByDescending(a => a.assigned_at)
                        .FirstOrDefault();

                    UserInfo? assignedToInfo = null;
                    if (latestAssign != null)
                    {
                        assignedToInfo = new UserInfo { id = latestAssign.assignedTo, name = LogicStrings.Unassigned };
                    }

                    var mappedTicketInfo = _mapper.Map<TicketInfo>(ticket);

                    mappedTicketInfo.createdBy = creator;
                    mappedTicketInfo.assignedTo = assignedToInfo;

                    var dto = new TicketResponseDto
                    {
                        ticket = mappedTicketInfo,
                        comments = _mapper.Map<List<TicketCommentInfo>>(ticket.Comments ?? new List<TicketComment>()),
                        attachments = _mapper.Map<List<TicketAttachmentInfo>>(ticket.Attachments ?? new List<TicketAttachment>())
                    };

                    ticketDtos.Add(dto);
                }

                return Result<UserOverviewResponseDto>.Success(new UserOverviewResponseDto
                {
                    user = userDto,
                    assignedAssets = assetDtos,
                    createdTickets = ticketDtos
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user overview for id {UserId}", id);
                return Result<UserOverviewResponseDto>.Failure(ErrorMessages.UnexpectedError, 500);
            }
        }
    }
}

