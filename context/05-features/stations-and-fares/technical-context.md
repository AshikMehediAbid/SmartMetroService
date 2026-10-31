---
type: feature-technical-context
title: Station and fare implementation
description: Current station routes, ordering, persistence, and fare calculation code map.
tags: [station, fare, api]
spec_version: 1
last_review: 2026-10-01
---
# Stations and Fares Technical Context

## Public surface

- `GET /api/station/{orderByHints}` lists stations; comment indicates `1` ascending and `0` descending.
- `POST /api/station/create`, `PUT /api/station/{stationId}`, and `DELETE /api/station/{stationId}` manage stations.
- `GET /api/station/fare?fromStationId=&toStationId=` returns fare data; omitted/zero destination is used by current service to return fares from an origin.
- `StationController` has no controller-level `[Authorize]` in the inspected source. Confirm intended protection before production use or reimplementation.

## Code map

- API: `SmartMetroService.Api/Controllers/StationController.cs`.
- Logic: `SmartMetroService.Application/Managers/StationService.cs`.
- Persistence/contracts: `Station`, `StationDistance`, `Settings`; `StationRepository`, `StationDistanceRepository`, `AdminRepository`.

## Design notes

Station order is contiguous and 1-based after update. Fare logic sums consecutive distances and applies settings via `CalculateFare`; inspect that helper and repositories for exact formula/rounding. `StationDistanceAndFareController` currently contains no actions. Avoid implementing in that placeholder without an agreed API design.
