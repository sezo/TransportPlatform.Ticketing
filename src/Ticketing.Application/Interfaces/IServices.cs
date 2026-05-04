namespace TransportPlatform.Ticketing.Application.Interfaces;

// ── Payment (Accounting service) ──────────────────────────────────────────────
public record PaymentResult(bool Success, string? FailureReason);

// ── Vehicle capacity (Vehicle service) ────────────────────────────────────────
public record CapacityResult(bool Available, string? FailureReason);

// ── Fiscal (Tax authority adapter) ────────────────────────────────────────────
public record FiscalResult(bool Success, string? FiscalNumber);
