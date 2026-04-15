using IMS_API.Controllers.Base;
using IMS_Application.Common.Constants;
using IMS_Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace IMS_API.Controllers
{
    [Route("api/settings/general/category/{categoryId}/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin,SupportEngineer")]
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
            var userIdClaim = User.FindFirst("userId")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdClaim, out int createdBy))
            {
                return Unauthorized(new { success = false, message = ErrorMessages.UserNotFound });
            }

            var result = await _subCategoryService.CreateSubCategoryAsync(name, categoryId, createdBy);
            return FromResult(result);
        }
    }
}
