---
type: feature-test-context
title: Gate validation test contract
description: Test expectations for QR parsing, ticket state transitions, and journey charging.
tags: [gate, scanner, tests]
spec_version: 1
last_review: 2026-10-01
---
# Gate Validation Test Context

No scanner/journey-specific tests were identified in the current test project.

| Criterion | Test expectation |
| --- | --- |
| AC1 | Invalid ciphertext, invalid JSON/ID, unsupported type, and expired payload leave state unchanged. |
| AC2 | Correct entry then exit succeeds; exit-first, repeated entry/exit, wrong station, used/expired ticket fail safely. |
| AC3 | Entry cache, missing cache, insufficient funds, exact fare, journey row, wallet debit, and cache removal. |
| AC4 | Repeated/concurrent requests yield at most one completed journey and one debit or a recoverable state. |
| AC5 | Only approved station/gate/device identities may submit scans. |

Use an in-memory/distributed-cache fake with controlled time. Add integration coverage for database state transitions if transaction semantics change.
