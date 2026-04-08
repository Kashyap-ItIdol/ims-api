using IMS_API.Controllers.Base;
using IMS_Application.Common.Constants;
using IMS_Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace IMS_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin,SupportEngineer")]
    public class CategoryController : BaseController
    {
        private readonly ICategoryService _categoryService;

        public CategoryController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        [HttpPost("settings/general/categories/Create")]
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
    }
}
