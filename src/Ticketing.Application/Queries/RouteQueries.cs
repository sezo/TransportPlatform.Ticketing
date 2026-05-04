using Microsoft.Extensions.Logging;
using TransportPlatform.Ticketing.Domain.Entities;
using TransportPlatform.Ticketing.Domain.Exceptions;
using TransportPlatform.Ticketing.Domain.Interfaces;

namespace TransportPlatform.Ticketing.Application.Queries;

// ── DTOs ──────────────────────────────────────────────────────────────────────
public record RouteDto(
    Guid Id,
    string Name,
    string Origin,
    string Destination,
    decimal Price,
    int TotalSeats);

// ── Get all active routes ─────────────────────────────────────────────────────
public record GetAvailableRoutesQuery();

public class GetAvailableRoutesHandler(
    IRouteRepository routeRepository,
    ILogger<GetAvailableRoutesHandler> logger)
{
    public async Task<IEnumerable<RouteDto>> HandleAsync(
        GetAvailableRoutesQuery query,
        CancellationToken ct = default)
    {
        logger.LogDebug("Getting all active routes");
        var routes = await routeRepository.GetAllActiveAsync(ct);
        return routes.Select(r => r.ToDto());
    }
}

// ── Get route by ID ───────────────────────────────────────────────────────────
public record GetRouteByIdQuery(Guid RouteId);

public class GetRouteByIdHandler(
    IRouteRepository routeRepository,
    ILogger<GetRouteByIdHandler> logger)
{
    public async Task<RouteDto> HandleAsync(
        GetRouteByIdQuery query,
        CancellationToken ct = default)
    {
        logger.LogDebug("Getting route {RouteId}", query.RouteId);

        var route = await routeRepository.GetByIdAsync(query.RouteId, ct)
            ?? throw new RouteNotFoundException(query.RouteId);

        return route.ToDto();
    }
}

// ── Extensions ────────────────────────────────────────────────────────────────
public static class RouteExtensions
{
    public static RouteDto ToDto(this Route route) =>
        new(route.Id,
            route.Name,
            route.Origin,
            route.Destination,
            route.Price,
            route.TotalSeats);
}
