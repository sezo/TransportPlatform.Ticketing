namespace TransportPlatform.Ticketing.Application.Commands;

public record ReserveTicketCommand(
    Guid UserId,
    Guid RouteId,
    int SeatNumber,
    decimal Price);

public record ReserveTicketResult(Guid TicketId);
