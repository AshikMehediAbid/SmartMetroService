---
type: technology-stack
title: Technology stack
description: Verified framework, libraries, persistence, and integrations in the repository.
tags: [stack, dotnet]
spec_version: 1
last_review: 2026-10-01
---
# Technology Stack

| Area | Current implementation |
| --- | --- |
| Runtime/API | .NET 10, ASP.NET Core controllers, OpenAPI in Development |
| Architecture | API, Application, Domain, Storage projects |
| Persistence | EF Core 10, SQL Server, migrations, repository/unit-of-work abstractions |
| Cache | `IDistributedCache` backed by StackExchange Redis |
| Authentication | ASP.NET Core JWT bearer; manual SmartMetro JWT and Keycloak handlers selected by issuer |
| Mapping/validation | AutoMapper; DTOs and ASP.NET model validation |
| QR/PDF | QRCoder and QuestPDF |
| Payment | AamarPay HTTP integration |
| Tests | xUnit, `Microsoft.AspNetCore.Mvc.Testing`, EF Core InMemory |

Package versions are defined by the project files and may change. Email, Keycloak, database, cache, payment, and encryption configuration must be checked in `appsettings*.json` by key name only; never copy configured secret values into context or examples.
