using IMS_Application.DTOs;
using IMS_Application.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace IMS_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateUserDto dto)
        {
            await _userService.CreateUserAsync(dto, 1);
            return Ok("User created");
        }

        [HttpPut]
        public async Task<IActionResult> Update(UpdateUserDto dto)
        {
            await _userService.UpdateUserAsync(dto, 1);
            return Ok("User updated");
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var data = await _userService.GetAllUsersAsync();
            return Ok(data);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _userService.DeleteUserAsync(id, 1);
            return Ok("User deleted");
        }
    }
}
