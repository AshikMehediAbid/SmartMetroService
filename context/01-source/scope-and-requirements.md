---
type: product-scope
title: Scope and verified capabilities
description: Separates implemented capability from unconfirmed product expectations.
tags: [scope, requirements]
spec_version: 1
last_review: 2026-10-01
---
# Scope and Verified Capabilities

## Present in this repository

- ASP.NET Core HTTP API with account/profile, station/fare, ticket, payment, wallet, scanner, and admin controllers.
- Application services, domain entities, SQL Server persistence, EF Core migrations, and an xUnit integration-test project.
- Manual JWT and Keycloak bearer-token validation paths.
- Email OTP/password flows, AamarPay payment initiation/validation, QR generation, PDF ticket generation, and Redis-backed RapidPass entry state.

## Not established by repository evidence

- A frontend application, passenger journey UX, or hardware gate/device protocol.
- Production deployment topology, availability targets, retention policy, operational ownership, or regulatory requirements.
- Whether RapidPass is a prepaid pass, a fare-account credential, or another product beyond the behavior encoded in current services.
- Fare policy beyond the persisted settings and calculation implementation; discounts, caps, transfer rules, and rounding policy need confirmation.
- Whether station CRUD and scanner endpoints are intentionally public, or protected at an upstream boundary.

For new work, convert only explicitly agreed items into acceptance criteria. Preserve the existing API unless the requirement authorizes a contract change.
