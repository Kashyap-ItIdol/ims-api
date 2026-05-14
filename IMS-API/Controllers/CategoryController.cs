using IMS_API.Controllers.Base;
using IMS_Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IMS_API.Controllers
{
    [Route("api/settings/general/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin,Support Engineer")]
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
            var userResult = GetCurrentUserId();
            if (!userResult.IsSuccess)
            {
                return FromResult(userResult);
            }
            return FromResult(await _categoryService.CreateCategoryAsync(name, userResult.Data));
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            return FromResult(await _categoryService.GetAllCategoriesAsync());
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            return FromResult(await _categoryService.GetCategoryByIdAsync(id));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromQuery] string name)
        {
            var userResult = GetCurrentUserId();
            if (!userResult.IsSuccess)
            {
                return FromResult(userResult);
            }
            return FromResult(await _categoryService.UpdateCategoryAsync(id, name, userResult.Data));
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var userResult = GetCurrentUserId();
            if (!userResult.IsSuccess)
            {
                return FromResult(userResult);
            }
            return FromResult(await _categoryService.DeleteCategoryAsync(id, userResult.Data));
        }
    }
}
