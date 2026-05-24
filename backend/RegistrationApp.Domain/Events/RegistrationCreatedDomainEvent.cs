using RegistrationApp.Domain.Common;
using RegistrationApp.Domain.Entities;

namespace RegistrationApp.Domain.Events;

public class RegistrationCreatedDomainEvent : DomainEvent
{
    public Registration Registration { get; }

    public RegistrationCreatedDomainEvent(Registration registration)
    {
        Registration = registration;
    }
}
