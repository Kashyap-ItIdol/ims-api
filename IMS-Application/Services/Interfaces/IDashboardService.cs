using IMS_Application.DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace IMS_Application.Services.Interfaces
{
    public interface IDashboardService
    {
        Task<DashboardStatsDto> GetStatsAsync(DateTime? startDate, DateTime? endDate);

        Task<List<TicketChartDto>> GetTicketChartAsync();

        Task<List<RecentTicketDto>> GetRecentTicketsAsync();

        Task<DashboardResponseDto> GetDashboardAsync();
    }
}

