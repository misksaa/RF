using System.Threading.Tasks;
using MassTransit;
using Microsoft.Extensions.Logging;
using RegistrationApp.Infrastructure.IntegrationEvents;

namespace RegistrationApp.Infrastructure.Consumers;

public class RegistrationCreatedIntegrationEventConsumer : IConsumer<RegistrationCreatedIntegrationEvent>
{
    private readonly ILogger<RegistrationCreatedIntegrationEventConsumer> _logger;

    public RegistrationCreatedIntegrationEventConsumer(ILogger<RegistrationCreatedIntegrationEventConsumer> logger)
    {
        _logger = logger;
    }

    public Task Consume(ConsumeContext<RegistrationCreatedIntegrationEvent> context)
    {
        var message = context.Message;

        _logger.LogInformation("Processing integration event for Registration ID: {RegistrationId}", message.Id);
        _logger.LogInformation("Sending welcome email to: {Email}", message.Email);
        _logger.LogInformation("Sending welcome SMS to: {MobileNumber}", message.MobileNumber);

        _logger.LogInformation("Successfully completed integration event processing for Registration ID: {RegistrationId}", message.Id);

        return Task.CompletedTask;
    }
}
