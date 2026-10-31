---
type: feature-business-context
title: Stations and fares
description: Ordered station network, adjacent distances, and journey fare lookup.
tags: [station, fare]
spec_version: 1
last_review: 2026-10-01
---
# Stations and Fares

## Purpose

Maintain metro stations and their ordered route distances, and provide fares between a selected origin and destination or from an origin to available destinations.

## Observed rules

- Stations have an order and persisted location/coordinate/active data.
- Station create/update can change route order; update replaces the links to adjacent stations using submitted neighboring distances. Delete shifts later station orders and can fail when references prevent deletion.
- Fare lookup uses station order, consecutive distances, `UnitFare`, and `MinimumFare` from system settings.

## Acceptance criteria for parity

- **AC1** Station listing returns stations in requested order with adjacent-distance information.
- **AC2** Create/update validates duplicate names and valid insertion position; station order remains contiguous.
- **AC3** Updating route position replaces only the relevant adjacent distance links and preserves unrelated links.
- **AC4** Delete handles missing and referenced stations without corrupting route order.
- **AC5** Fare lookup returns the selected-pair fare or the origin's available destination fares; missing stations return a defined empty/error response.

Fare formula, same-station journeys, rounding, inactive stations, and route branching are **Confirm** decisions. Keep the existing source behavior until an accepted requirement resolves them.
