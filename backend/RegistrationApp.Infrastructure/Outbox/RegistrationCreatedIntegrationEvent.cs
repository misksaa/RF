using System;

namespace RegistrationApp.Infrastructure.IntegrationEvents;

public record RegistrationCreatedIntegrationEvent
{
    public Guid Id { get; init; }
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string MobileNumber { get; init; } = string.Empty;
    public DateTime CreatedAtUtc { get; init; }
}
