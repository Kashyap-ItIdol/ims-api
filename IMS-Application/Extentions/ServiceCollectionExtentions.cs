using IMS_Application.Services;
using IMS_Application.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace IMS_Application.Extentions
{
    public static class ServiceCollectionExtentions
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IDepartmentService, DepartmentService>();

            //services.AddScoped<IRoleService, RoleService>();
            
            services.AddScoped<ITicketService, TicketService>();
            services.AddScoped<ICategoryService, CategoryService>();

            return services;
        }
    }
}
