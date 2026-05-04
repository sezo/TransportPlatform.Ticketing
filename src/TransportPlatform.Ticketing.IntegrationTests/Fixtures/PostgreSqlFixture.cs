using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using Testcontainers.PostgreSql;
using TransportPlatform.Ticketing.Infrastructure.Persistence;

namespace TransportPlatform.Ticketing.IntegrationTests.Fixtures;

/// <summary>
/// Database fixture using Testcontainers to spin up a real PostgreSQL container.
/// </summary>
public class PostgreSqlFixture
{
    private PostgreSqlContainer? _container;
    private TicketingDbContext? _dbContext;

    public TicketingDbContext DbContext
    {
        get => _dbContext ?? throw new InvalidOperationException("DbContext not initialized. Initialize fixture first.");
    }

    public async Task InitializeAsync()
    {
        // ── Start PostgreSQL container ──────────────────────────────────────
        _container = new PostgreSqlBuilder()
            .WithImage("postgres:16-alpine")
            .Build();

        await _container.StartAsync();

        // ── Create DbContext with connection to container ──────────────────
        var options = new DbContextOptionsBuilder<TicketingDbContext>()
            .UseNpgsql(_container.GetConnectionString())
            .Options;

        _dbContext = new TicketingDbContext(options);

        // ── Migrate database ──────────────────────────────────────────────
        await _dbContext.Database.MigrateAsync();
    }

    public async Task DisposeAsync()
    {
        if (_dbContext != null)
            await _dbContext.DisposeAsync();

        if (_container != null)
            await _container.StopAsync();
    }
}
