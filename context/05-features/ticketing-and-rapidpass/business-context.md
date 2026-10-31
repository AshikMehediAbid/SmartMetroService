---
type: feature-business-context
title: Ticketing and RapidPass
description: Issue and retrieve passenger QR tickets and RapidPass credentials.
tags: [tickets, rapidpass, qr]
spec_version: 1
last_review: 2026-10-01
---
# Ticketing and RapidPass

## Purpose

Provide a passenger with a scannable single-journey ticket or RapidPass credential and a way to inspect/download ticket information.

## Observed rules

- A single-journey ticket records user, origin, destination, fare, type, status, and expiry. The current service sets expiry two days after issue and initially marks it `Fresh`.
- QR content carries ticket/pass ID, expiry, and QR type in an encrypted payload. Ticket listing marks old fresh tickets expired before returning the selected status.
- A RapidPass is created per user when absent. Current behavior sets or renews an expiry around one day and regenerates QR bytes.
- Authenticated routes list tickets by status, get/create or refresh RapidPass, and download a ticket PDF.

## Acceptance criteria for parity

- **AC1** An eligible purchase creates one single-journey ticket with fare, stations, status, validity, and QR payload.
- **AC2** Passenger can list only their tickets by status; expired tickets are not presented as fresh.
- **AC3** Passenger can retrieve/create and refresh their own RapidPass under the agreed validity policy.
- **AC4** Ticket PDF download returns the requested ticket only to an authorized owner (ownership behavior must be verified).
- **AC5** QR payload cannot be forged by editing its visible fields and its expiry/type are checked during validation.

Ticket durations, ownership checks, refresh limits, and whether a RapidPass is a fare product or credential require confirmation for a new implementation.
