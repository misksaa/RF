using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using RegistrationApp.Application.Common.Exceptions;
using RegistrationApp.Application.Common.Interfaces;

namespace RegistrationApp.Application.Registrations.Queries.GetRegistration;

public record GetRegistrationQuery(Guid Id) : IRequest<RegistrationDto>;

public class GetRegistrationQueryHandler : IRequestHandler<GetRegistrationQuery, RegistrationDto>
{
    private readonly IRegistrationDbContext _context;
    private readonly IMapper _mapper;

    public GetRegistrationQueryHandler(IRegistrationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<RegistrationDto> Handle(GetRegistrationQuery request, CancellationToken cancellationToken)
    {
        var registration = await _context.Registrations
            .Include(r => r.Addresses)
            .FirstOrDefaultAsync(r => r.Id == request.Id, cancellationToken);

        if (registration == null)
        {
            throw new NotFoundException(nameof(Domain.Entities.Registration), request.Id);
        }

        var govIds = registration.Addresses.Select(a => a.GovernorateId).Distinct().ToList();
        var cityIds = registration.Addresses.Select(a => a.CityId).Distinct().ToList();

        var governorates = await _context.Governorates
            .Where(g => govIds.Contains(g.Id))
            .ToDictionaryAsync(g => g.Id, cancellationToken);

        var cities = await _context.Cities
            .Where(c => cityIds.Contains(c.Id))
            .ToDictionaryAsync(c => c.Id, cancellationToken);

        var addressDtos = registration.Addresses.Select(a =>
        {
            var dto = _mapper.Map<AddressDto>(a);
            
            if (governorates.TryGetValue(a.GovernorateId, out var gov))
            {
                dto.GovernorateNameAr = gov.NameAr;
                dto.GovernorateNameEn = gov.NameEn;
            }

            if (cities.TryGetValue(a.CityId, out var city))
            {
                dto.CityNameAr = city.NameAr;
                dto.CityNameEn = city.NameEn;
            }

            return dto;
        }).ToList();

        var regDto = _mapper.Map<RegistrationDto>(registration);
        regDto.Addresses = addressDtos;

        return regDto;
    }
}
