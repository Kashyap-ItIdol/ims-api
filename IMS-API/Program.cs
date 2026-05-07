using IMS_API;
using IMS_Application.Extentions;
using IMS_Infrastructure.Extentions;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, services, configuration) =>
    configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext());

builder.Services.AddControllers();

builder.Services.AddSwaggerDocumentation();

builder.Services.AddProjectServices();

builder.Services.AddValidationResponseFormat();

builder.Services.AddApplication();

builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddValidation();

builder.Services.AddMapping();

builder.Services.AddExceptionHandling();

builder.Services.AddJwtAuthentication(builder.Configuration);

var app = builder.Build();

app.UseSerilogRequestLogging();

app.UseExceptionHandler();

app.UseSwagger();

app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "IMS API V1");
    options.RoutePrefix = string.Empty;
});

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();