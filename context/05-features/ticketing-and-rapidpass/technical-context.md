---
type: feature-technical-context
title: Ticket and RapidPass implementation
description: Current ticket endpoints, QR payload generation, entities, and related service code.
tags: [tickets, rapidpass, qr]
spec_version: 1
last_review: 2026-10-01
---
# Ticketing and RapidPass Technical Context

## Public surface

- `GET /api/tickets?ticketStatus=...` is authorized and returns the caller's tickets for a status.
- `GET /api/tickets/rapidpass` and `PUT /api/tickets/rapidpass` are authorized get/create and refresh operations.
- `GET /api/tickets/download-ticket/{id}` is authorized and returns PDF. Verify owner scoping in `PdfService` before changing or claiming isolation.
- Payment fulfillment calls `ITicketService.GenerateQrTicketAsync`; the scanner consumes the encrypted `QrPayload`.

## Code map

- API: `SmartMetroService.Api/Controllers/TicketController.cs`.
- Logic: `SmartMetroService.Application/Managers/TicketService.cs`, `QrCodeService.cs`, `PdfService.cs`.
- Payload/contracts: `Ticket`, `RapidPass`, `QrPayload`, ticket DTOs, `TicketStatus`, `TicketType`.
- Persistence: `TicketRepository`, `RapidPassRepository`.

## Public surface for dependent features

Ticket issuance accepts user, stations, fare, and type through the application service. QR generation serializes ID/expiry/type, encrypts it, then renders the QR bytes. The scanner should use the same payload/encryption contract; do not create a parallel token format.
