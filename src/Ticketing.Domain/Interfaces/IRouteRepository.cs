using TransportPlatform.Ticketing.Domain.Entities;

namespace TransportPlatform.Ticketing.Domain.Interfaces;

public interface IRouteRepository
{
    Task<IEnumerable<Route>> GetAllActiveAsync(CancellationToken ct = default);
    Task<Route?> GetByIdAsync(Guid id, CancellationToken ct = default);
}
