namespace TransportPlatform.Ticketing.Application.Commands;

public record ValidateTicketCommand(Guid TicketId, Guid InspectorId);
