# TransportPlatform.Ticketing

Ticketing bounded context. Handles ticket reservation, payment confirmation,
and validation. Highest-traffic service — runs 2 instances in production.

## Prerequisites
- .NET 9 SDK
- Docker Desktop
- Infra stack running (`_transport-platform-meta/infra`)

## Quick start

### 1. Ensure infra is running
```bash
cd ../_transport-platform-meta/infra
docker compose up -d
```

### 2. Start the service
```bash
docker compose up -d
```

### 3. Verify
- API: http://localhost:5001/swagger
- Health: http://localhost:5001/health

## Running without Docker (for debugging)
```bash
cd src/TransportPlatform.Ticketing.Api
dotnet run
```
Ensure `appsettings.Development.json` has correct connection strings pointing to localhost ports.

## Running tests

### Unit tests
```bash
dotnet test tests/TransportPlatform.Ticketing.Tests.Unit
```

### Integration tests
Requires Docker — Testcontainers spins up PostgreSQL and RabbitMQ automatically.
```bash
dotnet test tests/TransportPlatform.Ticketing.Tests.Integration
```

## Database migrations
```bash
# Add migration
dotnet ef migrations add <MigrationName> \
  --project src/TransportPlatform.Ticketing.Infrastructure \
  --startup-project src/TransportPlatform.Ticketing.Api

# Apply migrations
dotnet ef database update \
  --project src/TransportPlatform.Ticketing.Infrastructure \
  --startup-project src/TransportPlatform.Ticketing.Api
```
Migrations run automatically on startup in Development environment.

## Key endpoints
| Method | Path | Description | Auth |
|---|---|---|---|
| POST | /api/tickets | Reserve a ticket | Passenger, BusinessClient |
| GET | /api/tickets/{id} | Get ticket by id | Owner, Admin |
| GET | /api/tickets/my | Get my tickets (paged) | Passenger, BusinessClient |
| POST | /api/tickets/{id}/validate | Validate ticket at vehicle | Inspector |

All endpoints require a valid JWT. In local dev, get a token from Keycloak:
```
POST http://localhost:9090/realms/transport/protocol/openid-connect/token
Content-Type: application/x-www-form-urlencoded

grant_type=password&client_id=b2c-web&username=test@test.com&password=test
```

## Project structure
```
src/
  TransportPlatform.Ticketing.Api/          Controllers, middleware, Program.cs
  TransportPlatform.Ticketing.Application/  Commands, queries, sagas (MediatR)
  TransportPlatform.Ticketing.Domain/       Entities, value objects, domain events
  TransportPlatform.Ticketing.Infrastructure/ EF Core, outbox, RabbitMQ consumers
tests/
  TransportPlatform.Ticketing.Tests.Unit/        Domain + application unit tests
  TransportPlatform.Ticketing.Tests.Integration/ Full stack integration tests
```

## Architecture decisions
See `_transport-platform-meta/docs/adr/` for full rationale.
Key decisions affecting this service:
- ADR 007 — Clean Architecture
- ADR 008 — MassTransit outbox and sagas
- ADR 005 — Separate PostgreSQL per service

## CI/CD
- Push to `feature/*` → runs build + unit tests
- PR to `main` → runs build + unit tests + integration tests
- Merge to `main` → builds Docker image, pushes to Harbor, deploys to dev
See `.github/workflows/` for pipeline definitions.

## Team
**Team 1** — also owns `transport-platform-reporting`.
Branch lifetime: max 5 days. All PRs require Team 1 lead approval.
