using Microsoft.EntityFrameworkCore;
using TransportPlatform.Ticketing.Domain.Entities;
using TransportPlatform.Ticketing.Domain.Interfaces;
using TransportPlatform.Ticketing.Infrastructure.Persistence;

namespace TransportPlatform.Ticketing.Infrastructure.Persistence.Repositories;

public class RouteRepository(TicketingDbContext db) : IRouteRepository
{
    public async Task<IEnumerable<Route>> GetAllActiveAsync(CancellationToken ct = default) =>
        await db.Routes
            .Where(r => r.IsActive)
            .OrderBy(r => r.Name)
            .ToListAsync(ct);

    public Task<Route?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        db.Routes.FirstOrDefaultAsync(r => r.Id == id, ct);
}
