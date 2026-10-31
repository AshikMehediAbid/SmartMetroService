---
type: feature-business-context
title: Gate validation and journeys
description: Validate QR credentials and process entry/exit gate events.
tags: [gate, scanner, journey]
spec_version: 1
last_review: 2026-10-01
---
# Gate Validation and Journeys

## Purpose

Accept a passenger QR scan at a station gate, validate the credential, and record entry/exit and fare consequences.

## Observed rules

- Single-journey tickets accept entry only while fresh and within their permitted station range; entry changes status to `InUse`. Exit consumes the ticket and records journey information according to the service flow.
- RapidPass entry is stored in distributed cache with station and time. Exit requires active cached entry, computes fare from the station pair, checks wallet balance, records a journey, deducts fare, and removes the cached entry.
- QR payload is decrypted and parsed before routing to single-ticket or RapidPass handling.

## Acceptance criteria for parity

- **AC1** Malformed, forged, unsupported, or expired QR data is rejected without changing state.
- **AC2** Single-ticket entry/exit order, permitted stations, expiry, and one-time use are enforced.
- **AC3** RapidPass exit requires a prior entry and sufficient balance; successful exit records one journey and one debit.
- **AC4** Duplicate/concurrent gate events cannot create duplicate journeys, double-charge, or strand a credential without a recovery path.
- **AC5** Station/gate identifiers and scanner caller authorization are validated under the agreed device trust model.

The current scanner endpoint has no explicit `[Authorize]` attribute. Gate authentication, offline operation, event replay policy, and recovery from cache loss are **Confirm** items.
