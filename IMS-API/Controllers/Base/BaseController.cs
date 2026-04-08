﻿using IMS_Application.Common.Models;
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

            return StatusCode(result.StatusCode,new
            {
                success = true,
                message = result.Message,
                data = result.Data
            });
        }

        protected void SetRefreshTokenCookie(string token, int? expireDays = null)
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict
            };

            if (expireDays.HasValue)
            {
                cookieOptions.Expires = DateTime.UtcNow.AddDays(expireDays.Value);
            }

            Response.Cookies.Append("refreshToken", token, cookieOptions);
        }

        protected void DeleteRefreshTokenCookie()
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict
            };

            Response.Cookies.Delete("refreshToken", cookieOptions);
        }
    }
}
