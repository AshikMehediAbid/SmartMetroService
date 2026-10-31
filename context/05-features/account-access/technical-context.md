---
type: feature-technical-context
title: Account access implementation
description: Current API and application-service mapping for account flows.
tags: [account, api, authentication]
spec_version: 1
last_review: 2026-10-01
---
# Account Access Technical Context

## Public surface

- `POST /api/account/register`, `POST /api/account/login`, `GET /api/account/verifyemail?email=&otp=`, `POST /api/account/token`, and `POST /api/account/logout`.
- `POST /api/account/change-password` is marked `[Authorize]`; recovery and profile routes are in `AccountController` and must be inspected for their current exact binding/auth contract before change.
- Login response includes an access token plus email verification flags. Refresh token is an HttpOnly cookie named `refreshToken`.
- Auth accepts either application JWTs or Keycloak JWTs through `DefaultAuthenticationConfig`; Keycloak `realm_access.roles` is mapped to role claims.

## Code map

- API: `SmartMetroService.Api/Controllers/AccountController.cs`, `Configurations/DefaultAuthenticationConfig.cs`.
- Use cases: `SmartMetroService.Application/Managers/AccountService.cs`, `ProfileService.cs`, `OTPService.cs`, `EmailService.cs`.
- Contracts/entities: `IAccountService`, `IProfileService`, `IOTPService`, `User`, `UserOTP`, `Token`.
- Persistence: `AccountRepository`, `TokenRepository`, `UserOTPRepository`.

## Confirm before changing

Cookie secure/samesite/domain settings, token TTL and rotation policy, whether logout requires authorization, verification throttling, password reset semantics, role/claim mapping for each issuer, and enumeration-resistant recovery responses. Never trust a client-provided user email in place of the authenticated subject.
