namespace TransportPlatform.Ticketing.Application.Interfaces;

public interface IFiscalService
{
    Task<FiscalResult> FiscalizeAsync(Guid ticketId, decimal amount, CancellationToken ct = default);
}
