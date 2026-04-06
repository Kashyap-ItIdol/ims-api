using IMS_API.Controllers.Base;
using IMS_Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IMS_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class DepartmentController : BaseController
    {
        private readonly IDepartmentService _departmentService;

        public DepartmentController(
            IDepartmentService departmentService)
        {
            _departmentService = departmentService;
        }

        [HttpGet("lookup")]
        public async Task<IActionResult> GetAllDepartments()
        {
            var result = await _departmentService.GetAllDepartmentsAsync();
            return FromResult(result);
        }
    }
}
