using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using RegistrationApp.Application.Common.Interfaces;

namespace RegistrationApp.Application.Lookups.Queries.GetGovernorates;

public record GetGovernoratesQuery : IRequest<List<LookupDto>>;

public class GetGovernoratesQueryHandler : IRequestHandler<GetGovernoratesQuery, List<LookupDto>>
{
    private readonly IRegistrationDbContext _context;
    private readonly IMapper _mapper;

    public GetGovernoratesQueryHandler(IRegistrationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<List<LookupDto>> Handle(GetGovernoratesQuery request, CancellationToken cancellationToken)
    {
        var governorates = await _context.Governorates
            .Where(g => g.IsActive)
            .OrderBy(g => g.NameEn)
            .ToListAsync(cancellationToken);

        return _mapper.Map<List<LookupDto>>(governorates);
    }
}
