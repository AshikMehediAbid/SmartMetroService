---
type: feature-test-context
title: Payment and wallet test contract
description: Test expectations for payment integrity, wallet accounting, and callback handling.
tags: [payments, wallet, tests]
spec_version: 1
last_review: 2026-10-01
---
# Payment and Wallet Test Context

No payment/wallet-specific tests were identified in the current test project.

| Criterion | Test expectation |
| --- | --- |
| AC1 | Claimed identity mismatch is rejected; amount is derived/validated server-side. |
| AC2 | Sufficient/insufficient balance outcomes and atomicity of debit plus ticket issuance. |
| AC3 | Pending record and expected provider request are created without a live gateway. |
| AC4 | Wrong transaction, failed status, amount mismatch, replayed success, and exact valid success. |
| AC5 | Failed/cancelled updates and paid-state immutability; status lookup behavior. |
| AC6 | Cross-user balance/transaction access is denied; wallet ledger effects are exact. |

Stub the AamarPay HTTP boundary. Assert persisted payment, ticket, and wallet state, not only response status.
