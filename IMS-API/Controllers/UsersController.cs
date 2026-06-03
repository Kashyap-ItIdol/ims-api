using IMS_API.Controllers.Base;
using IMS_Application.DTOs;
using IMS_Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IMS_API.Controllers
{
    [Authorize(Roles = "Admin")]
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : BaseController
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        [AllowAnonymous]
        [HttpGet("me")]
        public async Task<IActionResult> GetMyProfile()
        {
            var userResult = GetCurrentUserId();
            if (!userResult.IsSuccess)
                return FromResult(userResult);

            return FromResult(await _userService.GetMyProfileAsync(userResult.Data));
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateUserDto dto)
        {
            var userResult = GetCurrentUserId();
            if (!userResult.IsSuccess)
                return FromResult(userResult);

            return FromResult(await _userService.CreateUserAsync(dto, userResult.Data));
        }

        [HttpPut]
        public async Task<IActionResult> Update(UpdateUserDto dto)
        {
            var userResult = GetCurrentUserId();
            if (!userResult.IsSuccess)
                return FromResult(userResult);

            return FromResult(await _userService.UpdateUserAsync(dto, userResult.Data));
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            return FromResult(await _userService.GetAllUsersAsync());
        }

        [HttpGet("{id}/overview")]
        public async Task<IActionResult> GetOverviewById(int id)
        {
            return FromResult(await _userService.GetUserOverviewByIdAsync(id));
        }

        [HttpGet("{id}/activity")]
        public async Task<IActionResult> GetActivityById(int id, [FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
        {
            return FromResult(await _userService.GetUserActivitiesByIdAsync(id, startDate, endDate));
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var userResult = GetCurrentUserId();
            if (!userResult.IsSuccess)
                return FromResult(userResult);

            return FromResult(await _userService.DeleteUserAsync(id, userResult.Data));
        }

        [HttpPost("filter")]
        public async Task<IActionResult> Filter([FromBody] UserFilterDto filter)
        {
            return FromResult(await _userService.FilterUsersAsync(filter ?? new UserFilterDto()));
        }

        [HttpGet("filter/options")]
        public async Task<IActionResult> GetFilterOptions()
        {
            return FromResult(await _userService.GetUserFilterOptionsAsync());
        }

        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] string query)
        {
            return FromResult(await _userService.SearchUsersAsync(query));
        }
    }
}