using Ticketing.Domain.Enums;
using TransportPlatform.Ticketing.Domain.Exceptions;

namespace TransportPlatform.Ticketing.Domain.Entities;

public class Ticket
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public Guid RouteId { get; private set; }
    public int SeatNumber { get; private set; }
    public decimal Price { get; private set; }
    public TicketStatus Status { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset ExpiresAt { get; private set; }
    public string? CancellationReason { get; private set; }

    private Ticket() { }

    public static Ticket Reserve(Guid userId, Guid routeId, int seatNumber, decimal price)
    {
        return new Ticket
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            RouteId = routeId,
            SeatNumber = seatNumber,
            Price = price,
            Status = TicketStatus.PendingPayment,
            CreatedAt = DateTimeOffset.UtcNow,
            ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(15)
        };
    }

    public void Confirm()
    {
        if (Status != TicketStatus.PendingPayment)
            throw new BusinessRuleException($"Cannot confirm ticket in status {Status}.");

        if (DateTimeOffset.UtcNow > ExpiresAt)
            throw new BusinessRuleException("Reservation expired. Please reserve again.");

        Status = TicketStatus.Confirmed;
    }

    public void Cancel(string reason)
    {
        if (Status == TicketStatus.Used)
            throw new BusinessRuleException("Cannot cancel a used ticket.");

        Status = TicketStatus.Cancelled;
        CancellationReason = reason;
    }

    public void MarkUsed()
    {
        if (Status != TicketStatus.Confirmed)
            throw new BusinessRuleException("Only confirmed tickets can be validated.");

        Status = TicketStatus.Used;
    }
}
