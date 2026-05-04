using Asp.Versioning;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ticketing.Application.Handlers;
using TransportPlatform.Ticketing.Application.Commands;
using TransportPlatform.Ticketing.Application.Queries;
using TransportPlatform.Ticketing.Domain.Exceptions;

namespace TransportPlatform.Ticketing.Api.Controllers;

// ── Request DTOs ──────────────────────────────────────────────────────────────

public record BuyTicketRequest(Guid RouteId, int SeatNumber);

public record ValidateTicketRequest(Guid InspectorId);

public record CancelTicketRequest(string Reason);

// ── Controller ────────────────────────────────────────────────────────────────

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/tickets")]
[Authorize(Policy = "permission:ticket:write")]
public class TicketsController(
    ReserveTicketHandler reserveHandler,
    CancelTicketHandler cancelHandler,
    ValidateTicketHandler validateHandler,
    GetTicketByIdHandler getByIdHandler,
    GetUserTicketsHandler getUserTicketsHandler,
    GetRouteByIdHandler getRouteByIdHandler) : ControllerBase
{
    private Guid CurrentUserId =>
        Guid.TryParse(
            User.FindFirstValue(ClaimTypes.NameIdentifier),
            out var id) ? id : Guid.Empty;

    // ── POST /api/tickets ─────────────────────────────────────────────────────

    /// <summary>
    /// Buy a ticket. Orchestrates: capacity check (Vehicle stub) → payment (Accounting stub)
    /// → fiscalization (Tax stub) → confirms ticket. POC of cross-service flow.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Buy([FromBody] BuyTicketRequest request, CancellationToken ct)
    {
        // Look up route price server-side — client never dictates price
        var route = await getRouteByIdHandler.HandleAsync(
            new GetRouteByIdQuery(request.RouteId), ct);

        var command = new ReserveTicketCommand(
            UserId: CurrentUserId,
            RouteId: request.RouteId,
            SeatNumber: request.SeatNumber,
            Price: route.Price);

        var result = await reserveHandler.HandleAsync(command, ct);

        return CreatedAtAction(
            nameof(GetById),
            new { id = result.TicketId },
            result);
    }

    // ── GET /api/tickets/{id} ─────────────────────────────────────────────────

    /// <summary>Get a ticket by ID.</summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var ticket = await getByIdHandler.HandleAsync(new GetTicketByIdQuery(id), ct);
        return Ok(ticket);
    }

    // ── GET /api/tickets/my ───────────────────────────────────────────────────

    /// <summary>Get all tickets for the current user (paged).</summary>
    [HttpGet("my")]
    public async Task<IActionResult> GetMine(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var result = await getUserTicketsHandler.HandleAsync(
            new GetUserTicketsQuery(CurrentUserId, page, pageSize), ct);
        return Ok(result);
    }

    // ── POST /api/tickets/{id}/validate ──────────────────────────────────────

    /// <summary>
    /// Inspector validates (punches) a ticket at the vehicle.
    /// Called from the Internal gateway — inspector devices only.
    /// </summary>
    [HttpPost("{id:guid}/validate")]
    public async Task<IActionResult> Validate(
        Guid id,
        [FromBody] ValidateTicketRequest request,
        CancellationToken ct)
    {
        var result = await validateHandler.HandleAsync(
            new ValidateTicketCommand(id, request.InspectorId), ct);
        return Ok(result);
    }

    // ── DELETE /api/tickets/{id} ──────────────────────────────────────────────

    /// <summary>Cancel a ticket. Issues a refund stub and publishes TicketCancelled event.</summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Cancel(
        Guid id,
        [FromBody] CancelTicketRequest request,
        CancellationToken ct)
    {
        var result = await cancelHandler.HandleAsync(
            new CancelTicketCommand(id, CurrentUserId, request.Reason), ct);
        return Ok(result);
    }
}
