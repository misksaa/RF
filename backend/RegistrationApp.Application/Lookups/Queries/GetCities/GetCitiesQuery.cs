using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using RegistrationApp.Application.Common.Interfaces;

namespace RegistrationApp.Application.Lookups.Queries.GetCities;

public record GetCitiesQuery(int GovernorateId) : IRequest<List<LookupDto>>;

public class GetCitiesQueryHandler : IRequestHandler<GetCitiesQuery, List<LookupDto>>
{
    private readonly IRegistrationDbContext _context;
    private readonly IMapper _mapper;

    public GetCitiesQueryHandler(IRegistrationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<List<LookupDto>> Handle(GetCitiesQuery request, CancellationToken cancellationToken)
    {
        var cities = await _context.Cities
            .Where(c => c.GovernorateId == request.GovernorateId && c.IsActive)
            .OrderBy(c => c.NameEn)
            .ToListAsync(cancellationToken);

        return _mapper.Map<List<LookupDto>>(cities);
    }
}
