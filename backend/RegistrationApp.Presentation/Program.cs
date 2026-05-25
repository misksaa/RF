using System;
using System.IO;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using RegistrationApp.Application;
using RegistrationApp.Infrastructure;
using RegistrationApp.Persistence;
using RegistrationApp.Presentation.Middlewares;
using Serilog;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using System.Text.Json;

try
{
    var builder = WebApplication.CreateBuilder(args);

    // 1. Configure Serilog Structured Logging
    Log.Logger = new LoggerConfiguration()
        .ReadFrom.Configuration(builder.Configuration)
        .Enrich.FromLogContext()
        .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {CorrelationId} {Message:lj}{NewLine}{Exception}")
        .WriteTo.File(
            path: Path.Combine("logs", "app-.log"),
            rollingInterval: RollingInterval.Day,
            outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {CorrelationId} {Message:lj}{NewLine}{Exception}")
        .CreateLogger();

    builder.Host.UseSerilog();

    Log.Information("Starting the ASP.NET Core Web API...");

    // 2. Register Clean Architecture Layer Services
    builder.Services.AddApplicationServices();
    builder.Services.AddPersistenceServices(builder.Configuration);
    builder.Services.AddInfrastructureServices(builder.Configuration);

    builder.Services.AddControllers();

    // Register Health Checks for EF Core DbContext
    builder.Services.AddHealthChecks()
        .AddDbContextCheck<RegistrationDbContext>("sqlserver");
    
    // 3. Configure CORS Policy for React Frontend
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("CorsPolicy", policy =>
        {
            policy.AllowAnyOrigin()
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
    });

    builder.Services.AddEndpointsApiExplorer();
    
    // 4. Configure Swagger with XML comments & rich descriptions
    builder.Services.AddSwaggerGen(options =>
    {
        options.SwaggerDoc("v1", new OpenApiInfo
        {
            Version = "v1",
            Title = "User Registration API",
            Description = "Demo User Registration API",
            Contact = new OpenApiContact
            {
                Name = "Developer Team",
                Email = "dev@securedsmartsystems.com"
            }
        });

        // Enable XML Comments in Swagger
        var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
        var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFilename);
        if (File.Exists(xmlPath))
        {
            options.IncludeXmlComments(xmlPath);
        }
    });

    var app = builder.Build();

    // 5. Apply Middlewares in correct order
    app.UseMiddleware<ExceptionHandlingMiddleware>();
    app.UseMiddleware<CorrelationIdMiddleware>();
    app.UseMiddleware<RequestDurationLoggingMiddleware>();

    // Serve Swagger always for interview testing/review purposes
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Registration API V1");
        c.RoutePrefix = string.Empty; // Serve Swagger UI at the application's root URL (/)
    });

    app.UseCors("CorsPolicy");

    app.UseAuthorization();

    app.MapControllers();

    // Map Health Checks Endpoint with structured JSON response
    app.MapHealthChecks("/health", new HealthCheckOptions
    {
        ResponseWriter = async (context, report) =>
        {
            context.Response.ContentType = "application/json";

            var response = new
            {
                status = report.Status.ToString(),
                results = report.Entries.ToDictionary(
                    entry => entry.Key,
                    entry => new
                    {
                        status = entry.Value.Status.ToString(),
                        description = entry.Value.Description,
                        duration = entry.Value.Duration
                    })
            };

            var jsonOptions = new JsonSerializerOptions
            {
                WriteIndented = true
            };

            await context.Response.WriteAsync(JsonSerializer.Serialize(response, jsonOptions));
        }
    });

    // 6. Auto-migrate Database & Seed Lookups on startup!
    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        try
        {
            Log.Information("Applying EF Core Database migrations...");
            var dbContext = services.GetRequiredService<RegistrationDbContext>();
            // Since we'll create the migration next, this will run it automatically!
            await Microsoft.EntityFrameworkCore.RelationalDatabaseFacadeExtensions.MigrateAsync(dbContext.Database);
            Log.Information("Database successfully migrated.");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "An error occurred while migrating the database.");
        }
    }

    app.Run();
}
catch (Exception ex) when (ex is not OperationCanceledException && ex.GetType().Name != "HostAbortedException")
{
    Log.Fatal(ex, "ASP.NET Core Web API terminated unexpectedly.");
}
finally
{
    Log.CloseAndFlush();
}

public partial class Program { }
