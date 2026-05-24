using AutoMapper;
using RegistrationApp.Application.Common.Mappings;
using Xunit;

namespace RegistrationApp.Tests.Application;

public class MappingTests
{
    [Fact]
    public void Configuration_ShouldBeValid()
    {
        var configuration = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<MappingProfile>();
        });

        configuration.AssertConfigurationIsValid();
    }
}
