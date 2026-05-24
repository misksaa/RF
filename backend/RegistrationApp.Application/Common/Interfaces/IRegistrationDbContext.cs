using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RegistrationApp.Domain.Entities;

namespace RegistrationApp.Application.Common.Interfaces;

public interface IRegistrationDbContext
{
    DbSet<Registration> Registrations { get; }
    DbSet<Address> Addresses { get; }
    DbSet<Governorate> Governorates { get; }
    DbSet<City> Cities { get; }
    DbSet<RegistrationApp.Application.Common.Models.OutboxMessage> OutboxMessages { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
