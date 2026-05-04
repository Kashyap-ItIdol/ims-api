using FluentValidation;
using IMS_Application.Services;
using IMS_Application.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace IMS_Application.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IDepartmentService, DepartmentService>();
            services.AddScoped<ITicketService, TicketService>();
            services.AddScoped<ICategoryService, CategoryService>();
            services.AddScoped<ISubCategoryService, SubCategoryService>();
            services.AddScoped<IClientAssetService, ClientAssetService>();
            services.AddScoped<IAssetAssignmentService, AssetAssignmentService>();

            // Add FluentValidation validators
            services.AddValidatorsFromAssembly(typeof(ServiceCollectionExtensions).Assembly);

            return services;
        }
    }
}
