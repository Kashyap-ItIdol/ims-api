﻿using IMS_API.Controllers.Base;
using IMS_Application.DTOs;
using IMS_Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

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

        [HttpPost]
        public async Task<IActionResult> Create(CreateUserDto dto)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

            var result = await _userService.CreateUserAsync(dto, userId);
            return FromResult(result);
        }

        [HttpPut]
        public async Task<IActionResult> Update(UpdateUserDto dto)
        {
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

            var result = await _userService.UpdateUserAsync(dto, currentUserId);
            return FromResult(result);
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
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

            var result = await _userService.DeleteUserAsync(id, userId);
            return FromResult(result);
        }
    }
}
