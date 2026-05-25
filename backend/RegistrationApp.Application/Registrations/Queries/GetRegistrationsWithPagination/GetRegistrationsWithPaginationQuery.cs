using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using RegistrationApp.Application.Common.Interfaces;
using RegistrationApp.Application.Common.Models;
using RegistrationApp.Application.Registrations.Queries.GetRegistration;

namespace RegistrationApp.Application.Registrations.Queries.GetRegistrationsWithPagination;

public record GetRegistrationsWithPaginationQuery(
    string? SearchTerm,
    int PageNumber = 1,
    int PageSize = 10) : IRequest<PaginatedList<RegistrationDto>>;

public class GetRegistrationsWithPaginationQueryHandler 
    : IRequestHandler<GetRegistrationsWithPaginationQuery, PaginatedList<RegistrationDto>>
{
    private readonly IRegistrationDbContext _context;
    private readonly IMapper _mapper;

    public GetRegistrationsWithPaginationQueryHandler(IRegistrationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<PaginatedList<RegistrationDto>> Handle(
        GetRegistrationsWithPaginationQuery request, 
        CancellationToken cancellationToken)
    {
        var query = _context.Registrations
            .Include(r => r.Addresses)
            .AsQueryable();

        // Apply case-insensitive searching across multiple fields if SearchTerm is provided
        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var cleanSearch = request.SearchTerm.Trim().ToLowerInvariant();
            query = query.Where(r => 
                r.FirstName.ToLower().Contains(cleanSearch) ||
                r.LastName.ToLower().Contains(cleanSearch) ||
                r.Email.ToLower().Contains(cleanSearch) ||
                r.MobileNumber.Contains(cleanSearch));
        }

        // Apply sorting (newest registrations first)
        query = query.OrderByDescending(r => r.CreatedAtUtc);

        // Fetch paginated database items
        var paginatedList = await PaginatedList<Domain.Entities.Registration>.CreateAsync(
            query,
            request.PageNumber,
            request.PageSize,
            cancellationToken);

        if (!paginatedList.Items.Any())
        {
            return new PaginatedList<RegistrationDto>(
                new List<RegistrationDto>(),
                paginatedList.TotalCount,
                paginatedList.PageNumber,
                request.PageSize);
        }

        // Batch-fetch Governorate and City translations across all addresses in the page
        var allAddresses = paginatedList.Items.SelectMany(r => r.Addresses).ToList();
        var govIds = allAddresses.Select(a => a.GovernorateId).Distinct().ToList();
        var cityIds = allAddresses.Select(a => a.CityId).Distinct().ToList();

        var governorates = await _context.Governorates
            .Where(g => govIds.Contains(g.Id))
            .ToDictionaryAsync(g => g.Id, cancellationToken);

        var cities = await _context.Cities
            .Where(c => cityIds.Contains(c.Id))
            .ToDictionaryAsync(c => c.Id, cancellationToken);

        // Map domain objects to dynamic DTOs and populate lookups in memory
        var mappedItems = paginatedList.Items.Select(reg =>
        {
            var addressDtos = reg.Addresses.Select(a =>
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

            var regDto = _mapper.Map<RegistrationDto>(reg);
            regDto.Addresses = addressDtos;
            return regDto;
        }).ToList();

        return new PaginatedList<RegistrationDto>(
            mappedItems,
            paginatedList.TotalCount,
            paginatedList.PageNumber,
            request.PageSize);
    }
}
