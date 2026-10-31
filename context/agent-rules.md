---
type: agent-rules
title: AI development rules
description: Non-negotiable rules for AI agents changing SmartMetroService.
tags: [rules, agents]
spec_version: 1
last_review: 2026-10-01
---
# Agent Rules

1. Read `architecture-map.md` first. Load only the documents it routes to the task.
2. Before changing behavior, state the user/problem, acceptance criteria, and affected feature. Ask for clarification when a decision marked **Confirm** controls the implementation.
3. Preserve the API/Application/Domain/Storage dependency direction and existing repository abstractions unless an approved design changes them.
4. Do not infer product requirements from names, comments, sample data, or one code path. Verify behavior in its service, persistence mapping, endpoint, and tests as relevant.
5. Treat payment callbacks, wallet balances, ticket issuance, QR validation, token rotation, and station fares as integrity-sensitive. Preserve idempotency and ownership checks; add tests for failure and duplicate/replay cases when changing them.
6. Never write credentials, live tokens, private keys, connection-string secrets, personal data, or real payment payloads into source, tests, logs, or context files. Use configuration placeholders and test-only values.
7. Do not change database schema without a migration. Do not remove or rewrite existing migrations to make a local database work.
8. Add or update focused tests for changed behavior. Run the narrowest useful test first, then the relevant project test suite when practical. Report what was and was not run.
9. Keep edits scoped. Do not silently change endpoint contracts, auth requirements, payment semantics, enum serialization, or persisted meanings as cleanup.
10. Update the affected `technical-context.md` and `test-context.md` when implementation or test coverage changes. Update business context only after requirements are agreed.
11. Keep feature-to-feature dependencies limited to the `## Public surface` section of another feature's technical context. If a contract is insufficient, update it deliberately.
12. Do not claim an endpoint, integration, test, or deployment works unless verified in this repository or an executed check.
