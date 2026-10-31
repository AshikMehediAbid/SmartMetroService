---
type: constraints
title: Product and technical constraints
description: Verified constraints and decisions requiring confirmation before rebuilding or extending the service.
tags: [constraints, security]
spec_version: 1
last_review: 2026-10-01
---
# Constraints

## Verified

- Projects target `net10.0`, use nullable reference types, and enable implicit usings.
- SQL Server is the EF Core provider. Redis is registered as distributed cache, defaulting to `localhost:6379` when no Redis connection is configured.
- The configured CORS policy allows `http://localhost:4200` with credentials.
- Payment currency defaults to BDT in the payment entity; payment integration is AamarPay.
- Configuration contains credentials and cryptographic keys and must be supplied outside committed context documentation.

## Confirm before a new build or production change

- Supported .NET runtime/hosting environment and required versions of SQL Server, Redis, Keycloak, SMTP, and AamarPay.
- Production CORS origins, HTTPS/metadata requirements, secret storage, key rotation, and callback allowlisting.
- Station/fare rules, ticket validity and refund policy, RapidPass pricing/validity, and wallet accounting rules.
- Authorization for all administrative and gate/device operations; do not assume development defaults are production policy.
