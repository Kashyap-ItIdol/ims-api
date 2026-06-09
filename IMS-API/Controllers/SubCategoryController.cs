using IMS_API.Controllers.Base;
using IMS_Application.DTOs;
using IMS_Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IMS_API.Controllers
{
    [Route("api/settings/general/category/{categoryId}/SubCategory")]
    [ApiController]
    [Authorize(Roles = "Admin,Support Engineer")]
    public class SubCategoryController : BaseController
    {
        private readonly ISubCategoryService _subCategoryService;

        public SubCategoryController(ISubCategoryService subCategoryService)
        {
            _subCategoryService = subCategoryService;
        }

        [HttpPost("Create")]
        public async Task<IActionResult> Create(int categoryId, [FromQuery] string name)
        {
            var userResult = GetCurrentUserId();
            if (!userResult.IsSuccess)
                return FromResult(userResult);

            return FromResult(await _subCategoryService.CreateSubCategoryAsync(name, categoryId, userResult.Data));
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(int categoryId)
        {
            return FromResult(await _subCategoryService.GetSubCategoriesByCategoryIdAsync(categoryId));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int categoryId, int id, [FromBody] UpdateSubCategoryDto request)
        {
            var userResult = GetCurrentUserId();
            if (!userResult.IsSuccess)
                return FromResult(userResult);

            return FromResult(await _subCategoryService.UpdateSubCategoryAsync(id, request, userResult.Data));
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int categoryId, int id)
        {
            var userResult = GetCurrentUserId();
            if (!userResult.IsSuccess)
                return FromResult(userResult);

            return FromResult(await _subCategoryService.DeleteSubCategoryAsync(id, userResult.Data));
        }
    }
}