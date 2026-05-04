using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TransportPlatform.Ticketing.Application.Queries;

namespace TransportPlatform.Ticketing.Api.Controllers;

/// <summary>
/// Read-only route catalogue.
/// In the full system this data lives in the Vehicle service — Ticketing queries it
/// via an HTTP client or read-model projection. For MVP demo it lives here.
/// </summary>
[ApiController]
[Route("api/routes")]
[Authorize(Policy = "permission:ticket:write")]
public class RoutesController(
    GetAvailableRoutesHandler getRoutesHandler,
    GetRouteByIdHandler getRouteByIdHandler) : ControllerBase
{
    /// <summary>Browse all active routes available for ticket purchase.</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var routes = await getRoutesHandler.HandleAsync(new GetAvailableRoutesQuery(), ct);
        return Ok(routes);
    }

    /// <summary>Get a single route by ID.</summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var route = await getRouteByIdHandler.HandleAsync(new GetRouteByIdQuery(id), ct);
        return Ok(route);
    }
}
