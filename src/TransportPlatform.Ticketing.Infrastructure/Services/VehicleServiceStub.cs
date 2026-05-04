using Microsoft.Extensions.Logging;
using TransportPlatform.Ticketing.Application.Interfaces;

namespace TransportPlatform.Ticketing.Infrastructure.Services;

/// <summary>
/// MVP stub — always reports capacity available.
/// Replace with an HTTP call to the Vehicle service post-demo.
/// In real architecture this would be an async saga step via RabbitMQ,
/// not a synchronous call — see TicketPurchaseSaga design.
/// </summary>
public class VehicleServiceStub(ILogger<VehicleServiceStub> logger) : IVehicleService
{
    public Task<CapacityResult> ReserveCapacityAsync(
        Guid ticketId, Guid routeId, int seatNumber, CancellationToken ct = default)
    {
        logger.LogInformation(
            "[STUB] Capacity reserved — RouteId: {RouteId}, Seat: {Seat}",
            routeId, seatNumber);

        return Task.FromResult(new CapacityResult(true, null));
    }

    public Task ReleaseCapacityAsync(Guid ticketId, CancellationToken ct = default)
    {
        logger.LogInformation("[STUB] Capacity released — TicketId: {TicketId}", ticketId);
        return Task.CompletedTask;
    }
}
