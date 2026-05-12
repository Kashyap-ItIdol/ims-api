using IMS_API.Controllers.Base;
using IMS_Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IMS_API.Controllers
{
    [Route("api/settings/general/category/{categoryId}/[controller]")]
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
        public async Task<IActionResult> Create([FromQuery] string name, int categoryId)
        {
            var userResult = GetCurrentUserId();
            if (!userResult.IsSuccess)
            {
                return FromResult(userResult);
            }
            return FromResult(await _subCategoryService.CreateSubCategoryAsync(name, categoryId, userResult.Data));
        }
    }
}
