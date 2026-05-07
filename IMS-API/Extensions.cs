using IMS_API.ExceptionHandlers;
using IMS_Application.Extentions;
using IMS_Application.Interfaces;
using IMS_Application.Services;
using IMS_Application.Services.Interfaces;
using IMS_Infrastructure.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using System.Text.Json;

namespace IMS_API
{
    public static class Extensions
    {
        public static IServiceCollection AddProjectServices(
            this IServiceCollection services)
        {
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IUserRepository, UserRepository>();

            services.AddScoped<IAssetRepository, AssetRepository>();
            services.AddScoped<IAssetService, AssetService>();

            return services;
        }

        public static IServiceCollection AddSwaggerDocumentation(
            this IServiceCollection services)
        {
            services.AddEndpointsApiExplorer();

            services.AddSwaggerGen(options =>
            {
                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "Enter JWT Token"
                });

                options.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        Array.Empty<string>()
                    }
                });
            });

            return services;
        }

        public static IServiceCollection AddValidationResponseFormat(
            this IServiceCollection services)
        {
            services.Configure<ApiBehaviorOptions>(options =>
            {
                options.InvalidModelStateResponseFactory = context =>
                {
                    var errors = context.ModelState
                        .Where(x => x.Value?.Errors.Count > 0)
                        .ToDictionary(
                            kvp => string.IsNullOrWhiteSpace(kvp.Key)
                                ? "general"
                                : char.ToLowerInvariant(kvp.Key[0]) + kvp.Key.Substring(1),
                            kvp => kvp.Value!.Errors
                                .Select(e => e.ErrorMessage)
                                .ToArray()
                        );

                    var response = new
                    {
                        success = false,
                        message = "Validation failed",
                        data = (object?)null,
                        errors
                    };

                    return new BadRequestObjectResult(response);
                };
            });

            return services;
        }

        public static IServiceCollection AddJwtAuthentication(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    var jwtKey = configuration["JwtSettings:Key"]
                        ?? throw new InvalidOperationException("JWT Key is not configured");

                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,

                        ValidIssuer = configuration["JwtSettings:Issuer"],
                        ValidAudience = configuration["JwtSettings:Audience"],

                        IssuerSigningKey = new SymmetricSecurityKey(
                            Encoding.UTF8.GetBytes(jwtKey)),

                        ClockSkew = TimeSpan.Zero
                    };

                    options.Events = new JwtBearerEvents
                    {
                        OnChallenge = async context =>
                        {
                            context.HandleResponse();

                            context.Response.StatusCode = 401;
                            context.Response.ContentType = "application/json";

                            var result = JsonSerializer.Serialize(new
                            {
                                success = false,
                                message = "Unauthorized access",
                                data = (object?)null
                            });

                            await context.Response.WriteAsync(result);
                        },

                        OnForbidden = async context =>
                        {
                            context.Response.StatusCode = 403;
                            context.Response.ContentType = "application/json";

                            var result = JsonSerializer.Serialize(new
                            {
                                success = false,
                                message = "Access denied",
                                data = (object?)null
                            });

                            await context.Response.WriteAsync(result);
                        }
                    };
                });

            return services;
        }

        public static IServiceCollection AddExceptionHandling(
            this IServiceCollection services)
        {
            services.AddProblemDetails();
            services.AddExceptionHandler<GlobalExceptionHandler>();

            return services;
        }

        public static IServiceCollection AddMapping(
            this IServiceCollection services)
        {
            services.AddAutoMapper(
                _ => { },
                typeof(ApplicationAssemblyMarker)
            );

            return services;
        }
    }
}