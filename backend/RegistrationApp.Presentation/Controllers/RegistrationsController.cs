using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RegistrationApp.Application.Registrations.Commands.CreateRegistration;
using RegistrationApp.Application.Registrations.Queries.GetRegistration;
using RegistrationApp.Application.Registrations.Queries.GetRegistrationsWithPagination;
using RegistrationApp.Application.Common.Models;

namespace RegistrationApp.Presentation.Controllers;

[ApiController]
[Route("api/registrations")]
public class RegistrationsController : ControllerBase
{
    private readonly IMediator _mediator;

    public RegistrationsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Creates a new user registration with profile details and addresses.
    /// </summary>
    /// <param name="command">Registration payload details.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The created registration identifier.</returns>
    /// <response code="201">Returns the created registration ID and the Location header.</response>
    /// <response code="400">If the input model is invalid (validation errors).</response>
    /// <response code="409">If email or mobile number already exists.</response>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create([FromBody] CreateRegistrationCommand command, CancellationToken cancellationToken)
    {
        var id = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id }, new { id });
    }

    /// <summary>
    /// Retrieves user registration details by identifier.
    /// </summary>
    /// <param name="id">Registration GUID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Registration details.</returns>
    /// <response code="200">Returns registration details.</response>
    /// <response code="404">If registration is not found.</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(RegistrationDto))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var query = new GetRegistrationQuery(id);
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Retrieves a paginated list of registrations, optionally filtered by a search term.
    /// </summary>
    /// <param name="searchTerm">Search filter for first name, last name, email, or mobile number.</param>
    /// <param name="pageNumber">Page index (default is 1).</param>
    /// <param name="pageSize">Page size limit (default is 10).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A paginated list container of registrations.</returns>
    /// <response code="200">Returns the paginated results container.</response>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PaginatedList<RegistrationDto>))]
    public async Task<IActionResult> GetWithPagination(
        [FromQuery] string? searchTerm,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var query = new GetRegistrationsWithPaginationQuery(searchTerm, pageNumber, pageSize);
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }
}
