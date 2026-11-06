# Unit Tests

Fast, isolated tests for pure logic with no external dependencies.

## What belongs here

- Domain logic: fare calculations, ticket state transitions, QR payload encryption/decryption
- Application services and managers tested in isolation (dependencies mocked)
- Utility functions and validators
- Request/response DTO mapping and validation

## What does NOT belong here

- Tests that require a database
- Tests that need `WebApplicationFactory`
- Tests that call external services (AamarPay, SMTP, Keycloak, Redis)
- Tests that depend on HTTP or authentication

## Example structure

```
UnitTests/
├── Domain/
│   ├── FareCalculationTests.cs
│   └── TicketStateTransitionTests.cs
├── Application/
│   ├── Services/
│   │   └── StationServiceTests.cs
│   └── Managers/
│       └── AccountManagerTests.cs
└── Common/
    └── QrPayloadTests.cs
```

## Running unit tests

```bash
# All unit tests
bash scripts/ci.sh --filter "Namespace=SmartMetroService.Tests.UnitTests"

# Specific test class
bash scripts/ci.sh --filter "FullyQualifiedName~FareCalculationTests"
```
