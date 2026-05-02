using IMS_Application.Interfaces;
using IMS_Application.Services;
using IMS_Application.Services.Interfaces;

using IMS_Infrastructure.Data;
using IMS_Infrastructure.Repositories;
using IMS_Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace IMS_Infrastructure.Extentions
{
    public static class ServiceCollectionExtentions
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
        {
            services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(config.GetConnectionString("DefaultConnection")));
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            services.AddScoped<IUserRepository, UserRepository>();

            services.AddScoped<ITicketRepository, TicketRepository>();
            services.AddScoped<ICategoryRepository, CategoryRepository>();
            services.AddScoped<ISubCategoryRepository, SubCategoryRepository>();
            services.AddScoped<ITokenService, TokenService>();

            services.AddScoped<ITicketService, TicketService>();

            return services;
        }
    }
}
