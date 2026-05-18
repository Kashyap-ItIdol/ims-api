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
                    IsDeleted = u.IsDeleted
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

        public async Task<Result<List<UserResponseDto>>> SearchUsersAsync(string query)
        {
            try
            {
                query ??= string.Empty;
                query = query.Trim();

                var users = await _unitOfWork.Users.GetAllWithRolesAsync();

                var isNumeric = int.TryParse(query, out var userId);

                var filtered = users.Where(u =>
                    (isNumeric && u.Id == userId) ||
                    (!string.IsNullOrEmpty(u.FullName) && u.FullName.Contains(query, StringComparison.OrdinalIgnoreCase)) ||
                    (!string.IsNullOrEmpty(u.Email) && u.Email.Contains(query, StringComparison.OrdinalIgnoreCase)) ||
                    (u.Department != null && !string.IsNullOrEmpty(u.Department.Name) &&
                     u.Department.Name.Contains(query, StringComparison.OrdinalIgnoreCase)) ||
                    (u.Role != null && !string.IsNullOrEmpty(u.Role.Name) &&
                     u.Role.Name.Contains(query, StringComparison.OrdinalIgnoreCase))
                ).ToList();

                return Result<List<UserResponseDto>>.Success(filtered.Select((u, index) => new UserResponseDto
                {
                    Id = u.Id,
                    EmpCode = $"EMP-{(index + 1).ToString("D3")}",
                    FullName = u.FullName,
                    Email = u.Email,
                    Role = u.Role?.Name ?? string.Empty,
                    Department = u.Department != null ? u.Department.Name : null,
                    IsDeleted = u.IsDeleted
                }).ToList());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching users");
                return Result<List<UserResponseDto>>.Failure(ErrorMessages.UnexpectedError, 500);
            }
        }

        public async Task<Result<List<UserResponseDto>>> FilterUsersAsync(UserFilterDto filter)
        {
            try
            {
                filter ??= new UserFilterDto();


                // Repository is intentionally kept simple; apply the filter logic in the service.
                var users = await _unitOfWork.Users.FilterAsync(filter);

                if (filter.FullNames != null && filter.FullNames.Count > 0 && !filter.FullNames.Contains("all"))
                    users = users.Where(u => filter.FullNames!.Contains(u.FullName)).ToList();

                if (filter.RoleKeys != null && filter.RoleKeys.Count > 0 && !filter.RoleKeys.Contains("all"))
                {
                    var wantEmployee = filter.RoleKeys.Contains("employee");
                    var wantSupport = filter.RoleKeys.Contains("support-engineer");

                    if (wantEmployee && !wantSupport)
                        users = users.Where(u => u.RoleId == RoleConstants.Employee).ToList();
                    else if (!wantEmployee && wantSupport)
                        users = users.Where(u => u.RoleId == RoleConstants.SupportEngineer).ToList();
                    // if both selected => no extra filter
                }

                if (filter.DepartmentNames != null && filter.DepartmentNames.Count > 0 && !filter.DepartmentNames.Contains("all"))
                    users = users.Where(u => u.Department != null && filter.DepartmentNames!.Contains(u.Department.Name)).ToList();

                // Status filter -> IsDeleted
                if (filter.StatusKeys != null && filter.StatusKeys.Count > 0 && !filter.StatusKeys.Contains("all"))
                {
                    var wantActive = filter.StatusKeys.Contains("active");
                    var wantInactive = filter.StatusKeys.Contains("inactive");

                    if (wantActive && !wantInactive)
                        users = users.Where(u => !u.IsDeleted).ToList();
                    else if (!wantActive && wantInactive)
                        users = users.Where(u => u.IsDeleted).ToList();
                    // if both selected => no extra filter
                }

                // mapping should match GetAllUsersAsync
                return Result<List<UserResponseDto>>.Success(users.Select((u, index) => new UserResponseDto
                {
                    Id = u.Id,
                    EmpCode = $"EMP-{(index + 1).ToString("D3")}",
                    FullName = u.FullName,
                    Email = u.Email,
                    Role = u.Role?.Name ?? string.Empty,
                    Department = u.Department != null ? u.Department.Name : null,
                    IsDeleted = u.IsDeleted
                }).ToList());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error filtering users");
                return Result<List<UserResponseDto>>.Failure(ErrorMessages.UnexpectedError, 500);
            }
        }

        public async Task<Result<UserFilterOptionsDto>> GetUserFilterOptionsAsync()
        {
            try
            {
                var users = await _unitOfWork.Users.GetAllWithRolesAsync();

                var fullNames = users.Select(u => u.FullName)
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .Distinct()
                    .OrderBy(x => x)
                    .Select(x => new UserFilterOptionsDto.CheckboxOption { Key = x, Label = x })
                    .ToList();

                var roles = new List<UserFilterOptionsDto.CheckboxOption>
                {
                    new() { Key = "all", Label = "All" },
                    new() { Key = "employee", Label = "Employee" },
                    new() { Key = "support-engineer", Label = "Support Engineer" },
                };

                var departments = users
                    .Where(u => u.Department != null)
                    .Select(u => u.Department!.Name)
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .Distinct()
                    .OrderBy(x => x)
                    .Select(x => new UserFilterOptionsDto.CheckboxOption { Key = x, Label = x })
                    .ToList();

                departments.Insert(0, new UserFilterOptionsDto.CheckboxOption { Key = "all", Label = "All" });

                var statuses = new List<UserFilterOptionsDto.CheckboxOption>
                {
                    new() { Key = "all", Label = "All" },
                    new() { Key = "active", Label = "Active" },
                    new() { Key = "inactive", Label = "Inactive" },
                };

                return Result<UserFilterOptionsDto>.Success(new UserFilterOptionsDto
                {
                    FullNames = new() { Options = fullNames },
                    Roles = new() { Options = roles },
                    Departments = new() { Options = departments },
                    Statuses = new() { Options = statuses }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user filter options");
                return Result<UserFilterOptionsDto>.Failure(ErrorMessages.UnexpectedError, 500);
            }
        }
    }
}

