---
type: glossary
title: Domain glossary
description: Shared terms used by SmartMetroService contracts and code.
tags: [glossary, domain]
spec_version: 1
last_review: 2026-10-01
---
# Glossary

| Term | Meaning in the current implementation |
| --- | --- |
| Station | A metro stop with an integer ID, display/location data, active flag, and ordering position. |
| Station distance | A persisted distance between consecutive stations, used to derive journey distances/fares. |
| Single-journey ticket | A purchased ticket with origin/destination, fare, QR bytes, status, and expiry. |
| RapidPass | A user-associated QR credential. Current scanner flow records entry state in distributed cache and deducts computed fare at exit. |
| Journey | A recorded trip with user, entry/exit stations and times, fare, status, and optional RapidPass reference. |
| Wallet | A user's stored balance, used for single-ticket account-balance purchase and RapidPass fare deduction; recharge is represented in payment flow. |
| `Fresh` / `InUse` / `Used` / `Expired` | Ticket lifecycle enum values; exact transitions are described in the ticketing and gate-validation contracts. |
| AamarPay | External payment gateway used for online payment initiation and server-side transaction validation. |
| Refresh token | Opaque token returned in the `refreshToken` cookie; persisted as a hash and rotated by the account service. |
