---
type: feature-business-context
title: Account access and profile
description: Passenger registration, verification, sign-in, token lifecycle, and profile recovery.
tags: [account, authentication]
spec_version: 1
last_review: 2026-10-01
---
# Account Access and Profile

## Purpose

Allow a passenger to create an account, verify email, sign in, maintain a session, and recover or change a password.

## Observed rules

- Registration accepts name, email, phone number, password, and confirmation through a request DTO.
- Email verification uses an OTP. Login returns an access token and an `IsEmailVerified`/`IsEmailSent` result; the API response message reflects verification state.
- A refresh token is issued in an HttpOnly `refreshToken` cookie. Refresh/logout operations use that cookie; refresh tokens are stored hashed and support revocation.
- Password change requires authorization. Recovery is an email-based flow.

## Acceptance criteria for parity

- **AC1** A valid registration creates an account and initiates the verification flow; invalid input is rejected.
- **AC2** Verification accepts a valid, unexpired OTP and rejects an invalid/expired OTP.
- **AC3** Login returns an access token and sets an HttpOnly refresh cookie; unverified-account behavior is explicit in the response.
- **AC4** Refresh rotates/replaces the refresh token and rejects a missing, expired, revoked, or invalid token.
- **AC5** Logout clears the cookie and revokes the supplied refresh token where applicable.
- **AC6** Authorized password change and password recovery follow verified identity and disclosure-safe responses.

Confirm whether AC1-AC6 are required parity criteria for a rebuild; recovery behavior, token expiration, and endpoint authorization are not fully specified by business documentation.
