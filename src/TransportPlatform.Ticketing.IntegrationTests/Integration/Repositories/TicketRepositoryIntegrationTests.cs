using NUnit.Framework;
using Shouldly;
using Ticketing.Domain.Enums;
using TransportPlatform.Ticketing.Domain.Entities;
using TransportPlatform.Ticketing.Infrastructure.Persistence.Repositories;
using TransportPlatform.Ticketing.IntegrationTests.Fixtures;

namespace TransportPlatform.Ticketing.IntegrationTests.Integration.Repositories;

[TestFixture]
public class TicketRepositoryIntegrationTests
{
    private PostgreSqlFixture _fixture = null!;
    private TicketRepository _repository = null!;

    [OneTimeSetUp]
    public async Task OneTimeSetupAsync()
    {
        _fixture = new PostgreSqlFixture();
        await _fixture.InitializeAsync();
        _repository = new TicketRepository(_fixture.DbContext);
    }

    [OneTimeTearDown]
    public async Task OneTimeTearDownAsync()
    {
        await _fixture.DisposeAsync();
    }

    [Test]
    public async Task AddAsync_WhenTicketProvided_ShouldPersistToDatabase()
    {
        // ── ARRANGE ────────────────────────────────────────────────────────
        var userId = Guid.NewGuid();
        var routeId = Guid.NewGuid();
        var price = 45.99m;

        var ticket = Ticket.Reserve(userId, routeId, seatNumber: 12, price);

        // ── ACT ────────────────────────────────────────────────────────────
        // Save ticket to real database via repository
        await _repository.AddAsync(ticket, CancellationToken.None);

        // Retrieve it back
        var retrieved = await _repository.GetByIdAsync(ticket.Id, CancellationToken.None);

        // ── ASSERT ─────────────────────────────────────────────────────────
        retrieved.ShouldNotBeNull();
        retrieved.Id.ShouldBe(ticket.Id);
        retrieved.UserId.ShouldBe(userId);
        retrieved.RouteId.ShouldBe(routeId);
        retrieved.SeatNumber.ShouldBe(12);
        retrieved.Price.ShouldBe(price);
        retrieved.Status.ShouldBe(TicketStatus.PendingPayment);
    }

    [Test]
    public async Task GetActiveReservationCountAsync_WhenUserHasConfirmedTickets_ShouldCountCorrectly()
    {
        // ── ARRANGE ────────────────────────────────────────────────────────
        var userId = Guid.NewGuid();
        var routeId = Guid.NewGuid();

        // Create 3 tickets
        var ticket1 = Ticket.Reserve(userId, routeId, 1, 25.00m);
        var ticket2 = Ticket.Reserve(userId, routeId, 2, 25.00m);
        var ticket3 = Ticket.Reserve(userId, routeId, 3, 25.00m);

        // Save them
        await _repository.AddAsync(ticket1, CancellationToken.None);
        await _repository.AddAsync(ticket2, CancellationToken.None);
        await _repository.AddAsync(ticket3, CancellationToken.None);

        // Confirm two of them
        ticket1.Confirm();
        ticket2.Confirm();
        await _repository.UpdateAsync(ticket1, CancellationToken.None);
        await _repository.UpdateAsync(ticket2, CancellationToken.None);

        // ── ACT ────────────────────────────────────────────────────────────
        var activeCount = await _repository.GetActiveReservationCountAsync(userId, CancellationToken.None);

        // ── ASSERT ─────────────────────────────────────────────────────────
        // 1 PendingPayment + 2 Confirmed = 3 active
        activeCount.ShouldBe(3);
    }
}
