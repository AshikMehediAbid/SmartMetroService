---
type: testing-guidance
title: Testing guidance
description: Test framework, current coverage, and expected gates for changes.
tags: [testing, quality]
spec_version: 1
last_review: 2026-10-01
---
# Testing

## Current baseline

`SmartMetroService.Tests` uses xUnit and ASP.NET Core `WebApplicationFactory<Program>`. The checked-in `AuthFlowTests` exercises registration, login, HttpOnly refresh-cookie issuance, and logout cookie clearing against an EF InMemory provider. Do not assume the rest of the feature behavior has automated coverage.

## Change gate

- Add or update tests for changed acceptance criteria, including invalid input and relevant authorization/failure paths.
- For payment and ticket integrity changes, cover duplicate callbacks, mismatched amount/transaction, repeat scans, and persistence effects as applicable.
- For station/fare changes, cover ordering, missing station/distance, and minimum/rounding cases as applicable.
- Run the focused test first, then `dotnet test SmartMetroService.Tests/SmartMetroService.Tests.csproj` for a meaningful change when the environment supports it.
- Tests must use fake credentials and isolated data. Never call a live payment gateway or send real email from automated tests.
