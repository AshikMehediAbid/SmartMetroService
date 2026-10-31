---
type: loading-policy
title: Context loading map
description: Routes coding tasks to the smallest relevant set of SmartMetroService context documents.
tags: [architecture, loading-policy]
spec_version: 1
last_review: 2026-10-01
---
# Context Loading Map

## Always read

- `agent-rules.md`
- `02-shared/engineering-conventions.md`
- `02-shared/testing.md`
- `04-architecture/architecture.md`

## Read by task

| Task | Add these documents |
| --- | --- |
| Product scope or new capability | `01-source/vision.md`, `01-source/scope-and-requirements.md`, `03-business/domain-overview.md`, relevant feature business context |
| Account, authentication, profile | `05-features/account-access/` all three contracts |
| Stations, ordering, distance, fare | `05-features/stations-and-fares/` all three contracts |
| Tickets, QR issuance, RapidPass | `05-features/ticketing-and-rapidpass/` all three contracts |
| Payment gateway or wallet | `05-features/payments-and-wallet/` all three contracts |
| Gate scanning or journey lifecycle | `05-features/gate-validation/` all three contracts, plus ticketing technical context |
| Admin or fare settings | `05-features/admin-settings/` all three contracts |
| Persistence/schema | `02-shared/tech-stack.md`, `04-architecture/data-model.md`, related feature technical and test contexts |
| Authentication/authorization | `02-shared/tech-stack.md`, account-access technical context, admin-settings technical context |
| Local setup or deployment | `13-operations/local-development.md` |
| System boundaries or component design | `08-c4-diagram/system-context.md`, `04-architecture/architecture.md` |
| New work planning | `WORKFLOW.md`, `06-phase/README.md`, relevant source and feature business contexts |

Read source files only after context narrows the implementation surface. Use the technical contract's code map, then verify the current implementation and tests; paths and behavior can drift.

## Evidence policy

Business context describes behavior verified in the code or explicit repository documentation. It does not silently turn an implementation detail into a requirement. Items marked **Confirm** are unresolved and must be decided before an AI treats them as acceptance criteria. The source code and tests are authoritative for what currently runs; an approved requirement is authoritative for what new work should do.
