using IMS_API.ExceptionHandlers;
using IMS_Application.Extentions;
using IMS_Application.Interfaces;
using IMS_Infrastructure.Data.Configurations;
using IMS_Infrastructure.Extentions;
using IMS_Infrastructure.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Filters;
using IMS_API.Swagger;
using Serilog;
using System.Text;
using System.Text.Json;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Log.Information("Starting IMS API up...");

    var builder = WebApplication.CreateBuilder(args);

    ConfigureBuilder(builder);
    ConfigureServices(builder.Services, builder.Configuration);
    ConfigureAuthentication(builder.Services, builder.Configuration);

    var app = builder.Build();
    
    ConfigureMiddleware(app);
    InitializeDatabase(app);

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "IMS API terminated unexpectedly during startup");
}
finally
{
    Log.CloseAndFlush();
}

static void ConfigureBuilder(WebApplicationBuilder builder)
{
    builder.Host.UseSerilog((context, services, configuration) => configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext());
}

static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
{
    services.AddEndpointsApiExplorer();
    ConfigureSwagger(services);
    ConfigureControllers(services);
    ConfigureAutoMapper(services);
    
    services.AddApplication();
    services.AddInfrastructure(configuration);
    services.AddValidation();
    services.AddProblemDetails();
    services.AddExceptionHandler<GlobalExceptionHandler>();
}
static void ConfigureSwagger(IServiceCollection services)
{
    services.AddSwaggerGen(options =>
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
                new[] { "readAccess", "writeAccess" }
            }
        });

        // Add support for file uploads
        options.OperationFilter<FileUploadOperationFilter>();
    });
}

static void ConfigureControllers(IServiceCollection services)
{
    services.AddControllers()
        .AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
            options.JsonSerializerOptions.WriteIndented = true;
        });

    services.Configure<ApiBehaviorOptions>(options =>
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
}

static void ConfigureAutoMapper(IServiceCollection services)
{
    services.AddAutoMapper(
        _ => { },
        System.Reflection.Assembly.GetExecutingAssembly(),
        typeof(IMS_Application.Extentions.ApplicationAssemblyMarker).Assembly
    );
}

static void ConfigureAuthentication(IServiceCollection services, IConfiguration configuration)
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
                        message = "You are not authorized to access this resource. Please log in.",
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
                        message = "You do not have permission to perform this action.",
                        data = (object?)null
                    });

                    await context.Response.WriteAsync(result);
                }
            };
        });
}

static void ConfigureMiddleware(WebApplication app)
{
    app.UseSerilogRequestLogging();
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
}

static void InitializeDatabase(WebApplication app)
{
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    
    try
    {
        context.Database.Migrate();
        Console.WriteLine("Database migrations applied successfully");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Database migration error: {ex.Message}");
    }

    // Seed email templates (temporarily disabled until EmailTemplates table exists)
    // var emailTemplateRepository = scope.ServiceProvider.GetRequiredService<IEmailTemplateRepository>();
    // await EmailTemplateSeeder.SeedAsync(emailTemplateRepository);
}