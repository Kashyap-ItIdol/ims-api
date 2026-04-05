using IMS_Application.Common.Models;
using Microsoft.AspNetCore.Mvc;

namespace IMS_API.Controllers.Base
{
    [ApiController]
    public class BaseController  : ControllerBase
    {
        protected IActionResult FromResult<T>(Result<T> result)
        {
            if (!result.IsSuccess)
            {
                return StatusCode(result.StatusCode, new
                {
                    success = false,
                    message = result.Message,
                    data = (object?)null
                });
            }

            return Ok(new
            {
                success = true,
                message = result.Message,
                data = result.Data
            });
        }
    }
}
