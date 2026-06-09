using IMS_API.Controllers.Base;
using IMS_Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IMS_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ReportsController : BaseController
    {
        private readonly IReportService _reportService;

        public ReportsController(IReportService reportService)
        {
            _reportService = reportService;
        }

        [HttpGet("tickets/reports")]
        public async Task<IActionResult> GetTicketReports(
            [FromQuery] DateTime? start_date = null,
            [FromQuery] DateTime? end_date = null,
            [FromQuery] int? support_engineer_id = null)
        {
            var userIdResult = GetCurrentUserId();
            if (!userIdResult.IsSuccess)
                return FromResult(userIdResult);

            return FromResult(await _reportService.GetTicketReportsAsync(
                userIdResult.Data, start_date, end_date, support_engineer_id));
        }
    }
}