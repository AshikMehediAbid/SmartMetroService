---
type: data-model
title: Persisted domain model
description: Main EF Core entities and relationships for the existing service.
tags: [database, domain]
spec_version: 1
last_review: 2026-10-01
---
# Data Model

The EF Core context declares sets for `User`, `UserOTP`, refresh `Token`, `Station`, `StationDistance`, `Settings`, `UserWallet`, `Ticket`, `Journey`, `Payment`, and `RapidPass`.

Key relationships and state:

- Users own tickets, wallet state, payments, refresh tokens, and RapidPass credentials.
- Tickets reference optional origin/destination stations and may have journeys. Ticket status/type are enums.
- Journeys reference a user, origin/destination stations, and optionally a RapidPass; they carry fare, start/end timestamps, and status.
- Payments persist merchant transaction ID, amount/currency/status, provider transaction data, payment purpose, ticket type/stations, and ticket-generated state.
- Stations carry a unique domain ID and station order; station distances link station IDs and distance.
- `BaseEntity` supplies created/updated timestamps and a soft-delete flag to selected entities.

The exact constraints, delete behaviors, indexes, conversions, and seed state live in `SmartMetroService.Storage/Sql/MyApplicationDbContext.cs` and EF migrations. Inspect those before changing schema; this summary is not a replacement for them.
