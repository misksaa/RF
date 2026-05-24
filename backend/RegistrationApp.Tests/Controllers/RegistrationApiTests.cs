using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using RegistrationApp.Application.Lookups.Queries;
using RegistrationApp.Application.Registrations.Commands.CreateRegistration;
using RegistrationApp.Application.Registrations.Queries.GetRegistration;
using Xunit;

namespace RegistrationApp.Tests.Controllers;

public class RegistrationApiTests : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory<Program> _factory;

    public RegistrationApiTests(CustomWebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetGovernorates_ShouldReturnActiveGovernorates()
    {
        var response = await _client.GetAsync("/api/lookups/governorates");

        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<List<LookupDto>>();
        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.Contains(result, g => g.NameEn == "Cairo");
    }

    [Fact]
    public async Task GetCities_ShouldReturnFilteredCities()
    {
        var response = await _client.GetAsync("/api/lookups/cities?governorateId=1");

        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<List<LookupDto>>();
        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.Contains(result, c => c.NameEn == "Heliopolis");
    }

    [Fact]
    public async Task CreateRegistration_ShouldSucceed_WithValidData()
    {
        var email = $"mohamed.{Guid.NewGuid()}@example.com";
        var command = new CreateRegistrationCommand
        {
            FirstName = "Mohamed",
            MiddleName = "Ahmed",
            LastName = "Ali",
            BirthDate = DateTime.UtcNow.AddYears(-25),
            MobileNumber = $"+2010{new Random().Next(10000000, 99999999)}",
            Email = email,
            Addresses = new List<CreateAddressCommandDto>
            {
                new CreateAddressCommandDto
                {
                    GovernorateId = 1,
                    CityId = 1,
                    Street = "Tahrir St",
                    BuildingNumber = "12",
                    FlatNumber = "3A",
                    IsPrimary = true
                }
            }
        };

        var response = await _client.PostAsJsonAsync("/api/registrations", command);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(response.Headers.Location);

        var responseBody = await response.Content.ReadFromJsonAsync<Dictionary<string, Guid>>();
        Assert.NotNull(responseBody);
        Assert.True(responseBody.ContainsKey("id"));

        var getResponse = await _client.GetAsync(response.Headers.Location);
        getResponse.EnsureSuccessStatusCode();

        var registrationDto = await getResponse.Content.ReadFromJsonAsync<RegistrationDto>();
        Assert.NotNull(registrationDto);
        Assert.Equal("Mohamed", registrationDto.FirstName);
        Assert.Single(registrationDto.Addresses);
        Assert.Equal("Cairo", registrationDto.Addresses.First().GovernorateNameEn);
    }

    [Fact]
    public async Task CreateRegistration_ShouldFail_WhenNameHasNumbers()
    {
        var command = new CreateRegistrationCommand
        {
            FirstName = "Mohamed123",
            LastName = "Ali",
            BirthDate = DateTime.UtcNow.AddYears(-25),
            MobileNumber = "+201006158123",
            Email = "invalidname@example.com",
            Addresses = new List<CreateAddressCommandDto>
            {
                new CreateAddressCommandDto
                {
                    GovernorateId = 1,
                    CityId = 1,
                    Street = "Tahrir St",
                    BuildingNumber = "12",
                    FlatNumber = "3",
                    IsPrimary = true
                }
            }
        };

        var response = await _client.PostAsJsonAsync("/api/registrations", command);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateRegistration_ShouldFail_WhenCityDoesNotBelongToGovernorate()
    {
        var command = new CreateRegistrationCommand
        {
            FirstName = "Mohamed",
            LastName = "Ali",
            BirthDate = DateTime.UtcNow.AddYears(-25),
            MobileNumber = "+201006158123",
            Email = "mismatchedlookups@example.com",
            Addresses = new List<CreateAddressCommandDto>
            {
                new CreateAddressCommandDto
                {
                    GovernorateId = 1,
                    CityId = 5,
                    Street = "Tahrir St",
                    BuildingNumber = "12",
                    FlatNumber = "3",
                    IsPrimary = true
                }
            }
        };

        var response = await _client.PostAsJsonAsync("/api/registrations", command);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
