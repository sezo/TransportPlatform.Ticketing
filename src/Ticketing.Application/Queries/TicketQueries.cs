using Microsoft.Extensions.Logging;
using TransportPlatform.Ticketing.Domain.Entities;
using TransportPlatform.Ticketing.Domain.Exceptions;
using TransportPlatform.Ticketing.Domain.Interfaces;

namespace TransportPlatform.Ticketing.Application.Queries;

// ── DTOs ──────────────────────────────────────────────────────────────────────
public record TicketDto(
    Guid Id,
    Guid UserId,
    Guid RouteId,
    int SeatNumber,
    decimal Price,
    string Status,
    DateTimeOffset CreatedAt,
    DateTimeOffset ExpiresAt);

public record PagedResult<T>(
    IEnumerable<T> Items,
    int Page,
    int PageSize,
    int TotalCount);

// ── Get by ID ─────────────────────────────────────────────────────────────────
public record GetTicketByIdQuery(Guid TicketId);

public class GetTicketByIdHandler(
    ITicketRepository ticketRepository,
    ILogger<GetTicketByIdHandler> logger)
{
    public async Task<TicketDto> HandleAsync(
        GetTicketByIdQuery query,
        CancellationToken ct = default)
    {
        logger.LogDebug("Getting ticket {TicketId}", query.TicketId);

        var ticket = await ticketRepository.GetByIdAsync(query.TicketId, ct)
            ?? throw new TicketNotFoundException(query.TicketId);

        return ticket.ToDto();
    }
}

// ── Get user tickets (paged) ──────────────────────────────────────────────────
public record GetUserTicketsQuery(Guid UserId, int Page = 1, int PageSize = 20);

public class GetUserTicketsHandler(
    ITicketRepository ticketRepository,
    ILogger<GetUserTicketsHandler> logger)
{
    public async Task<PagedResult<TicketDto>> HandleAsync(
        GetUserTicketsQuery query,
        CancellationToken ct = default)
    {
        logger.LogDebug(
            "Getting tickets for user {UserId} page {Page}",
            query.UserId, query.Page);

        var tickets = await ticketRepository
            .GetByUserIdAsync(query.UserId, query.Page, query.PageSize, ct);

        var dtos = tickets.Select(t => t.ToDto()).ToList();

        // TODO: get total count from repository for proper paging
        return new PagedResult<TicketDto>(dtos, query.Page, query.PageSize, dtos.Count);
    }
}

// ── Extension ─────────────────────────────────────────────────────────────────
public static class TicketExtensions
{
    public static TicketDto ToDto(this Ticket ticket) =>
        new(ticket.Id,
            ticket.UserId,
            ticket.RouteId,
            ticket.SeatNumber,
            ticket.Price,
            ticket.Status.ToString(),
            ticket.CreatedAt,
            ticket.ExpiresAt);
}
