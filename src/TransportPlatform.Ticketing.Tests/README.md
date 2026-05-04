# TransportPlatform.Ticketing.Tests

Unit tests for the Ticketing microservice using **nUnit**, **Moq**, **AutoFixture**, and **Shouldly**.

## Test Structure

Each test follows the **Arrange-Act-Assert** pattern:

```csharp
[Test]
public async Task TestName_GivenContext_ExpectedOutcome()
{
    // ── ARRANGE ────────────────────────────────────────────────────────
    // Set up test data, mocks, and handler

    // ── ACT ────────────────────────────────────────────────────────────
    // Execute the code under test

    // ── ASSERT ─────────────────────────────────────────────────────────
    // Verify outcomes using Shouldly assertions
}
```

## Tests Included

### ReserveTicketHandlerTests

1. **HandleAsync_WhenValidCommandProvided_ShouldReserveTicketAndReturnId**
   - Happy path: Valid ticket reservation succeeds
   - Verifies ticket is saved and events are published

2. **HandleAsync_WhenUserExceedsMaxReservations_ShouldThrowBusinessRuleException**
   - Validation: User at max capacity (5 reservations) cannot reserve more
   - Verifies short-circuit (downstream services not called)

### ValidateTicketHandlerTests

1. **HandleAsync_WhenConfirmedTicketExists_ShouldMarkAsUsedAndPublishEvent**
   - Happy path: Inspector validates a confirmed ticket
   - Verifies ticket is marked used and validation event is published

2. **HandleAsync_WhenTicketNotFound_ShouldThrowTicketNotFoundException**
   - Error handling: Non-existent ticket throws appropriate exception
   - Verifies downstream services aren't called unnecessarily

## Run Tests

```bash
cd TransportPlatform.Ticketing/src

# All tests
dotnet test

# Specific test class
dotnet test --filter ClassName=ReserveTicketHandlerTests

# Verbose output
dotnet test --verbosity detailed
```

## Tools

| Tool | Purpose |
|---|---|
| **nUnit** | Test framework with `[TestFixture]` and `[Test]` attributes |
| **Moq** | Mock/stub external dependencies (`ITicketRepository`, etc.) |
| **AutoFixture** | Generate realistic test data (Guid, decimals, etc.) |
| **Shouldly** | Fluent assertions: `result.IsValid.ShouldBeTrue()` |

## Notes

- Mocks verify behavior (e.g., `_repository.Verify(...)`)
- No database or HTTP calls — all mocked
- Tests are isolated and can run in any order
