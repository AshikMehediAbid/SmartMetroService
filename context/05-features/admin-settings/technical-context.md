---
type: feature-technical-context
title: Admin settings implementation
description: Current role-protected settings endpoints and application service.
tags: [admin, settings, api]
spec_version: 1
last_review: 2026-10-01
---
# Admin Settings Technical Context

## Public surface

- `GET /api/admin/settings` and `PUT /api/admin/settings` are protected at controller level with `[Authorize(Roles = "Admin")]`.
- Current update accepts a `Settings` entity as input and returns it after service update.

## Code map

- API: `SmartMetroService.Api/Controllers/AdminController.cs`.
- Logic: `SmartMetroService.Application/Managers/AdminService.cs`.
- Domain/persistence: `Settings`, `IAdminRepository`, `AdminRepository`, `MyApplicationDbContext`.
- Role claims: manual JWT and Keycloak mapping in `DefaultAuthenticationConfig.cs`; verify each issuer produces the expected `Admin` claim.

## Change guard

Do not weaken role protection while changing payloads. Before exposing the endpoint externally, establish input validation, record selection semantics, audit requirements, and safe settings bounds.
