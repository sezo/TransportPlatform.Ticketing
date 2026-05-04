using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TransportPlatform.Infrastructure.Common.Messaging;
using TransportPlatform.Ticketing.Application.Interfaces;
using TransportPlatform.Ticketing.Domain.Interfaces;
using TransportPlatform.Ticketing.Infrastructure.Persistence;
using TransportPlatform.Ticketing.Infrastructure.Persistence.Repositories;
using TransportPlatform.Ticketing.Infrastructure.Persistence.Seeding;
using TransportPlatform.Ticketing.Infrastructure.Services;

namespace TransportPlatform.Ticketing.Infrastructure;

public static class InfrastructureExtensions
{
    public static IServiceCollection AddTicketingInfrastructure(
        this IServiceCollection services,
        IConfiguration config)
    {
        // ── Database ──────────────────────────────────────────────────────────
        services.AddDbContext<TicketingDbContext>(options =>
            options.UseNpgsql(
                config.GetConnectionString("TicketingDb"),
                npgsql => npgsql.MigrationsAssembly(
                    typeof(TicketingDbContext).Assembly.GetName().Name)));

        // ── Repositories ──────────────────────────────────────────────────────
        services.AddScoped<ITicketRepository, TicketRepository>();
        services.AddScoped<IRouteRepository, RouteRepository>();

        // ── Stub services (swap for real implementations post-demo) ───────────
        services.AddScoped<IPaymentService, PaymentServiceStub>();
        services.AddScoped<IFiscalService, FiscalServiceStub>();
        services.AddScoped<IVehicleService, VehicleServiceStub>();

        // ── Messaging (MassTransit + RabbitMQ, registers IEventPublisher) ─────
        services.AddTransportMessaging(config);

        // ── Database tracing (extends the OTel setup from AddTransportObservability) ──
        services.AddOpenTelemetry()
            .WithTracing(tracing => tracing.AddSource("Npgsql"));

        // ── Seeder ────────────────────────────────────────────────────────────
        services.AddScoped<TicketingDbSeeder>();

        return services;
    }

    /// <summary>
    /// Runs EF migrations and seeds demo data.
    /// Call once on startup in Development — safe to call repeatedly.
    /// </summary>
    public static async Task InitialiseDatabaseAsync(
        this IServiceProvider services,
        CancellationToken ct = default)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<TicketingDbContext>();
        await db.Database.MigrateAsync(ct);

        var seeder = scope.ServiceProvider.GetRequiredService<TicketingDbSeeder>();
        await seeder.SeedAsync(ct);
    }
}
