using Microsoft.EntityFrameworkCore;
using Ticketing.Domain.Enums;
using TransportPlatform.Ticketing.Domain.Entities;
using TransportPlatform.Ticketing.Domain.Interfaces;
using TransportPlatform.Ticketing.Infrastructure.Persistence;

namespace TransportPlatform.Ticketing.Infrastructure.Persistence.Repositories;

public class TicketRepository(TicketingDbContext db) : ITicketRepository
{
    public Task<Ticket?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        db.Tickets.FirstOrDefaultAsync(t => t.Id == id, ct);

    public async Task<IEnumerable<Ticket>> GetByUserIdAsync(
        Guid userId, int page, int pageSize, CancellationToken ct = default) =>
        await db.Tickets
            .Where(t => t.UserId == userId)
            .OrderByDescending(t => t.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

    public Task<int> GetActiveReservationCountAsync(Guid userId, CancellationToken ct = default) =>
        db.Tickets.CountAsync(
            t => t.UserId == userId &&
                 (t.Status == TicketStatus.PendingPayment || t.Status == TicketStatus.Confirmed),
            ct);

    public async Task AddAsync(Ticket ticket, CancellationToken ct = default)
    {
        await db.Tickets.AddAsync(ticket, ct);
        await db.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(Ticket ticket, CancellationToken ct = default)
    {
        db.Tickets.Update(ticket);
        await db.SaveChangesAsync(ct);
    }
}
