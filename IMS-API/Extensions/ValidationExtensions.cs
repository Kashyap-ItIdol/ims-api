using FluentValidation;
using FluentValidation.AspNetCore;
using IMS_Application.Validators;


namespace IMS_API.Extensions
{
    public static class ValidationExtensions
    {
        public static IServiceCollection AddValidation(this IServiceCollection services)
        {
            services.AddFluentValidationAutoValidation();

            services.AddValidatorsFromAssembly(typeof(LoginValidator).Assembly);

            return services;
        }
    }
}
