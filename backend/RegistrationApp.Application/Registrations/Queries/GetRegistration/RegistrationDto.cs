using System;
using System.Collections.Generic;

namespace RegistrationApp.Application.Registrations.Queries.GetRegistration;

public class RegistrationDto
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string MiddleName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public DateTime BirthDate { get; set; }
    public string MobileNumber { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public IReadOnlyCollection<AddressDto> Addresses { get; set; } = new List<AddressDto>();
}
