using IMS_API.Controllers.Base;
using IMS_Application.Common.Constants;
using IMS_Application.Common.Models;
using IMS_Application.DTOs;
using IMS_Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace IMS_API.Controllers
{
    [Route("api/settings/general/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin,SupportEngineer")]
    public class CategoryController : BaseController
    {
        private readonly ICategoryService _categoryService;

        public CategoryController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        [HttpPost("Create")]
        public async Task<IActionResult> Create([FromQuery] string name)
        {
            var userIdClaim = User.FindFirst("userId")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdClaim, out int createdBy))
            {
                return Unauthorized(new { success = false, message = ErrorMessages.UserNotFound });
            }

            var result = await _categoryService.CreateCategoryAsync(name, createdBy);
            return FromResult(result);
        }


        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _categoryService.GetAllCategoriesAsync();
            return FromResult(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _categoryService.GetCategoryByIdAsync(id);
            return FromResult(result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromQuery] string name)
        {
            var userIdClaim = User.FindFirst("userId")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdClaim, out int updatedBy))
            {
                var errorResult = Result<ListCategoriesDto>.Failure(ErrorMessages.UserNotFound, 401);
                return FromResult(errorResult);
            }

            var result = await _categoryService.UpdateCategoryAsync(id, name, updatedBy);
            return FromResult(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var userIdClaim = User.FindFirst("userId")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdClaim, out int updatedBy))
            {
                return Unauthorized(new { success = false, message = ErrorMessages.UserNotFound });
            }

            var result = await _categoryService.DeleteCategoryAsync(id, updatedBy);
            return FromResult(result);
        }
    }
}
