---
type: feature-test-context
title: Ticket and RapidPass test contract
description: Test expectations for ticket issuance, ownership, status, and QR lifecycle.
tags: [tickets, rapidpass, tests]
spec_version: 1
last_review: 2026-10-01
---
# Ticket and RapidPass Test Context

No ticket/RapidPass-specific tests were identified in the current test project.

| Criterion | Test expectation |
| --- | --- |
| AC1 | Ticket fields, status, validity, encrypted payload identity/type, and one-time issuance. |
| AC2 | User isolation, status filtering, and expiry transition behavior. |
| AC3 | First creation, existing credential, expired credential, and refresh behavior. |
| AC4 | Existing ticket PDF and cross-user access denial. |
| AC5 | Tampered ciphertext, malformed payload, wrong type, and expired payload rejection at the scanner boundary. |

Coordinate QR contract tests with `gate-validation`; use deterministic time and isolated repository/cache substitutes where practical.
