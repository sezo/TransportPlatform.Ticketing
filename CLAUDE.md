# TransportPlatform.Ticketing — Claude context

## Bounded context
Ticket lifecycle management. Covers everything from ticket reservation through
payment confirmation to validation at the vehicle. This is the highest-traffic
service in the platform — runs 2 instances behind YARP LeastRequests load balancer.

## Team ownership
**Team 1** owns this repo. Team 1 also owns transport-platform-reporting.

## Business rules (domain layer enforces these)
- A ticket cannot be confirmed without a successful payment
- A ticket cannot be validated (used) unless it is in Confirmed status
- A cancelled ticket cannot be used or re-confirmed
- Seat reservation expires after 15 minutes if payment is not completed
- A passenger cannot hold more than 5 active reservations simultaneously

## Architecture
Clean Architecture (Onion):
- `TransportPlatform.Ticketing.Domain` — zero dependencies, pure C#
- `TransportPlatform.Ticketing.Application` — depends on Domain only
- `TransportPlatform.Ticketing.Infrastructure` — EF Core, RabbitMQ, Redis, Outbox
- `TransportPlatform.Ticketing.Api` — controllers, middleware, DI wiring

## Key patterns
### Outbox pattern
Every command that publishes an event saves the event to the OutboxMessages table
in the same EF Core transaction as the domain change. OutboxProcessor background
service polls and publishes to RabbitMQ. Guarantees at-least-once delivery.

### Saga — TicketPurchaseSaga
Coordinates ticket purchase across three services:
1. Ticketing reserves ticket → publishes TicketReserved
2. Accounting processes payment → publishes PaymentProcessed or PaymentFailed
3. Vehicle decrements capacity → publishes CapacityReserved or CapacityReservationFailed
Compensation: PaymentFailed or CapacityReservationFailed → TicketReservationCancelled

### CQRS
Commands (write side): ReserveTicket, ConfirmTicket, CancelTicket, ValidateTicket
Queries (read side): GetTicketById, GetUserTickets — hit Redis cache first, then DB

### Auth
Service never validates user JWT directly.
YARP gateway validates user token, injects X-User-Id and X-User-Roles headers,
replaces with M2M token. UserContextMiddleware extracts headers into UserContext.

## Integration events published
See TransportPlatform.Contracts/Events/Ticketing/ for full definitions.
- TicketReserved
- TicketConfirmed
- TicketCancelled
- TicketValidated (used by inspector)
- TicketReservationCancelled (saga compensation)

## Integration events consumed
- PaymentProcessed (from Accounting) → saga transition
- PaymentFailed (from Accounting) → saga compensation
- CapacityReserved (from Vehicle) → saga transition
- CapacityReservationFailed (from Vehicle) → saga compensation

## Database
PostgreSQL 16 — dedicated instance, no sharing.
Connection string: `Host=postgres-tickets;Database=tickets;Username=transport;Password=transport`
Migrations: `dotnet ef migrations add <Name> --project TransportPlatform.Ticketing.Infrastructure`

## Running locally
```bash
# Ensure infra is running first
cd ../../_transport-platform-meta/infra && docker compose up -d

# Start ticketing service
docker compose up -d

# Service available at http://localhost:5001
# Swagger UI at http://localhost:5001/swagger (Development only)
```

## What NOT to do
- Never query the vehicles or accounting database directly
- Never put payment logic in this service — that belongs in Accounting
- Never validate user JWT tokens in controllers — use UserContext instead
- Never publish events directly to RabbitMQ — always use the Outbox
- Never add cross-service joins — use the read model (Reporting service)
- Never share the Ticket entity outside this service — expose DTOs only

## Known technical debt
- Reservation expiry (15 min) currently handled by a background job polling the DB.
  Should be replaced with a delayed message in RabbitMQ.
- Redis cache invalidation on ticket status change is manual.
  Consider a cache-aside pattern with event-driven invalidation.
