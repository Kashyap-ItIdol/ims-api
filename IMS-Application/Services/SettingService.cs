using AutoMapper;
using IMS_Application.Common.Constants;
using IMS_Application.Common.Models;
using IMS_Application.DTOs;
using IMS_Application.Interfaces;
using IMS_Application.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace IMS_Application.Services
{
    public class SettingService : ISettingService
    {
        private readonly ISettingRepository _settingRepository;
        private readonly ILogger<SettingService> _logger;
        private readonly IMapper _mapper;

        public SettingService(
            ISettingRepository settingRepository,
            ILogger<SettingService> logger,
            IMapper mapper)
        {
            _settingRepository = settingRepository;
            _logger = logger;
            _mapper = mapper;
        }

        private static Func<IMS_Domain.Entities.RecentActivity, bool> ApplySearchFilter(string? search, bool includeUser = true)
        {
            if (string.IsNullOrWhiteSpace(search))
                return _ => true;

            var q = search.Trim();

            int? itemId = null;
            if (int.TryParse(q, out var parsedId))
                itemId = parsedId;
            return x =>
                (x.ItemName != null && x.ItemName.Contains(q)) ||
                (x.Action != null && x.Action.Contains(q)) ||
                (x.Details != null && x.Details.Contains(q)) ||
                (itemId.HasValue && x.ItemId == itemId.Value) ||
                (includeUser && x.User != null && x.User.FullName != null && x.User.FullName.Contains(q));
        }

        public async Task<Result<PagedResult<RecentActivityItemDto>>> GetRecentActivitiesAsync(int pageNumber, int pageSize, string? search)
        {
            if (pageNumber < 1 || pageSize < 1)
                return Result<PagedResult<RecentActivityItemDto>>.Failure(ErrorMessages.InvalidPagination, 400);

            try
            {
                var activities = await _settingRepository.GetRecentActivitiesAsync(pageNumber, pageSize, search);

                var filter = ApplySearchFilter(search, includeUser: true);
                var filteredItems = activities
                    .Where(filter)
                    .OrderByDescending(x => x.DateTime)
                    .ToList();

                var pagedResult = new PagedResult<RecentActivityItemDto>
                {
                    Items = _mapper.Map<List<RecentActivityItemDto>>(filteredItems),
                    TotalCount = await _settingRepository.GetRecentActivitiesTotalCountAsync(search),
                    PageNumber = pageNumber,
                    PageSize = pageSize
                };

                return Result<PagedResult<RecentActivityItemDto>>.Success(pagedResult, SuccessMessages.RetrievedSuccessfully);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving recent activities. pageNumber={PageNumber} pageSize={PageSize}", pageNumber, pageSize);

                return Result<PagedResult<RecentActivityItemDto>>.Failure(ErrorMessages.ServerError, 500);
            }
        }

        public async Task<Result<PagedResult<RecentActivityItemDto>>> GetRecentDeletedActivitiesAsync(int pageNumber, int pageSize, string? search)
        {
            if (pageNumber < 1 || pageSize < 1)
                return Result<PagedResult<RecentActivityItemDto>>.Failure(ErrorMessages.InvalidPagination, 400);

            try
            {
                var allDeletedActivities = await _settingRepository.GetDeletedRecentActivitiesAsync(1, int.MaxValue, null);

                var filter = ApplySearchFilter(search, includeUser: true);
                var filteredItems = allDeletedActivities
                    .Where(filter)
                    .OrderByDescending(x => x.DateTime)
                    .ToList();

                var paged = filteredItems
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                var pagedResult = new PagedResult<RecentActivityItemDto>
                {
                    Items = _mapper.Map<List<RecentActivityItemDto>>(paged),
                    TotalCount = filteredItems.Count,
                    PageNumber = pageNumber,
                    PageSize = pageSize
                };

                return Result<PagedResult<RecentActivityItemDto>>.Success(pagedResult, SuccessMessages.RetrievedSuccessfully);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving recent deleted activities. pageNumber={PageNumber} pageSize={PageSize}", pageNumber, pageSize);

                return Result<PagedResult<RecentActivityItemDto>>.Failure(ErrorMessages.ServerError, 500);
            }
        }
    }
}