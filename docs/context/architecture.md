# Architecture

How SmartMetroService is put together today, read from the code (2026-10-31). The code wins over this file; when they
disagree, fix this file in the same PR.

## Projects and dependency direction

```text
SmartMetroService.Api ──> SmartMetroService.Application ──> SmartMetroService.Domain
        │                          ▲
        └──> SmartMetroService.Storage (implements Application's repository interfaces)
```

| Project | Holds | May reference |
| --- | --- | --- |
| `Domain` | Entities (`Entities/`), enums (`Enums/`, plus enums declared next to their entity) | nothing |
| `Application` | Managers (`Managers/*Service.cs`) and their interfaces (`Interfaces/IManagers`), repository interfaces and `IUnitOfWork` (`Interfaces/IRepositories`), DTOs (`Models/`), `MappingProfile` (AutoMapper), `ApiException` types | Domain; abstraction packages (configuration, caching, logging, HTTP client) and pure libraries (AutoMapper, BCrypt, QRCoder, QuestPDF, JWT) |
| `Storage` | `MyApplicationDbContext` (`Sql/`), `Repository<T>` and one repository per aggregate, `UnitOfWork`, EF migrations | Application, EF Core |
| `Api` | Controllers, `DependencyConfig` (all DI), `DefaultAuthenticationConfig` (auth), `ApiResponse<T>`, `Program.cs` | Application, Storage |
| `Tests` | xUnit tests through `WebApplicationFactory<Program>` | Api (and through it everything) |

The "manager" name is the folder; the classes are `…Service` and are registered as `I…Service`. One manager may call
another (`PaymentService` calls `StationService`, `WalletService`, `TicketService`, `AamarPayService`).

## Request path

1. `Program.cs`: `AddDependencies` → JWT auth → authorization → controllers. Middleware: HTTPS redirection, CORS policy
   `AllowNgFrontend` (`http://localhost:4200`, credentials allowed), authentication, authorization. OpenAPI JSON is
   mapped only in Development (`/openapi/v1.json`). No global exception handler on `master`.
2. A controller binds a DTO, reads the caller from `User` (`ClaimTypes.Email`, or `sub` for logout), calls one manager
   and maps the result to `ApiResponse<T>` or a plain object (see `api-conventions.md`).
3. A manager loads and changes entities through `IUnitOfWork` repositories, then calls `CompleteAsync()`
   (`SaveChangesAsync`). There is no explicit transaction anywhere; each `CompleteAsync` is its own transaction.

## Authentication

`DefaultAuthenticationConfig` registers a policy scheme `SmartMetro` that peeks at the bearer token's `iss` claim:

- `iss == Keycloak:Issuer` → scheme `Keycloak` (authority and audience from config; `realm_access.roles` copied into
  `ClaimTypes.Role` on validation; `RequireHttpsMetadata = false`).
- anything else → scheme `SmartMetroJwt`: our own HS256 token signed with `Jwt:Key`, issuer `Jwt:Issuer`, audience
  `Jwt:Audience`, zero clock skew. Issued by `AccountService` at login with claims `sub` (user id), name, e-mail,
  mobile phone and role; lifetime 3 minutes.

Refresh tokens: 64 random bytes, base64, stored as a SHA-256 hex hash in `RefreshTokens`, sent to the browser in the
`refreshToken` cookie (HttpOnly, Secure, SameSite=None, 30 days). `POST api/account/token` rotates it; reusing a
revoked token revokes every active token of that user. Keycloak users are mirrored into `Users` by
`POST api/account/keycloak-user`.

Authorization is per action with `[Authorize]` / `[Authorize(Roles = "Admin")]`; there is no fallback policy, so an
action without an attribute is anonymous.

## Main flows

**Fare lookup** (`StationService.GetFare`): find both stations, walk the station order between them summing the
`StationDistances` of each consecutive pair, then `fare = max(UnitFare × distance, MinimumFare)` rounded to the nearest
10 with `Math.Round` (banker's rounding; see open question Q3). With no destination it returns the fare to every other
station.

**Buy a single-journey ticket with the wallet** (`POST api/payment`, `PaymentMethod.AccountBalance`):
token e-mail must equal the body's `UserEmail` → fare from `GetFare` → balance check → wallet debit (saved) → ticket
created `Fresh` with a 2-day expiry (saved) → QR bytes generated from the encrypted `QrPayload` (saved). Three saves.

**Buy online** (`PaymentMethod.Online`, also for RapidPass purchase and wallet recharge): `AamarPayService` posts the
payment request (new `tran_id` GUID) and stores a `Pending` `Payment` row, then returns AamarPay's `payment_url`.
AamarPay later posts the browser back to `api/payment/success|fail|cancel` with `mer_txnid`:
- success → server-side validation call to AamarPay → if `Successful`, `FinalizePaymentAsync` checks the stored row,
  status and amount, then credits the wallet (recharge) or creates the ticket, marks the row `Paid` and redirects the
  browser to `BaseFeUrl?transactionId=…&status=…`.
- fail / cancel → status `Failed` / `Cancelled` unless already `Paid`, then redirect.

**Scan a QR at a gate** (`POST api/scanner/qr-data`, anonymous): decrypt the payload (AES, key and IV from
`EncryptionKey`), then by `QrType`:
- `SingleJourney`: `Fresh` + entry gate inside the ticket's route → `InUse`; `InUse` + exit gate inside the route →
  `Used`; expired, used or wrong gate → error.
- `RapidPass`: wallet balance ≥ `MinimumFare` required. Entry → store `{StationId, EntryTime}` in Redis under the
  RapidPass id for one day. Exit → read the entry, compute the fare, require balance ≥ fare, add a `Journey`
  (`Complete`), debit the wallet (one save), remove the Redis key.

**RapidPass** (`GET/PUT api/tickets/rapidpass`): one per user, created on first request; the QR is regenerated with a
1-day expiry when expired (GET) or on demand (PUT).

**Ticket PDF** (`GET api/tickets/download-ticket/{id}`): `PdfService` renders the ticket and its QR with QuestPDF.

## Configuration keys

| Key | Used by | Notes |
| --- | --- | --- |
| `ConnectionStrings:DefaultConnection` | DbContext | user secrets locally; tests override it with `SMARTMETRO_TEST_DB` |
| `ConnectionStrings:Redis` | `IDistributedCache` | defaults to `localhost:6379` |
| `Jwt:Key`, `Jwt:Issuer`, `Jwt:Audience` | own JWT | key must be ≥ 32 bytes for HS256 |
| `Keycloak:Issuer`, `Keycloak:Audience` | Keycloak scheme | |
| `Email:*` | `EmailService` (SMTP) | OTP and password-recovery mail |
| `EncryptionKey:Key`, `EncryptionKey:IV` | `EncryptionService` | QR payload encryption |
| `AamarPay:StoreId`, `SignatureKey`, `ApiUrl`, `ValidationApiUrl`, `CallbackUrls:*` | `AamarPayService` | |
| `BaseFeUrl` | `PaymentController` | where callbacks redirect the browser |
| `AdminConfiguration:*` | — | present in `appsettings.json`; no code reads it on `master` |

Real values belong in user secrets or environment variables (`AGENTS.md` §2a).
