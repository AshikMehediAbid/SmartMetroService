---
type: business-domain
title: Metro service domain overview
description: Cross-feature entities and behaviors visible in the current codebase.
tags: [business, domain]
spec_version: 1
last_review: 2026-10-01
---
# Domain Overview

The current model links passengers to tickets, payments, wallet balances, RapidPass credentials, and journeys. Stations are ordered; distances connect station pairs; system settings supply unit and minimum fare values. A single-journey ticket starts `Fresh`, becomes `InUse` after an accepted entry scan, and is consumed after exit. A RapidPass records an entry in distributed cache and creates a completed journey and wallet deduction at exit.

Account registration creates an application user and uses email OTP verification. Sign-in issues an access token and refresh token cookie. Online payments are initiated through AamarPay and validated server-to-server before the application marks the payment paid and issues a ticket or credits wallet funds.

These are observed flows, not a complete operating policy. Confirm business decisions for fare rounding, refunds, ticket expiry, fare caps, concurrent scans, RapidPass lifecycle, settlement/reconciliation, and account deletion before implementing them.
