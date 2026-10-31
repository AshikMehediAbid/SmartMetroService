---
type: operations-guide
title: Local development and configuration
description: Local prerequisites, safe configuration keys, and project verification commands.
tags: [operations, development]
spec_version: 1
last_review: 2026-10-01
---
# Local Development

## Prerequisites

- .NET 10 SDK.
- SQL Server reachable through `ConnectionStrings:DefaultConnection`.
- Redis reachable through `ConnectionStrings:Redis` (code fallback: `localhost:6379`).
- For real account/payment flows: appropriate SMTP, AamarPay, Keycloak, JWT, encryption, and frontend URL configuration.

Exact configuration keys are read in `DependencyConfig`, `DefaultAuthenticationConfig`, and the relevant services. Supply values through user-secrets/environment-specific configuration. Never commit usable secrets. Do not connect tests to production services.

## Commands

```powershell
dotnet restore SmartMetroService.slnx
dotnet build SmartMetroService.slnx
dotnet test SmartMetroService.Tests/SmartMetroService.Tests.csproj
dotnet run --project SmartMetroService.Api/SmartMetroService.Api.csproj
```

OpenAPI is mapped in Development. Use the configured launch profile or inspect `SmartMetroService.Api/Properties/launchSettings.json` for the local URL. Apply database migrations using the repository's normal EF tooling and the intended environment connection string; do not run migrations against production without explicit approval.

## Operational questions before deployment

Set production CORS origins, HTTPS and Keycloak metadata policy, secret/key rotation, callback validation, rate limits, logging redaction, health checks, cache availability/recovery, database backup/retention, and payment reconciliation. These are not fully defined in the repository context.
