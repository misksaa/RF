using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RegistrationApp.Application.Common.Interfaces;

namespace RegistrationApp.Persistence;

public static class DependencyInjection
{
    public static IServiceCollection AddPersistenceServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<RegistrationDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly(typeof(RegistrationDbContext).Assembly.FullName)));

        services.AddScoped<IRegistrationDbContext>(provider => provider.GetRequiredService<RegistrationDbContext>());

        return services;
    }
}
