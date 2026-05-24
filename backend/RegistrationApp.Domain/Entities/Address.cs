using System;
using RegistrationApp.Domain.Common;

namespace RegistrationApp.Domain.Entities;

public class Address : Entity
{
    public Guid RegistrationId { get; set; }
    public int GovernorateId { get; set; }
    public int CityId { get; set; }
    public string Street { get; set; } = string.Empty;
    public string BuildingNumber { get; set; } = string.Empty;
    public string FlatNumber { get; set; } = string.Empty;
    public bool IsPrimary { get; set; }
    
    // Parameterless constructor for EF Core
    public Address() { }

    public Address(int governorateId, int cityId, string street, string buildingNumber, string flatNumber, bool isPrimary)
    {
        GovernorateId = governorateId;
        CityId = cityId;
        Street = street?.Trim() ?? string.Empty;
        BuildingNumber = buildingNumber?.Trim() ?? string.Empty;
        FlatNumber = flatNumber?.Trim() ?? string.Empty;
        IsPrimary = isPrimary;
    }
}
