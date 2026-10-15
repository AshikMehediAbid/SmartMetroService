# SmartMetroService

SmartMetroService is an ASP.NET Core backend for managing metro services (accounts, authentication, storage and business logic). It provides a minimal API surface, application-layer managers, domain entities, and a storage layer (EF Core + SQL). Intended for developers building a metro/transportation backend or using it as a starter template for .NET service-oriented applications.

## Stack
- Language(s): C#
- Framework / runtime: ASP.NET Core (minimal API) — Target framework as configured in project files (see csproj)
- Notable libraries:
  - Microsoft.AspNetCore.Authentication.JwtBearer (JWT auth)
  - Microsoft.EntityFrameworkCore.SqlServer (EF Core SQL Server)
  - Microsoft.AspNetCore.OpenApi / Swashbuckle (OpenAPI)
  - Microsoft.Extensions.Caching.StackExchangeRedis (caching support)

## Repository layout
Top-level tree (important entries only):
```
SmartMetroService.slnx             # Solution file
SmartMetroService.Api/            # Minimal API project (entry point)
  ├─ Program.cs                   # App startup and routing
  ├─ appsettings.json
  ├─ appsettings.Development.json
  ├─ Controllers/                 # API controllers (if present)
  ├─ Models/                      # DTOs / request/response models
  └─ SmartMetroService.Api.http   # HTTP request collection (sample requests)
SmartMetroService.Application/    # Application layer (managers, interfaces, mapping)
  ├─ Managers/
  ├─ Interfaces/
  └─ Models/
SmartMetroService.Domain/         # Domain entities and enums
  ├─ Entities/
  └─ Enums/
SmartMetroService.Storage/        # Storage layer (EF Core repositories, migrations, SQL scripts)
  ├─ Repositories/
  ├─ Migrations/
  └─ Sql/
SmartMetroService.Tests/          # Automated tests (integration-like / auth flow tests)
  └─ AuthFlowTests.cs
README.md
```

How it fits together:
- The API project (SmartMetroService.Api) is the entry point. Program.cs registers services, maps controllers/endpoints and hosts the web app.
- The Application layer exposes managers and use-case logic which the API calls to perform operations.
- The Domain project defines the core entities and enums used across the app.
- The Storage project contains EF Core repositories and migrations for persistence (SQL Server).
- Tests exercise the public endpoints (see AuthFlowTests.cs for example register/login/logout flows).

## Quick start — run locally

Prerequisites
- .NET SDK matching the solution target (see each .csproj for TargetFramework, e.g. `net10.0`)
- SQL Server (or a compatible SQL Server instance)
- (Optional) Redis if you want to enable Redis caching
- (Optional) dotnet-ef tool for migrations: `dotnet tool install --global dotnet-ef`

1. Clone the repo
```bash
git clone https://github.com/AshikMehediAbid/SmartMetroService.git
cd SmartMetroService
```

2. Configure environment variables or appsettings
Create or update `appsettings.*.json` or set env vars. Common settings to provide:
- Connection string
  - ConnectionStrings__DefaultConnection="Server=...;Database=SmartMetro;User Id=...;Password=...;"
- JWT settings (names may vary; check project config / Program.cs or configuration keys)
  - Jwt:Issuer
  - Jwt:Audience
  - Jwt:Secret
- ASPNETCORE_ENVIRONMENT=Development

3. Restore, build and run API
```bash
dotnet restore
dotnet build
dotnet run --project SmartMetroService.Api
```
By default the app will start on the ports configured by ASP.NET Core (check console output). If OpenAPI is enabled you can visit `/swagger` to explore endpoints.

4. Apply EF Core migrations (if using the built-in migrations)
```bash
# From repo root or SmartMetroService.Storage project folder
dotnet ef database update --project SmartMetroService.Storage --startup-project SmartMetroService.Api
```

## API — common endpoints (examples)
The test suite demonstrates these flows. Endpoints and exact routes can be confirmed in `SmartMetroService.Api` controllers or `SmartMetroService.Api.http`.

- Register (example payload — POST /api/account/register)
```json
{
  "name": "Test User",
  "email": "test@example.com",
  "phoneNumber": "+8801234567890",
  "password": "StrongPass123!",
  "confirmPassword": "StrongPass123!"
}
```

- Login (POST /api/account/login)
```json
{
  "phoneNumber": "+8801234567890",
  "password": "StrongPass123!"
}
```
The login response typically returns a JWT. Use it for protected endpoints:
Authorization: `Bearer <token>`

- Logout (POST /api/account/logout)

See `SmartMetroService.Api/SmartMetroService.Api.http` and `SmartMetroService.Tests/AuthFlowTests.cs` for concrete request/response sequences used in tests.

## Running tests
From the repository root:
```bash
dotnet test
```
The `SmartMetroService.Tests` project contains an `AuthFlowTests.cs` that exercises account register/login/logout flows.

## Development notes & conventions
- Projects use a layered architecture: Api → Application → Domain → Storage.
- Application layer contains managers and interfaces to decouple API from persistence.
- Storage uses EF Core repositories located under `SmartMetroService.Storage/Repositories`.
- User secrets are referenced in the API csproj (UserSecretsId) for convenient local secrets during development.

## Troubleshooting
- If migrations fail, verify the connection string and that the database server is reachable.
- If JWT-authenticated endpoints return 401, confirm Jwt:Secret, Issuer and Audience values match what is used to generate tokens.
- For Swagger/OpenAPI issues, check that OpenAPI is enabled in Program.cs and that development settings allow the UI.

## Contributing
Contributions are welcome. For code changes:
1. Fork the repo
2. Create a feature branch: `git checkout -b feat/your-feature`
3. Run tests and ensure they pass: `dotnet test`
4. Open a pull request describing the change

Please follow existing code style and add tests for new behavior.

## License
Add a LICENSE file to explicitly choose a license (MIT, Apache-2.0, etc.). If you want a recommendation, MIT is a common permissive choice.

## Contact
Maintainer: AshikMehediAbid (GitHub: @AshikMehediAbid)


Try asking:
- "Where in SmartMetroService.Storage are repository interfaces implemented and which repository classes handle user accounts?"
- "Program.cs shows commented auth setup — do you want JWT middleware enabled by default and what exact configuration keys should be used?"
- "Can you add a Dockerfile and docker-compose to run SQL Server + the API for easier local setup?"
