using Microsoft.Extensions.Logging;
using TransportPlatform.Contracts.Events.Ticketing;
using TransportPlatform.Contracts.Messaging;
using TransportPlatform.Ticketing.Application.Commands;
using TransportPlatform.Ticketing.Application.Interfaces;
using TransportPlatform.Ticketing.Domain.Entities;
using TransportPlatform.Ticketing.Domain.Exceptions;
using TransportPlatform.Ticketing.Domain.Interfaces;

namespace Ticketing.Application.Handlers;

// ── Handler ───────────────────────────────────────────────────────────────────
public class ReserveTicketHandler(
    ITicketRepository ticketRepository,
    IRouteRepository routeRepository,
    IVehicleService vehicleService,
    IPaymentService paymentService,
    IFiscalService fiscalService,
    IEventPublisher eventPublisher,
    ILogger<ReserveTicketHandler> logger)
{
    public async Task<ReserveTicketResult> HandleAsync(
        ReserveTicketCommand command,
        CancellationToken ct = default)
    {
        logger.LogInformation(
            "Reserving ticket for user {UserId} on route {RouteId} seat {Seat}",
            command.UserId, command.RouteId, command.SeatNumber);

        var activeCount = await ticketRepository
            .GetActiveReservationCountAsync(command.UserId, ct);

        if (activeCount >= 5)
            throw new BusinessRuleException(
                "Passenger cannot hold more than 5 active reservations.");

        // ── 2. Reserve seat on vehicle (Vehicle service stub) ──────────────
        var capacity = await vehicleService.ReserveCapacityAsync(
            Guid.NewGuid(), command.RouteId, command.SeatNumber, ct);

        if (!capacity.Available)
            throw new BusinessRuleException(
                $"Seat not available: {capacity.FailureReason}");

        // ── 3. Create domain entity ────────────────────────────────────────
        var ticket = Ticket.Reserve(
            command.UserId,
            command.RouteId,
            command.SeatNumber,
            command.Price);

        await ticketRepository.AddAsync(ticket, ct);

        await eventPublisher.PublishAsync(new TicketReserved(
            ticket.Id,
            ticket.UserId,
            ticket.RouteId,
            ticket.SeatNumber,
            ticket.Price,
            DateTimeOffset.UtcNow), ct);

        // ── 4. Process payment (Accounting service stub) ───────────────────
        var payment = await paymentService.ProcessPaymentAsync(
            ticket.Id, ticket.Price, command.UserId, ct);

        if (!payment.Success)
        {
            await vehicleService.ReleaseCapacityAsync(ticket.Id, ct);
            ticket.Cancel($"Payment failed: {payment.FailureReason}");
            await ticketRepository.UpdateAsync(ticket, ct);
            throw new BusinessRuleException(
                $"Payment failed: {payment.FailureReason}");
        }

        // ── 5. Fiscalize (Tax authority adapter stub) ──────────────────────
        var fiscal = await fiscalService.FiscalizeAsync(
            ticket.Id, ticket.Price, ct);

        if (!fiscal.Success)
            logger.LogWarning(
                "Fiscalization failed for ticket {TicketId} — will retry async",
                ticket.Id);

        // ── 6. Confirm ticket ──────────────────────────────────────────────
        ticket.Confirm();
        await ticketRepository.UpdateAsync(ticket, ct);

        // ── 7. Publish event (RabbitMQ stub) ──────────────────────────────
        var route = await routeRepository.GetByIdAsync(ticket.RouteId, ct);

        await eventPublisher.PublishAsync(new TicketConfirmed(
            ticket.Id,
            ticket.UserId,
            ticket.RouteId,
            route?.Name ?? string.Empty,
            route?.Origin ?? string.Empty,
            route?.Destination ?? string.Empty,
            ticket.SeatNumber,
            ticket.Price,
            DateTimeOffset.UtcNow), ct);

        logger.LogInformation(
            "Ticket {TicketId} reserved and confirmed for user {UserId}",
            ticket.Id, command.UserId);

        return new ReserveTicketResult(ticket.Id);
    }
}

