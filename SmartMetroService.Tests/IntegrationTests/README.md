# Integration Tests

Tests that verify behavior across multiple layers using `WebApplicationFactory` against a real SQL Server test database.

## What belongs here

- Complete auth flows (register, login, logout, token refresh)
- API endpoints (controllers) with real database state
- Repository and query correctness
- Ownership and permission checks (another user's resource → 404)
- Payment and wallet operations with idempotency tests
- Scanning and journey recording workflows
- Complex transactions requiring database guarantees

## Test structure

```csharp
public class AuthFlowTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public AuthFlowTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task SomeEndpoint_WithValidInput_ReturnsExpectedResult()
    {
        // Create scope and access database
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<MyApplicationDbContext>();
        db.Database.EnsureDeleted();
        db.Database.EnsureCreated();

        // Create HTTP client
        var client = _factory.CreateClient();

        // Test the flow...
    }
}
```

## Key requirements

- Each test creates its own users and data (no test dependencies)
- Database is cleared before each test with `EnsureDeleted()` then `EnsureCreated()`
- No shared state between tests or test order dependency
- Ownership tests: attempt to access another user's resource → expect 404
- Permission tests: missing token → 401, non-admin on admin route → 403
- Money operations: test idempotency (double-submit should not double-charge)
- External services mocked at DI layer (never call real AamarPay, SMTP, Redis)

## Running integration tests

```bash
# Set test database (required)
export SMARTMETRO_TEST_DB='Server=(localdb)\MSSQLLocalDB;Database=SmartMetro_Test;Trusted_Connection=True;TrustServerCertificate=True'

# All integration tests
bash scripts/ci.sh --filter "Namespace=SmartMetroService.Tests.IntegrationTests"

# Specific test class
bash scripts/ci.sh --filter "FullyQualifiedName~AuthFlowTests"
```

## Example test patterns

### Ownership test
```csharp
[Fact]
public async Task GetUserWallet_WithAnothersUserId_Returns404()
{
    var client = _factory.CreateClient();
    var anotherUserId = "other-user@example.com";

    var response = await client.GetAsync($"/api/wallet/{anotherUserId}");

    Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
}
```

### Idempotency test (money operations)
```csharp
[Fact]
public async Task PaymentCallback_WithSameTransactionId_IsIdempotent()
{
    // First callback
    var response1 = await client.PostAsync("/api/payment/callback", payload);
    var ticketCount1 = db.Tickets.Count();

    // Second callback with same transaction id
    var response2 = await client.PostAsync("/api/payment/callback", payload);
    var ticketCount2 = db.Tickets.Count();

    // Should not create duplicate ticket
    Assert.Equal(ticketCount1, ticketCount2);
}
```
