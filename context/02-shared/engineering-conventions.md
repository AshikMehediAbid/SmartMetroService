---
type: engineering-conventions
title: Engineering conventions
description: Layering and change conventions observed in SmartMetroService.
tags: [conventions, architecture]
spec_version: 1
last_review: 2026-10-01
---
# Engineering Conventions

- Keep HTTP binding/status codes in `SmartMetroService.Api`; put use-case orchestration in `SmartMetroService.Application`; domain state/types in `SmartMetroService.Domain`; EF Core and repository implementations in `SmartMetroService.Storage`.
- Application services depend on `IManagers`/`IRepositories` contracts and `IUnitOfWork`, not directly on API controllers or concrete EF context where an abstraction exists.
- Register services and repositories in `SmartMetroService.Api/Configurations/DependencyConfig.cs`; authentication is configured in `DefaultAuthenticationConfig.cs` and middleware in `Program.cs`.
- Use request/response DTOs at API boundaries. Persisted entities are under `SmartMetroService.Domain/Entities`; schema changes require EF Core migrations.
- Preserve existing JSON enum string conversion and API response shapes unless an accepted requirement changes them.
- Follow neighboring async naming and nullable conventions. Keep user-visible errors useful but do not return secrets, stack traces, gateway credentials, or raw sensitive provider data.
- Avoid adding another abstraction or package when the existing service/repository patterns cover the use case.
