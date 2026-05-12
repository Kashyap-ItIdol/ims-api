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

        public async Task<Result<PagedResult<RecentActivityItemDto>>> GetRecentActivitiesAsync(int pageNumber, int pageSize)
        {
            if (pageNumber < 1 || pageSize < 1)
                return Result<PagedResult<RecentActivityItemDto>>.Failure(ErrorMessages.InvalidPagination, 400);

            try
            {
                var activities = await _settingRepository.GetRecentActivitiesAsync(pageNumber, pageSize);
                var items = activities
                    .OrderByDescending(x => x.DateTime)
                    .ToList();

                var pagedResult = new PagedResult<RecentActivityItemDto>
                {
                    Items = _mapper.Map<List<RecentActivityItemDto>>(items),
                    TotalCount = await _settingRepository.GetRecentActivitiesTotalCountAsync(),
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

        public async Task<Result<PagedResult<RecentActivityItemDto>>> GetRecentDeletedActivitiesAsync(int pageNumber, int pageSize)
        {
            if (pageNumber < 1 || pageSize < 1)
                return Result<PagedResult<RecentActivityItemDto>>.Failure(ErrorMessages.InvalidPagination, 400);

            try
            {
                var activities = await _settingRepository.GetDeletedRecentActivitiesAsync(pageNumber, pageSize);

                var items = activities
                    .OrderByDescending(x => x.DateTime)
                    .ToList();

                var pagedResult = new PagedResult<RecentActivityItemDto>
                {
                    Items = _mapper.Map<List<RecentActivityItemDto>>(items),
                    TotalCount = await _settingRepository.GetDeletedRecentActivitiesTotalCountAsync(),
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



