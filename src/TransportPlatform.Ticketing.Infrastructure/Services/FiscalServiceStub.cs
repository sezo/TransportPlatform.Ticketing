using Microsoft.Extensions.Logging;
using TransportPlatform.Ticketing.Application.Interfaces;

namespace TransportPlatform.Ticketing.Infrastructure.Services;

/// <summary>
/// MVP stub — always returns a fake fiscal number.
/// Replace with a real tax authority adapter (e.g. Croatian FINA/eRacun) post-demo.
/// </summary>
public class FiscalServiceStub(ILogger<FiscalServiceStub> logger) : IFiscalService
{
    public Task<FiscalResult> FiscalizeAsync(
        Guid ticketId, decimal amount, CancellationToken ct = default)
    {
        var fakeFiscalNumber = $"DEMO-{DateTime.UtcNow:yyyyMMdd}-{ticketId.ToString()[..8].ToUpper()}";

        logger.LogInformation(
            "[STUB] Fiscalization OK — TicketId: {TicketId}, FiscalNumber: {FiscalNumber}",
            ticketId, fakeFiscalNumber);

        return Task.FromResult(new FiscalResult(true, fakeFiscalNumber));
    }
}
