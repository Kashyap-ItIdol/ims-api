using IMS_API.ExceptionHandlers;
using IMS_API.Extensions;
using IMS_API.Hubs;
using IMS_Application.Extentions;
using IMS_Application.Interfaces;
using IMS_Infrastructure.Data.Configurations;
using IMS_Infrastructure.Extentions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Log.Information("Starting IMS API up...");

    var builder = WebApplication.CreateBuilder(args);

    var certPath = Environment.GetEnvironmentVariable("IMS_CERT_PATH")
    ?? "/home/demo/ims-api/certs/api.pfx";

    var certPassword = Environment.GetEnvironmentVariable("IMS_CERT_PASSWORD")
        ?? "demo";

    builder.WebHost.ConfigureKestrel(options =>
    {
        // Always enable HTTP so the API can start even if HTTPS cert is missing.
        options.ListenAnyIP(5224);

        try
        {
            if (!File.Exists(certPath))
            {
                Log.Warning("HTTPS certificate not found at path: {CertPath}. Starting HTTP-only.", certPath);
                return;
            }

            var cert = new X509Certificate2(certPath, certPassword);

            // HTTPS
            options.ListenAnyIP(5001, listenOptions =>
            {
                listenOptions.UseHttps(cert);
            });
        }
        catch (Exception certEx)
        {
            Log.Warning(certEx, "Failed to load HTTPS certificate from {CertPath}. Starting HTTP-only.", certPath);
            // Keep HTTP-only.
        }
    });



    builder.Host.UseSerilog((context, services, configuration) => configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext());

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
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("DevCors", policy =>
        {
            policy
                .SetIsOriginAllowed(origin => true)
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials();
        });
    });


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
    builder.Services.AddApiServices();

    // SignalR for NotificationDispatcher/NotificationHub
    builder.Services.AddSignalR();

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
                OnMessageReceived = context =>
                {
                    var accessToken = context.Request.Query["access_token"];
                    var path = context.HttpContext.Request.Path;

                    if (!string.IsNullOrEmpty(accessToken) &&
                        path.StartsWithSegments("/notifications"))
                    {
                        context.Token = accessToken;
                    }

                    return Task.CompletedTask;
                },

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

    var app = builder.Build();

    using (var scope = app.Services.CreateScope())
    {
        // Seed email templates during startup, but never crash the whole API if DB/auth is not ready.
        // Disable by setting: IMS_SEED_EMAIL_TEMPLATES=false
        var seedEnabled = app.Configuration.GetValue("IMS_SEED_EMAIL_TEMPLATES", true);
        if (seedEnabled)
        {
            try
            {
                var emailTemplateRepository = scope.ServiceProvider.GetRequiredService<IEmailTemplateRepository>();
                await EmailTemplateSeeder.SeedAsync(emailTemplateRepository);
            }
            catch (Exception seedEx)
            {
                Log.Warning(seedEx, "Email template seeding failed. Continuing startup.");
            }
        }
    }

    ConfigureMiddleware(app);

static void ConfigureMiddleware(WebApplication app)
{
    app.UseSerilogRequestLogging();

    app.UseExceptionHandler();

    app.UseCors("DevCors");

    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "IMS API V1");
        c.RoutePrefix = string.Empty;
    });


    app.UseAuthentication();
    app.UseAuthorization();

    app.UseStaticFiles();

    app.MapControllers();

    app.MapHub<NotificationHub>($"/notifications");

    app.Run();
}
}

catch (Exception ex)
{
    Log.Fatal(ex, "IMS API terminated unexpectedly during startup");
}
finally
{
    Log.CloseAndFlush();
}