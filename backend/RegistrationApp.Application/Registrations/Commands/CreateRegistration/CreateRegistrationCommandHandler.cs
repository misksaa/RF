using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using RegistrationApp.Application.Common.Exceptions;
using RegistrationApp.Application.Common.Interfaces;
using RegistrationApp.Domain.Entities;

namespace RegistrationApp.Application.Registrations.Commands.CreateRegistration;

public class CreateRegistrationCommandHandler : IRequestHandler<CreateRegistrationCommand, Guid>
{
    private readonly IRegistrationDbContext _context;

    public CreateRegistrationCommandHandler(IRegistrationDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> Handle(CreateRegistrationCommand request, CancellationToken cancellationToken)
    {
        // 1. Double check duplicate Email and Mobile Number (for 409 Conflict handling)
        var emailNormalized = request.Email.Trim().ToLowerInvariant();
        var mobileNormalized = request.MobileNumber.Trim();

        var emailExists = await _context.Registrations
            .AnyAsync(r => r.Email == emailNormalized, cancellationToken);
        if (emailExists)
        {
            throw new ConflictException("Email is already registered.");
        }

        var mobileExists = await _context.Registrations
            .AnyAsync(r => r.MobileNumber == mobileNormalized, cancellationToken);
        if (mobileExists)
        {
            throw new ConflictException("Mobile number is already registered.");
        }

        // 2. Map addresses DTOs to Domain Address Entities
        var addresses = request.Addresses.Select(a => new Address(
            a.GovernorateId,
            a.CityId,
            a.Street,
            a.BuildingNumber,
            a.FlatNumber,
            a.IsPrimary
        )).ToList();

        // 3. Create Registration Domain Entity (which validates name formats, age, addresses counts, etc.)
        var registration = new Registration(
            request.FirstName,
            request.MiddleName,
            request.LastName,
            request.BirthDate,
            request.MobileNumber,
            request.Email,
            addresses
        );

        // 4. Persist
        await _context.Registrations.AddAsync(registration, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return registration.Id;
    }
}
