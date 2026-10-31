---
type: feature-technical-context
title: Payment and wallet implementation
description: Current payment routes, AamarPay validation and fulfillment, and wallet code map.
tags: [payments, wallet, api]
spec_version: 1
last_review: 2026-10-01
---
# Payments and Wallet Technical Context

## Public surface

- `POST /api/payment` is authorized; it compares the email claim with the request email before calling `IPaymentService`.
- Anonymous form callbacks: `POST /api/payment/success`, `/fail`, `/cancel`. Success performs provider validation and redirects to configured frontend URL with status/transaction query values.
- `GET /api/payment/status/{merTxnId}` is authorized. Verify transaction ownership before exposing it; the inspected controller passes only the transaction ID to the service.
- `GET /api/wallet/balance` is authorized and derives email from claims.

## Code map

- API: `PaymentController`, `WalletController`.
- Logic: `PaymentService`, `AamarPayService`, `WalletService`.
- Data/contracts: `Payment`, wallet entity, purchase/status DTOs, payment enums.
- Persistence: `PaymentRepository`, `WalletRepository`, account repository.

## Integrity notes

Provider callback is a redirect surface, not proof of payment. Preserve server-to-server validation, transaction/amount checks, and idempotent fulfillment. Inspect transaction boundaries and the duplicate `CompleteAsync` call in current finalization before changing persistence behavior. Never log provider secrets or include them in redirect URLs.
