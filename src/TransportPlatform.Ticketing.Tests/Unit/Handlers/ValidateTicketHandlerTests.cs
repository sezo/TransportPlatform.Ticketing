using AutoFixture;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Shouldly;
using Ticketing.Application.Handlers;
using Ticketing.Domain.Enums;
using TransportPlatform.Contracts.Messaging;
using TransportPlatform.Ticketing.Application.Commands;
using TransportPlatform.Ticketing.Domain.Entities;
using TransportPlatform.Ticketing.Domain.Exceptions;
using TransportPlatform.Ticketing.Domain.Interfaces;

namespace TransportPlatform.Ticketing.Tests.Unit.Handlers;

[TestFixture]
public class ValidateTicketHandlerTests
{
    private Fixture _fixture = null!;
    private Mock<ITicketRepository> _ticketRepositoryMock = null!;
    private Mock<IEventPublisher> _eventPublisherMock = null!;
    private Mock<ILogger<ValidateTicketHandler>> _loggerMock = null!;
    private ValidateTicketHandler _handler = null!;

    [SetUp]
    public void Setup()
    {
        _fixture = new Fixture();
        _ticketRepositoryMock = new Mock<ITicketRepository>();
        _eventPublisherMock = new Mock<IEventPublisher>();
        _loggerMock = new Mock<ILogger<ValidateTicketHandler>>();

        _handler = new ValidateTicketHandler(
            _ticketRepositoryMock.Object,
            _eventPublisherMock.Object,
            _loggerMock.Object);
    }

    [Test]
    public async Task HandleAsync_WhenConfirmedTicketExists_ShouldMarkAsUsedAndPublishEvent()
    {
        // ── ARRANGE ────────────────────────────────────────────────────────
        var ticketId = _fixture.Create<Guid>();
        var userId = _fixture.Create<Guid>();
        var routeId = _fixture.Create<Guid>();
        var inspectorId = _fixture.Create<Guid>();
        var command = new ValidateTicketCommand(ticketId, inspectorId);

        // Create a confirmed ticket using Reserve() factory then Confirm()
        var ticket = Ticket.Reserve(
            userId: userId,
            routeId: routeId,
            seatNumber: 5,
            price: 25.50m);
        ticket.Confirm();

        // Mock: repository returns the confirmed ticket
        _ticketRepositoryMock
            .Setup(x => x.GetByIdAsync(ticketId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ticket);

        // Mock: repository update completes after ticket is marked used
        _ticketRepositoryMock
            .Setup(x => x.UpdateAsync(It.IsAny<Ticket>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Mock: event publisher publishes the TicketValidated event
        _eventPublisherMock
            .Setup(x => x.PublishAsync(It.IsAny<object>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // ── ACT ────────────────────────────────────────────────────────────
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // ── ASSERT ─────────────────────────────────────────────────────────
        result.ShouldNotBeNull();
        result.IsValid.ShouldBeTrue();
        result.Message.ShouldContain("Ticket valid");

        // Verify repository was called to fetch the ticket
        _ticketRepositoryMock.Verify(
            x => x.GetByIdAsync(ticketId, It.IsAny<CancellationToken>()),
            Times.Once);

        // Verify repository was called to update (persist MarkUsed state change)
        _ticketRepositoryMock.Verify(
            x => x.UpdateAsync(It.IsAny<Ticket>(), It.IsAny<CancellationToken>()),
            Times.Once);

        // Verify event was published (TicketValidated event)
        _eventPublisherMock.Verify(
            x => x.PublishAsync(It.IsAny<object>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task HandleAsync_WhenTicketNotFound_ShouldThrowTicketNotFoundException()
    {
        // ── ARRANGE ────────────────────────────────────────────────────────
        var ticketId = _fixture.Create<Guid>();
        var inspectorId = _fixture.Create<Guid>();
        var command = new ValidateTicketCommand(ticketId, inspectorId);

        // Mock: repository returns null (ticket not found)
        _ticketRepositoryMock
            .Setup(x => x.GetByIdAsync(ticketId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Ticket?)null);

        // ── ACT & ASSERT ───────────────────────────────────────────────────
        var exception = Should.Throw<TicketNotFoundException>(
            async () => await _handler.HandleAsync(command, CancellationToken.None));

        // Verify update was NOT called
        _ticketRepositoryMock.Verify(
            x => x.UpdateAsync(It.IsAny<Ticket>(), It.IsAny<CancellationToken>()),
            Times.Never);

        // Verify event was NOT published
        _eventPublisherMock.Verify(
            x => x.PublishAsync(It.IsAny<object>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
