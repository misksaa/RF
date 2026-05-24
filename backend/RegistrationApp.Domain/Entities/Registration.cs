using System;
using System.Collections.Generic;
using System.Linq;
using RegistrationApp.Domain.Common;
using RegistrationApp.Domain.Events;

namespace RegistrationApp.Domain.Entities;

public class Registration : Entity
{
    private readonly List<Address> _addresses = new();

    public string FirstName { get; private set; } = string.Empty;
    public string MiddleName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;
    public DateTime BirthDate { get; private set; }
    public string MobileNumber { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public DateTime CreatedAtUtc { get; private set; } = DateTime.UtcNow;
    public string CreatedBy { get; private set; } = "System";
    public DateTime? UpdatedAtUtc { get; private set; }
    public string? UpdatedBy { get; private set; }

    public IReadOnlyCollection<Address> Addresses => _addresses.AsReadOnly();

    // Parameterless constructor for EF Core
    private Registration() { }

    public Registration(
        string firstName,
        string middleName,
        string lastName,
        DateTime birthDate,
        string mobileNumber,
        string email,
        IEnumerable<Address> addresses)
    {
        FirstName = NormalizeName(firstName);
        MiddleName = NormalizeName(middleName);
        LastName = NormalizeName(lastName);
        BirthDate = birthDate.Date;
        MobileNumber = mobileNumber?.Trim() ?? string.Empty;
        Email = email?.Trim().ToLowerInvariant() ?? string.Empty;

        var addrList = addresses?.ToList() ?? new List<Address>();
        if (!addrList.Any())
        {
            throw new ArgumentException("At least one address is required.");
        }
        if (addrList.Count > 5)
        {
            throw new ArgumentException("Maximum of 5 addresses are allowed.");
        }

        // Validate primary address logic
        var primaryCount = addrList.Count(a => a.IsPrimary);
        if (primaryCount == 0)
        {
            // If only one address, or multiple addresses and none are primary,
            // set the first one as primary.
            addrList.First().IsPrimary = true;
        }
        else if (primaryCount > 1)
        {
            throw new ArgumentException("Only one address can be marked as primary.");
        }

        foreach (var addr in addrList)
        {
            _addresses.Add(addr);
        }

        AddDomainEvent(new RegistrationCreatedDomainEvent(this));
    }

    public static string NormalizeName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return string.Empty;

        return string.Join(" ", name.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)).Trim();
    }

    public int CalculateAge(DateTime relativeTo)
    {
        var age = relativeTo.Year - BirthDate.Year;
        if (BirthDate.Date > relativeTo.AddYears(-age)) 
            age--;
        return age;
    }
}
