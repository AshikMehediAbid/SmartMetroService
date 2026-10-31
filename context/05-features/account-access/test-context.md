---
type: feature-test-context
title: Account access test contract
description: Existing account-flow test and coverage expectations.
tags: [account, tests]
spec_version: 1
last_review: 2026-10-01
---
# Account Access Test Context

## Existing coverage

`SmartMetroService.Tests/AuthFlowTests.cs` registers and logs in a user, asserts an HttpOnly refresh cookie, logs out, and asserts a cookie-clearing header. It uses `WebApplicationFactory<Program>` and EF InMemory.

## Coverage to preserve or add

| Criterion | Test expectation |
| --- | --- |
| AC1 | Valid DTO persists user; invalid/duplicate registration follows the agreed API contract. |
| AC2 | Correct OTP succeeds; incorrect and expired OTP fail. |
| AC3 | Access token and cookie are issued; cookie security flags and unverified behavior match policy. |
| AC4 | Refresh rotates once; replay/revoked/expired/missing token is rejected. |
| AC5 | Logout revokes token and expires cookie, including repeated logout behavior if supported. |
| AC6 | Password change/recovery handles invalid identity without leaking account existence or secrets. |

Before relying on integration setup, verify how `WebApplicationFactory` replaces SQL Server, Redis, SMTP, and gateway dependencies. Do not call external services in tests.
