using IMS_API.Controllers.Base;
using IMS_Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IMS_API.Controllers
{
    [Route("api/settings")]
    [ApiController]
    [Authorize]
    public class SettingsController : BaseController
    {
        private readonly ISettingService _settingService;

        public SettingsController(ISettingService settingService)
        {
            _settingService = settingService;
        }

        [HttpGet("recentActivities")]
        public async Task<IActionResult> GetRecentActivities(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            return FromResult(await _settingService.GetRecentActivitiesAsync(pageNumber, pageSize));
        }

        [HttpGet("recentDeletedActivities")]
        public async Task<IActionResult> GetRecentDeletedActivities(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            return FromResult(await _settingService.GetRecentDeletedActivitiesAsync(pageNumber, pageSize));
        }
    }
}