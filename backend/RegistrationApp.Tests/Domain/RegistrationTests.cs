using System;
using System.Collections.Generic;
using System.Linq;
using RegistrationApp.Domain.Entities;
using Xunit;

namespace RegistrationApp.Tests.Domain;

public class RegistrationTests
{
    [Fact]
    public void NormalizeName_ShouldTrimAndCollapseSpaces()
    {
        // Arrange
        var input = "  Mohamed    Ahmed   Ali  ";

        // Act
        var result = Registration.NormalizeName(input);

        // Assert
        Assert.Equal("Mohamed Ahmed Ali", result);
    }

    [Fact]
    public void Constructor_ShouldSucceed_WithValidInputs()
    {
        // Arrange
        var birthDate = DateTime.UtcNow.AddYears(-25);
        var addresses = new List<Address>
        {
            new Address(1, 1, "Street 1", "12A", "10", true)
        };

        // Act
        var registration = new Registration(
            "Ahmed",
            "Ali",
            "Hassan",
            birthDate,
            "+201006158123",
            "ahmed@example.com",
            addresses
        );

        // Assert
        Assert.NotNull(registration);
        Assert.Equal("Ahmed", registration.FirstName);
        Assert.Equal("Ali", registration.MiddleName);
        Assert.Equal("Hassan", registration.LastName);
        Assert.Equal("+201006158123", registration.MobileNumber);
        Assert.Equal("ahmed@example.com", registration.Email);
        Assert.Single(registration.Addresses);
        Assert.True(registration.Addresses.First().IsPrimary);
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenNoAddressesProvided()
    {
        // Arrange
        var birthDate = DateTime.UtcNow.AddYears(-25);
        var emptyAddresses = new List<Address>();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => new Registration(
            "Ahmed", "Ali", "Hassan", birthDate, "+201006158123", "ahmed@example.com", emptyAddresses
        ));
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenMoreThanFiveAddressesProvided()
    {
        // Arrange
        var birthDate = DateTime.UtcNow.AddYears(-25);
        var addresses = new List<Address>();
        for (int i = 0; i < 6; i++)
        {
            addresses.Add(new Address(1, 1, "Street", "1", "1", i == 0));
        }

        // Act & Assert
        Assert.Throws<ArgumentException>(() => new Registration(
            "Ahmed", "Ali", "Hassan", birthDate, "+201006158123", "ahmed@example.com", addresses
        ));
    }

    [Fact]
    public void CalculateAge_ShouldBePrecise()
    {
        // Arrange
        var birthDate = new DateTime(2000, 5, 24);
        var addresses = new List<Address> { new Address(1, 1, "Street", "1", "1", true) };
        var registration = new Registration("Ahmed", "", "Hassan", birthDate, "+201006158123", "ahmed@example.com", addresses);

        // Act
        var ageExactly20 = registration.CalculateAge(new DateTime(2020, 5, 24));
        var ageOneDayBefore20 = registration.CalculateAge(new DateTime(2020, 5, 23));

        // Assert
        Assert.Equal(20, ageExactly20);
        Assert.Equal(19, ageOneDayBefore20);
    }
}
