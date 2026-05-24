using System;

namespace RegistrationApp.Domain.Common;

public abstract class DomainEvent
{
    public DateTime OccurredOnUtc { get; } = DateTime.UtcNow;
}
