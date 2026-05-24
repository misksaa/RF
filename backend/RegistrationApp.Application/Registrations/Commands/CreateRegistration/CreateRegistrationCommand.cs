using System;
using System.Collections.Generic;
using MediatR;

namespace RegistrationApp.Application.Registrations.Commands.CreateRegistration;

public record CreateRegistrationCommand : IRequest<Guid>
{
    public string FirstName { get; init; } = string.Empty;
    public string MiddleName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public DateTime BirthDate { get; init; }
    public string MobileNumber { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public List<CreateAddressCommandDto> Addresses { get; init; } = new();
}

public record CreateAddressCommandDto
{
    public int GovernorateId { get; init; }
    public int CityId { get; init; }
    public string Street { get; init; } = string.Empty;
    public string BuildingNumber { get; init; } = string.Empty;
    public string FlatNumber { get; init; } = string.Empty;
    public bool IsPrimary { get; init; }
}
