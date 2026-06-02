using IMS_Application.Common.Models;
using IMS_Application.DTOs;

namespace IMS_Application.Services.Interfaces
{
    public interface IReportService
    {
        Task<Result<TicketReportDto>> GetTicketReportsAsync(int currentUserId, DateTime? startDate, DateTime? endDate, int? supportEngineerId);
    }
}