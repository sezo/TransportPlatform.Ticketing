using Microsoft.Extensions.Logging;
using Ticketing.Domain.Enums;
using TransportPlatform.Ticketing.Application.Interfaces;
using TransportPlatform.Contracts.Events.Ticketing;
using TransportPlatform.Contracts.Messaging;
using TransportPlatform.Ticketing.Domain.Exceptions;
using TransportPlatform.Ticketing.Domain.Interfaces;

namespace TransportPlatform.Ticketing.Application.Commands;

// ── Command ───────────────────────────────────────────────────────────────────
public record CancelTicketCommand(Guid TicketId, Guid UserId, string Reason);
public record CancelTicketResult(bool Success, string Message);

// ── Handler ───────────────────────────────────────────────────────────────────
public class CancelTicketHandler(
    ITicketRepository ticketRepository,
    IPaymentService paymentService,
    IEventPublisher eventPublisher,
    ILogger<CancelTicketHandler> logger)
{
    public async Task<CancelTicketResult> HandleAsync(
        CancelTicketCommand command,
        CancellationToken ct = default)
    {
        logger.LogInformation(
            "Cancelling ticket {TicketId} for user {UserId}, reason: {Reason}",
            command.TicketId, command.UserId, command.Reason);

        var ticket = await ticketRepository.GetByIdAsync(command.TicketId, ct)
            ?? throw new TicketNotFoundException(command.TicketId);

        // Only the owner can cancel
        if (ticket.UserId != command.UserId)
            throw new BusinessRuleException("You can only cancel your own tickets.");

        // Capture status before cancel — Confirmed means payment was taken, needs refund
        var wasConfirmed = ticket.Status == TicketStatus.Confirmed;

        ticket.Cancel(command.Reason);
        await ticketRepository.UpdateAsync(ticket, ct);

        if (wasConfirmed)
            await paymentService.RefundAsync(ticket.Id, ct);

        await eventPublisher.PublishAsync(new TicketCancelled(
            ticket.Id,
            ticket.UserId,
            command.Reason,
            DateTimeOffset.UtcNow), ct);

        logger.LogInformation("Ticket {TicketId} cancelled", command.TicketId);

        return new CancelTicketResult(true, "Ticket cancelled.");
    }
}
