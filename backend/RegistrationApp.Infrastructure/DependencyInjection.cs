using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RegistrationApp.Application.Common.Interfaces;
using RegistrationApp.Infrastructure.Consumers;
using RegistrationApp.Infrastructure.Outbox;
using RegistrationApp.Infrastructure.Services;

namespace RegistrationApp.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<IDateTimeProvider, DateTimeProvider>();

        services.AddMassTransit(x =>
        {
            x.AddConsumer<RegistrationCreatedIntegrationEventConsumer>();

            var useInMemoryVal = configuration["RabbitMQ:UseInMemory"];
            var useInMemory = string.IsNullOrEmpty(useInMemoryVal) || !bool.TryParse(useInMemoryVal, out var inMem) || inMem;

            if (useInMemory)
            {
                x.UsingInMemory((context, cfg) =>
                {
                    cfg.ConfigureEndpoints(context);
                });
            }
            else
            {
                x.UsingRabbitMq((context, cfg) =>
                {
                    var host = configuration["RabbitMQ:Host"] ?? "localhost";
                    var port = ushort.TryParse(configuration["RabbitMQ:Port"], out var p) ? p : (ushort)5672;
                    var username = configuration["RabbitMQ:Username"] ?? "guest";
                    var password = configuration["RabbitMQ:Password"] ?? "guest";

                    cfg.Host(host, port, "/", h =>
                    {
                        h.Username(username);
                        h.Password(password);
                    });

                    cfg.ConfigureEndpoints(context);
                });
            }
        });

        services.AddHostedService<OutboxBackgroundProcessor>();

        return services;
    }
}
