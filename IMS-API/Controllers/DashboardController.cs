using IMS_Application.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace IMS_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DashboardController : ControllerBase
    {
        private readonly IDashboardService _dashboardService;

        public DashboardController(
            IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        [HttpGet]
        public async Task<IActionResult> GetDashboard([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
        {
            var result = await _dashboardService.GetDashboardAsync();
            return Ok(result);
        }

    }
}
