using AutoFixture;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Shouldly;
using Ticketing.Application.Handlers;
using TransportPlatform.Contracts.Messaging;
using TransportPlatform.Ticketing.Application.Commands;
using TransportPlatform.Ticketing.Application.Interfaces;
using TransportPlatform.Ticketing.Domain.Entities;
using TransportPlatform.Ticketing.Domain.Exceptions;
using TransportPlatform.Ticketing.Domain.Interfaces;

namespace TransportPlatform.Ticketing.Tests.Unit.Handlers;

[TestFixture]
public class ReserveTicketHandlerTests
{
    private Fixture _fixture = null!;
    private Mock<ITicketRepository> _ticketRepositoryMock = null!;
    private Mock<IRouteRepository> _routeRepositoryMock = null!;
    private Mock<IVehicleService> _vehicleServiceMock = null!;
    private Mock<IPaymentService> _paymentServiceMock = null!;
    private Mock<IFiscalService> _fiscalServiceMock = null!;
    private Mock<IEventPublisher> _eventPublisherMock = null!;
    private Mock<ILogger<ReserveTicketHandler>> _loggerMock = null!;
    private ReserveTicketHandler _handler = null!;

    [SetUp]
    public void Setup()
    {
        _fixture = new Fixture();
        _ticketRepositoryMock = new Mock<ITicketRepository>();
        _routeRepositoryMock = new Mock<IRouteRepository>();
        _vehicleServiceMock = new Mock<IVehicleService>();
        _paymentServiceMock = new Mock<IPaymentService>();
        _fiscalServiceMock = new Mock<IFiscalService>();
        _eventPublisherMock = new Mock<IEventPublisher>();
        _loggerMock = new Mock<ILogger<ReserveTicketHandler>>();

        _handler = new ReserveTicketHandler(
            _ticketRepositoryMock.Object,
            _routeRepositoryMock.Object,
            _vehicleServiceMock.Object,
            _paymentServiceMock.Object,
            _fiscalServiceMock.Object,
            _eventPublisherMock.Object,
            _loggerMock.Object);
    }

    [Test]
    public async Task HandleAsync_WhenValidCommandProvided_ShouldReserveTicketAndReturnId()
    {
        // ── ARRANGE ────────────────────────────────────────────────────────
        var userId = _fixture.Create<Guid>();
        var routeId = _fixture.Create<Guid>();
        var seatNumber = 5;
        var price = 25.50m;

        var command = new ReserveTicketCommand(
            UserId: userId,
            RouteId: routeId,
            SeatNumber: seatNumber,
            Price: price);

        // Mock: user has no active reservations
        _ticketRepositoryMock
            .Setup(x => x.GetActiveReservationCountAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);

        // Mock: vehicle service approves capacity reservation
        _vehicleServiceMock
            .Setup(x => x.ReserveCapacityAsync(
                It.IsAny<Guid>(), routeId, seatNumber, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CapacityResult(Available: true, FailureReason: null));

        // Mock: payment processes successfully
        _paymentServiceMock
            .Setup(x => x.ProcessPaymentAsync(
                It.IsAny<Guid>(), price, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PaymentResult(Success: true, FailureReason: null));

        // Mock: fiscal service accepts the transaction
        _fiscalServiceMock
            .Setup(x => x.FiscalizeAsync(
                It.IsAny<Guid>(), price, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FiscalResult(Success: true, FiscalNumber: "FISCAL-123"));

        // Mock: ticket is saved to repository
        _ticketRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<Ticket>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Mock: event is published
        _eventPublisherMock
            .Setup(x => x.PublishAsync(It.IsAny<object>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // ── ACT ────────────────────────────────────────────────────────────
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // ── ASSERT ─────────────────────────────────────────────────────────
        result.ShouldNotBeNull();
        result.TicketId.ShouldNotBe(Guid.Empty);

        // Verify repository was called to save the ticket
        _ticketRepositoryMock.Verify(
            x => x.AddAsync(It.IsAny<Ticket>(), It.IsAny<CancellationToken>()),
            Times.Once);

        // Verify event was published (handler publishes 2 events: TicketReserved + TicketConfirmed)
        _eventPublisherMock.Verify(
            x => x.PublishAsync(It.IsAny<object>(), It.IsAny<CancellationToken>()),
            Times.Exactly(2));
    }

    [Test]
    public async Task HandleAsync_WhenUserExceedsMaxReservations_ShouldThrowBusinessRuleException()
    {
        // ── ARRANGE ────────────────────────────────────────────────────────
        var userId = _fixture.Create<Guid>();
        var command = new ReserveTicketCommand(
            UserId: userId,
            RouteId: _fixture.Create<Guid>(),
            SeatNumber: 10,
            Price: 30.00m);

        // Mock: user already has 5 active reservations (at max capacity)
        _ticketRepositoryMock
            .Setup(x => x.GetActiveReservationCountAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(5);

        // ── ACT & ASSERT ───────────────────────────────────────────────────
        var exception = Should.Throw<BusinessRuleException>(
            async () => await _handler.HandleAsync(command, CancellationToken.None));

        exception.Message.ShouldContain("cannot hold more than 5 active reservations");

        // Verify vehicle service was NOT called (short-circuit validation)
        _vehicleServiceMock.Verify(
            x => x.ReserveCapacityAsync(
                It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
