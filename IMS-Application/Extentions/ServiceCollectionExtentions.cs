using IMS_Application.Services;
using IMS_Application.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace IMS_Application.Extentions
{
    public static class ServiceCollectionExtentions
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IRoleService, RoleService>();

            return services;
        }
    }
}
