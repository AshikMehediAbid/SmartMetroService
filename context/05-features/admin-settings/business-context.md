---
type: feature-business-context
title: Admin settings
description: Maintain system settings that affect metro fares.
tags: [admin, settings]
spec_version: 1
last_review: 2026-10-01
---
# Admin Settings

## Purpose

Allow an administrator to read and update the system fare settings used by station fare and gate fare calculations.

## Observed rules

- The controller requires the `Admin` role.
- The settings entity contains `UnitFare` and `MinimumFare` plus common base fields.

## Acceptance criteria for parity

- **AC1** Only a principal with the agreed administrator role can read or update settings.
- **AC2** Read returns the current persisted setting values.
- **AC3** Update validates allowed values, persists the update, and returns the resulting state.
- **AC4** Fare calculations consistently use the latest accepted settings.

Validation ranges, audit requirements, effective-time behavior, and whether multiple settings records are allowed are **Confirm** items.
