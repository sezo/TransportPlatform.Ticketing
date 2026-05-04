using Microsoft.Extensions.Logging;
using TransportPlatform.Ticketing.Application.Interfaces;

namespace TransportPlatform.Ticketing.Infrastructure.Services;

/// <summary>
/// MVP stub — always approves payment.
/// Replace with a real payment gateway (Stripe, etc.) post-demo.
/// </summary>
public class PaymentServiceStub(ILogger<PaymentServiceStub> logger) : IPaymentService
{
    public Task<PaymentResult> ProcessPaymentAsync(
        Guid ticketId, decimal amount, Guid userId, CancellationToken ct = default)
    {
        logger.LogInformation(
            "[STUB] Payment approved — TicketId: {TicketId}, Amount: {Amount}, UserId: {UserId}",
            ticketId, amount, userId);

        return Task.FromResult(new PaymentResult(true, null));
    }

    public Task RefundAsync(Guid ticketId, CancellationToken ct = default)
    {
        logger.LogInformation("[STUB] Refund issued — TicketId: {TicketId}", ticketId);
        return Task.CompletedTask;
    }
}
