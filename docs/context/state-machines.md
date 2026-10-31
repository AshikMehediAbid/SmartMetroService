# State machines

Statuses and the transitions the code performs today. A change that adds a status or a transition updates this file
in the same PR. Statuses are stored as `int` (enums) or `nvarchar` (payment); there are no database CHECK constraints.

## Single-journey ticket (`TicketStatus`)

```text
            entry scan, station inside route       exit scan, station inside route
  Fresh ───────────────────────────────────> InUse ───────────────────────────────> Used
    │                                          │
    │ ExpiryTime passed                        │ ExpiryTime passed (on scan)
    ▼                                          ▼
  Expired                                   (scan rejected; status not changed — see note)
```

| From | Event | To | Where |
| --- | --- | --- | --- |
| — | purchase completes (wallet or verified online payment) | `Fresh`, `ExpiryTime = now + 2 days` | `TicketService.GenerateQrTicketAsync` |
| `Fresh` | entry-gate scan at a station between the ticket's from and to | `InUse` | `ScannerService.HandleFreshTicket` |
| `InUse` | exit-gate scan at a station between from and to | `Used` | `ScannerService.HandleInUseTicket` |
| `Fresh` | listing the user's tickets after `ExpiryTime` | `Expired` | `TicketRepository.MarkOldFreshTicketsAsExpiredAsync` |
| `Used`, `Expired` | any scan | rejected | `ScannerService.HandleSingleJourneyTicket` |

Rules: `Used` and `Expired` are terminal. A `Fresh` ticket scanned at an exit gate, or an `InUse` ticket at an entry
gate, is rejected without a change.

Note: when a scan finds a `Fresh` or `InUse` ticket past `ExpiryTime`, the code sets `ExpiryTime = now` and rejects
the scan but leaves the status as it was. An `InUse` ticket that expires inside the network stays `InUse`. Whether it
should become `Expired`, and what an expired passenger inside the gates must do, is open question Q4.

`InUse` (4) has a higher number than `Expired` (3); do not compare statuses numerically.

## Journey (`JourneyStatus`, RapidPass only)

| From | Event | To | Where |
| --- | --- | --- | --- |
| — | RapidPass exit scan with a matching entry in Redis | `Complete` (start = entry time, end = now) | `ScannerService.HandleRapidPassJourney` |

`InProgress` and `TimeOut` exist in the enum but nothing writes them: an entry lives only in Redis (1 day) until the
exit scan. An entry that is never closed simply expires from Redis, with no journey and no charge (open question Q5).

## RapidPass QR

| Event | Effect | Where |
| --- | --- | --- |
| first `GET api/tickets/rapidpass` | RapidPass created, QR with `ExpiryTime = now + 1 day` | `TicketService.GetOrCreateUserRapidPass` |
| `GET` after expiry | QR regenerated, new 1-day expiry | same |
| `PUT api/tickets/rapidpass` | QR regenerated, new 1-day expiry | `TicketService.RefreshRapidPass` |

The scanner does not check a RapidPass QR's expiry today (it checks wallet balance and the Redis entry).

## Payment (`Payment.Status`, strings)

```text
  Pending ──verified success──> Paid   (terminal)
     │
     ├──fail callback──> Failed
     └──cancel callback──> Cancelled
```

| From | Event | To | Where |
| --- | --- | --- | --- |
| — | AamarPay returned a `payment_url` | `Pending` | `AamarPayService.SavePaymentStatus` |
| `Pending` (or `Failed`/`Cancelled`) | success callback + validation `Successful` + amount equal | `Paid`, `PaidAt`, gateway ids stored; wallet credited or ticket created | `PaymentService.FinalizePaymentAsync` |
| `Paid` | success callback again | no change, reports success | same |
| any but `Paid` | fail / cancel callback | `Failed` / `Cancelled` | `PaymentService.UpdatePaymentStatusAsync` |

Rules: only a verified validation reaches `Paid`; `Paid` never changes. Today a `Failed` or `Cancelled` payment can still
become `Paid` if a later success callback validates, and the fail/cancel callbacks are anonymous (known gap G4).

## OTP (`UserOTP`)

| Event | Effect |
| --- | --- |
| generate | new row, `ExpiryDate` = now + 10 min (e-mail verification) or + 30 min (password recovery), `IsUsed = false` |
| verify via `POST api/account/Verify-otp` | valid when a row matches e-mail, code and type, is unused and unexpired (`UserOTPRepository.GetOtpDataAsync`); for e-mail verification the user is marked verified and the row used |
| verify via `GET api/account/verifyemail` | `OTPService.ValidateOtpAsync` accepts **any** unused, unexpired row for that e-mail and type without comparing the code (known gap G13) |

No attempt limit exists on either path.

## Refresh token (`Token`)

| Event | Effect |
| --- | --- |
| login / rotate | new row, `ExpiredAt = now + 1 month` |
| `POST api/account/token` with a live token | old row `RevokedAt = now`, new token issued |
| reuse of a revoked token | every active token of that user revoked; 401 |
| logout | all of the user's active tokens revoked (by `sub`), or the cookie's token |
