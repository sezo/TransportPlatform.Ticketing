# TransportPlatform.Ticketing.IntegrationTests

Integration tests using **Testcontainers** for PostgreSQL and **EF Core** with a real database.

## Test Structure

Tests use `IAsyncLifetime` to:
1. Spin up a PostgreSQL container once per test fixture
2. Migrate the database schema
3. Test actual repository operations
4. Tear down the container

## Tests Included

### TicketRepositoryIntegrationTests

1. **AddAsync_WhenTicketProvided_ShouldPersistToDatabase**
   - Creates a ticket in memory
   - Saves to PostgreSQL container via repository
   - Retrieves it back and verifies all fields persisted correctly

2. **GetActiveReservationCountAsync_WhenUserHasConfirmedTickets_ShouldCountCorrectly**
   - Creates 3 tickets for a user
   - Confirms 2 of them
   - Verifies the count matches expectations (PendingPayment + Confirmed statuses)

## Run Tests

```bash
cd TransportPlatform.Ticketing/src

# All integration tests
dotnet test TransportPlatform.Ticketing.IntegrationTests

# Specific test class
dotnet test --filter ClassName=TicketRepositoryIntegrationTests

# Verbose output
dotnet test --verbosity detailed
```

## Requirements

- Docker must be running (for Testcontainers to spawn PostgreSQL)
- First run will pull the `postgres:16-alpine` image (~200MB)

## Tools

| Tool | Purpose |
|---|---|
| **Testcontainers** | Spin up real PostgreSQL in Docker for each test |
| **NUnit** + **Shouldly** | Test framework and assertions |
| **EF Core** | Database migrations and entity mapping |

## Notes

- Each test class runs a fresh container (IAsyncLifetime)
- Database is auto-migrated on startup
- Tests are isolated and can run in any order
- Real database operations — validates schema, constraints, EF Core mappings
