using System;

namespace RegistrationApp.Application.Registrations.Queries.GetRegistration;

public class AddressDto
{
    public Guid Id { get; set; }
    public int GovernorateId { get; set; }
    public string GovernorateNameAr { get; set; } = string.Empty;
    public string GovernorateNameEn { get; set; } = string.Empty;
    public int CityId { get; set; }
    public string CityNameAr { get; set; } = string.Empty;
    public string CityNameEn { get; set; } = string.Empty;
    public string Street { get; set; } = string.Empty;
    public string BuildingNumber { get; set; } = string.Empty;
    public string FlatNumber { get; set; } = string.Empty;
    public bool IsPrimary { get; set; }
}
