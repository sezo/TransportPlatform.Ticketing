using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TransportPlatform.Ticketing.Domain.Entities;
using TransportPlatform.Ticketing.Infrastructure.Persistence;

namespace TransportPlatform.Ticketing.Infrastructure.Persistence.Seeding;

/// <summary>
/// Seeds demo routes on first run. Safe to call on every startup — checks for existing data.
/// </summary>
public class TicketingDbSeeder(TicketingDbContext db, ILogger<TicketingDbSeeder> logger)
{
    public async Task SeedAsync(CancellationToken ct = default)
    {
        if (await db.Routes.AnyAsync(ct))
        {
            logger.LogDebug("Routes already seeded — skipping");
            return;
        }

        logger.LogInformation("Seeding demo routes...");

        var routes = new[]
        {
            Route.Create("ZG → SP Express",  "Zagreb",    "Split",     25.00m, 50),
            Route.Create("ZG → RJ Fast",     "Zagreb",    "Rijeka",    15.00m, 40),
            Route.Create("ZG → OS Shuttle",  "Zagreb",    "Osijek",    18.00m, 45),
            Route.Create("SP → DU Coast",    "Split",     "Dubrovnik", 20.00m, 35),
            Route.Create("ZG → MB Regional", "Zagreb",    "Maribor",   22.00m, 40),
        };

        await db.Routes.AddRangeAsync(routes, ct);
        await db.SaveChangesAsync(ct);

        logger.LogInformation("Seeded {Count} demo routes", routes.Length);
    }
}
