---
type: architecture-overview
title: Current architecture
description: Project boundaries, runtime flow, integrations, and architecture guardrails.
tags: [architecture, dotnet]
spec_version: 1
last_review: 2026-10-01
---
# Architecture

## Project dependency shape

`SmartMetroService.Api` hosts HTTP controllers and startup configuration. It calls interfaces from `SmartMetroService.Application`, which contains use-case services, DTOs, and manager/repository contracts. `SmartMetroService.Domain` contains entities and enums. `SmartMetroService.Storage` implements repositories/unit of work and EF Core `MyApplicationDbContext` with SQL Server.

The API composition root registers application services and repositories, configures AutoMapper and JSON enum conversion, adds SQL Server and Redis, and configures CORS. `Program.cs` orders HTTPS redirection, CORS, authentication, authorization, and controller mapping. OpenAPI is mapped only in Development.

## External boundaries

- Manual JWT issuer and Keycloak issuer are handled by separate bearer handlers behind a policy scheme.
- AamarPay initiates and validates payments through application services.
- Email is used by verification/recovery flows.
- Redis stores temporary RapidPass entry-gate journey data.
- QR payloads are encrypted before QR generation; PDF output is provided for tickets.

## Guardrails

- Keep provider-specific code and schema mapping in Storage; controllers should not own business workflows.
- Keep transaction validation and fulfillment in Application, not in browser redirects alone.
- Treat cache/database consistency and repeated callbacks/scans as explicit design concerns.
- No formal ADR history was present when this context was authored. Record future non-obvious decisions as numbered, append-only ADRs only after they are agreed; do not invent historical decisions.
