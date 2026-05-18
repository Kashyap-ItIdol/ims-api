using IMS_API.Services;
using IMS_Application.Interfaces;
using IMS_Application.Services;
using IMS_Application.Services.Interfaces;

namespace IMS_API.Extensions
{
    public static class ServiceCollectionExtentions
    {
        public static IServiceCollection AddApiServices(this IServiceCollection services)
        {
            services.AddScoped<INotificationDispatcher, NotificationDispatcher>();
            services.AddScoped<INotificationService, NotificationService>();

            return services;
        }
    }
}

