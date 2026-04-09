using System.Text;
using System.Text.Json;
using IMS_API.ExceptionHandlers;
using IMS_API.Extensions;
using IMS_Application.Extentions;
using IMS_Application.Interfaces;
using IMS_Application.Services;
using IMS_Application.Services.Interfaces;
using IMS_Infrastructure.Extentions;
using IMS_Infrastructure.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;

// 1. Create the Bootstrap Logger to catch startup errors
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Log.Information("Starting IMS API up...");

    var builder = WebApplication.CreateBuilder(args);

    // 2. Configure Serilog for the application (Reads from appsettings.json)
    builder.Host.UseSerilog((context, services, configuration) => configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext());

    // Add services to the container.
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(options =>
    {
        options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Name = "Authorization",
            Type = SecuritySchemeType.Http,
            Scheme = "Bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description = "Enter your JWT token. Example: eyJhbGci..."
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

    builder.Services.AddControllers();

    builder.Services.AddScoped<IAssetRepository, AssetRepository>();
    builder.Services.AddScoped<IAssetService, AssetService>();

    //  Validation Response Formatting (VERY IMPORTANT)
    builder.Services.Configure<ApiBehaviorOptions>(options =>
    {
        options.InvalidModelStateResponseFactory = context =>
        {
            var errors = context.ModelState
                .Where(x => x.Value?.Errors.Count > 0)
                .ToDictionary(
                    kvp => char.ToLowerInvariant(kvp.Key[0]) + kvp.Key.Substring(1),
                    kvp => kvp.Value!.Errors.Select(e => e.ErrorMessage).ToArray()
                );

            var response = new
            {
                success = false,
                message = "Validation failed",
                data = (object?)null,
                errors = errors
            };

            return new BadRequestObjectResult(response);
        };
    });

    builder.Services.AddAutoMapper(
        _ => { },
        typeof(ApplicationAssemblyMarker)
    );

    builder.Services.AddApplication();
    builder.Services.AddInfrastructure(builder.Configuration);
    builder.Services.AddValidation();
    builder.Services.AddProblemDetails();
    builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            var jwtKey = builder.Configuration["JwtSettings:Key"]
                ?? throw new InvalidOperationException("JWT Key is not configured");

            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,

                ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
                ValidAudience = builder.Configuration["JwtSettings:Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(jwtKey)),
                ClockSkew = TimeSpan.Zero
            };

            options.Events = new JwtBearerEvents
            {
                // Handles 401 Unauthorized (No token or expired token)
                OnChallenge = async context =>
                {
                    context.HandleResponse(); // Suppress the default empty response
                    context.Response.StatusCode = 401;
                    context.Response.ContentType = "application/json";

                    var result = JsonSerializer.Serialize(new
                    {
                        success = false,
                        message = "You are not authorized to access this resource. Please log in.",
                        data = (object?)null
                    });

                    await context.Response.WriteAsync(result);
                },

                // Handles 403 Forbidden (Valid token, but wrong Role)
                OnForbidden = async context =>
                {
                    context.Response.StatusCode = 403;
                    context.Response.ContentType = "application/json";

                    var result = JsonSerializer.Serialize(new
                    {
                        success = false,
                        message = "You do not have permission to perform this action.",
                        data = (object?)null
                    });

                    await context.Response.WriteAsync(result);
                }
            };
        });

    var app = builder.Build();

    // 3. Add Serilog Request Logging (Logs incoming HTTP requests concisely)
    app.UseSerilogRequestLogging();

    // Configure the HTTP request pipeline.
    app.UseExceptionHandler();

    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "IMS API V1");
        c.RoutePrefix = string.Empty;
    });

    app.UseHttpsRedirection();
    app.UseAuthentication();
    app.UseAuthorization();
    app.MapControllers();

    app.Run();
}
catch (Exception ex)
{
    // 4. Catch setup/DI errors (e.g., database connection issues on startup)
    Log.Fatal(ex, "IMS API terminated unexpectedly during startup");
}
finally
{
    // 5. Ensure all logs are flushed to sinks before shutting down
    Log.CloseAndFlush();
}