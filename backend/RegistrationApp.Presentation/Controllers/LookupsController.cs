using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RegistrationApp.Application.Lookups.Queries;
using RegistrationApp.Application.Lookups.Queries.GetCities;
using RegistrationApp.Application.Lookups.Queries.GetGovernorates;

namespace RegistrationApp.Presentation.Controllers;

[ApiController]
[Route("api/lookups")]
public class LookupsController : ControllerBase
{
    private readonly IMediator _mediator;

    public LookupsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Retrieves active Governorates.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of Governorates.</returns>
    /// <response code="200">Returns lookups list.</response>
    [HttpGet("governorates")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<LookupDto>))]
    public async Task<IActionResult> GetGovernorates(CancellationToken cancellationToken)
    {
        var query = new GetGovernoratesQuery();
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Retrieves cities filtered by Governorate.
    /// </summary>
    /// <param name="governorateId">Governorate Lookup ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of Cities for that Governorate.</returns>
    /// <response code="200">Returns lookups list.</response>
    [HttpGet("cities")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<LookupDto>))]
    public async Task<IActionResult> GetCities([FromQuery] int governorateId, CancellationToken cancellationToken)
    {
        var query = new GetCitiesQuery(governorateId);
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }
}
