# SmartMetroService.Tests

Test project for SmartMetroService backend. Tests are organized into unit tests and integration tests.

## Test Organization

### Unit Tests (`UnitTests/`)

Fast, isolated tests for pure logic with no external dependencies:
- Domain logic (fare calculation, ticket state transitions, QR payload encryption/decryption)
- Application managers and services (when tested in isolation)
- Utility functions and helpers

**Requirements:**
- No database access
- No HTTP calls (mock external services via DI)
- Run in < 100ms per test
- Can run in parallel
- No test dependencies or shared state

**Example patterns:**
```csharp
[Fact]
public void CalculateFare_WithValidStations_ReturnCorrectAmount()
{
    // Arrange
    var stationService = new StationService();
    
    // Act
    var fare = stationService.GetFare(startStation, endStation);
    
    // Assert
    Assert.Equal(expectedFare, fare);
}
```

### Integration Tests (`IntegrationTests/`)

Tests that verify behavior across layers using `WebApplicationFactory` against a real SQL Server test database:
- Complete auth flows (register, login, logout, token refresh)
- API endpoints with database state
- Repository and query correctness
- Ownership and permission checks
- Payment and wallet operations
- Scanning and journey recording

**Requirements:**
- Use `WebApplicationFactory<Program>` fixture
- Run against `SMARTMETRO_TEST_DB` (set via `scripts/ci.sh`)
- Each test creates its own users and data
- No dependencies between tests or test order
- Verify both happy path and error cases
- Include ownership/permission tests (another user's resource → 404)
- Money operations: test double-submit idempotency

**Test database setup:**
```bash
# Set test database connection string
export SMARTMETRO_TEST_DB='Server=(localdb)\MSSQLLocalDB;Database=SmartMetro_Test;Trusted_Connection=True;TrustServerCertificate=True'

# Run tests
bash scripts/ci.sh
```

## Running Tests

```bash
# All tests
bash scripts/ci.sh

# Specific test class
bash scripts/ci.sh --filter "FullyQualifiedName~AuthFlowTests"

# Using make (if available)
make test
```

## Test Coverage

- Every behavior change ships with tests
- Pure logic → unit tests
- Anything touching EF queries, auth, or HTTP shape → integration tests
- Every new endpoint on a user's resource → ownership tests
- Money paths → double-submit/idempotency tests
- External services (AamarPay, SMTP, Keycloak) → mocked at DI layer

See [AGENTS.md §6](../AGENTS.md#6-testing-expectations) for complete testing expectations.
