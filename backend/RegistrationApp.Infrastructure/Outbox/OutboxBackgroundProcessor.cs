using System;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RegistrationApp.Application.Common.Interfaces;
using RegistrationApp.Application.Common.Models;
using RegistrationApp.Infrastructure.IntegrationEvents;

namespace RegistrationApp.Infrastructure.Outbox;

public class OutboxBackgroundProcessor : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<OutboxBackgroundProcessor> _logger;

    public OutboxBackgroundProcessor(IServiceProvider serviceProvider, ILogger<OutboxBackgroundProcessor> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Outbox Background Processor is starting...");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessOutboxMessagesAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while processing Outbox messages.");
            }

            await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
        }
    }

    private async Task ProcessOutboxMessagesAsync(CancellationToken stoppingToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<IRegistrationDbContext>();
        var publishEndpoint = scope.ServiceProvider.GetRequiredService<IPublishEndpoint>();

        var messages = await dbContext.OutboxMessages
            .Where(m => m.ProcessedOnUtc == null)
            .OrderBy(m => m.OccurredOnUtc)
            .Take(20)
            .ToListAsync(stoppingToken);

        if (!messages.Any())
            return;

        _logger.LogInformation("Found {Count} unprocessed Outbox messages.", messages.Count);

        foreach (var message in messages)
        {
            try
            {
                if (message.Type.Contains("RegistrationCreatedDomainEvent"))
                {
                    using var doc = JsonDocument.Parse(message.Content);
                    var root = doc.RootElement;
                    var registrationNode = root.GetProperty("Registration");

                    var integrationEvent = new RegistrationCreatedIntegrationEvent
                    {
                        Id = registrationNode.GetProperty("Id").GetGuid(),
                        FirstName = registrationNode.GetProperty("FirstName").GetString() ?? string.Empty,
                        LastName = registrationNode.GetProperty("LastName").GetString() ?? string.Empty,
                        Email = registrationNode.GetProperty("Email").GetString() ?? string.Empty,
                        MobileNumber = registrationNode.GetProperty("MobileNumber").GetString() ?? string.Empty,
                        CreatedAtUtc = registrationNode.GetProperty("CreatedAtUtc").GetDateTime()
                    };

                    await publishEndpoint.Publish(integrationEvent, stoppingToken);
                    _logger.LogInformation("Successfully published RegistrationCreatedIntegrationEvent for ID: {RegistrationId}", integrationEvent.Id);
                }

                message.ProcessedOnUtc = DateTime.UtcNow;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to publish Outbox message {MessageId}", message.Id);
                message.Error = ex.Message;
            }
        }

        await dbContext.SaveChangesAsync(stoppingToken);
    }
}
