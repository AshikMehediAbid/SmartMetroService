---
type: feature-business-context
title: Payments and wallet
description: Purchase tickets or recharge wallet through account balance or online payment.
tags: [payments, wallet]
spec_version: 1
last_review: 2026-10-01
---
# Payments and Wallet

## Purpose

Collect payment for a single-journey ticket or wallet recharge, record transaction status, and fulfill only a validated successful transaction.

## Observed rules

- A single-journey fare is calculated server-side from station selection.
- Account-balance payment is supported for single-journey tickets and deducts wallet funds before ticket generation.
- Online payment uses AamarPay. The success callback validates the merchant transaction with the provider; current fulfillment credits wallet for wallet recharge or generates a ticket for other payment purposes.
- Payment status can be queried by merchant transaction ID. Paid records are not overwritten by fail/cancel updates in the service.

## Acceptance criteria for parity

- **AC1** Caller identity is checked against purchase identity; server computes ticket fare rather than trusting client amount.
- **AC2** Account-balance purchase rejects insufficient funds and creates/deducts/fulfills consistently.
- **AC3** Online initiation persists a pending transaction and returns the provider URL.
- **AC4** Fulfillment requires successful provider validation, matching transaction identity, and exact expected amount; repeat callback does not duplicate credit/ticket.
- **AC5** Fail/cancel/status responses reflect persisted state without changing an already-paid transaction.
- **AC6** Wallet balance is scoped to the authenticated user; recharge and fare debits are auditable.

Refunds, reconciliation, currency conversion, failure recovery between separate database writes, and wallet ledger policy are **Confirm** items.
