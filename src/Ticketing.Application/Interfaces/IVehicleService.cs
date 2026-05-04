namespace TransportPlatform.Ticketing.Application.Interfaces;

public interface IVehicleService
{
    Task<CapacityResult> ReserveCapacityAsync(Guid ticketId, Guid routeId, int seatNumber, CancellationToken ct = default);
    Task ReleaseCapacityAsync(Guid ticketId, CancellationToken ct = default);
}
