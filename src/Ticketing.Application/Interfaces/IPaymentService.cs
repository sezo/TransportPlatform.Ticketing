namespace TransportPlatform.Ticketing.Application.Interfaces;

public interface IPaymentService
{
    Task<PaymentResult> ProcessPaymentAsync(Guid ticketId, decimal amount, Guid userId, CancellationToken ct = default);
    Task RefundAsync(Guid ticketId, CancellationToken ct = default);
}
