namespace TransportPlatform.Ticketing.Domain.Exceptions;

public class BusinessRuleException(string message) : Exception(message);

public class TicketNotFoundException(Guid ticketId)
    : Exception($"Ticket {ticketId} not found.");

public class RouteNotFoundException(Guid routeId)
    : Exception($"Route {routeId} not found.");
