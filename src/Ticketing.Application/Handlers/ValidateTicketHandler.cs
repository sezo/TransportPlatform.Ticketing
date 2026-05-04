using Microsoft.Extensions.Logging;
using TransportPlatform.Contracts.Events.Ticketing;
using TransportPlatform.Contracts.Messaging;
using TransportPlatform.Ticketing.Application.Commands;
using TransportPlatform.Ticketing.Domain.Exceptions;
using TransportPlatform.Ticketing.Domain.Interfaces;

namespace Ticketing.Application.Handlers;

public class ValidateTicketHandler(
    ITicketRepository ticketRepository,
    IEventPublisher eventPublisher,
    ILogger<ValidateTicketHandler> logger)
{
    public async Task<ValidateTicketResult> HandleAsync(
        ValidateTicketCommand command,
        CancellationToken ct = default)
    {
        logger.LogInformation(
            "Inspector {InspectorId} validating ticket {TicketId}",
            command.InspectorId, command.TicketId);

        var ticket = await ticketRepository.GetByIdAsync(command.TicketId, ct)
            ?? throw new TicketNotFoundException(command.TicketId);

        ticket.MarkUsed();
        await ticketRepository.UpdateAsync(ticket, ct);

        await eventPublisher.PublishAsync(new TicketValidated(
            ticket.Id,
            ticket.UserId,
            ticket.RouteId,
            command.InspectorId,
            DateTimeOffset.UtcNow), ct);

        logger.LogInformation(
            "Ticket {TicketId} validated by inspector {InspectorId}",
            command.TicketId, command.InspectorId);

        return new ValidateTicketResult(true, "Ticket valid.");
    }
}
