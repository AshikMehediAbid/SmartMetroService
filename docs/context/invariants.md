# Invariants

Rules the domain must keep. Each names what enforces it **today** and its status: ✅ enforced (code + test),
🟡 code only (no test), 🔴 not enforced (a known gap, `known-gaps.md`). A change that touches an invariant keeps it
and, where it is 🟡 or 🔴, adds the test or fix only when that is the issue being worked on.

## Money and fares
| # | Invariant | Enforcement | Status |
| --- | --- | --- | --- |
| M1 | A journey's fare is computed on the server from station order, station distances and `Settings`; a client-supplied amount is never used for a journey. | `PaymentService.CompletePaymentAsync` overwrites `Amount` for `SingleJourney`; `ScannerService` computes RapidPass fares | 🟡 |
| M2 | A wallet recharge amount is greater than zero. | `PaymentService.CompletePaymentAsync`, `WalletService.IncreaseWalletBalanceAsync` | 🟡 |
| M3 | Money is exact: `decimal` in code, `decimal(18,2)` in SQL Server, BDT. | `Payment.Amount` only; fares are `int`, wallet balance is `double` | 🔴 G6 |
| M4 | A wallet balance never goes negative. | balance checks before debit (wallet purchase, RapidPass exit), not atomic with the debit | 🔴 G10 |
| M5 | A wallet debit is saved together with the ticket or journey it pays for; a credit together with the `Paid` payment it comes from. | RapidPass exit: one save ✅; wallet purchase and recharge: separate saves | 🔴 G10 |
| M6 | Each user has at most one wallet and at most one RapidPass. | code paths create one if missing; no unique index on `UserWallets.UserId` or `RapidPasses.UserId` | 🔴 G10 |

## Payments
| # | Invariant | Enforcement | Status |
| --- | --- | --- | --- |
| P1 | A payment is `Paid` only after AamarPay validation returns `Successful` with an amount equal to the stored `Payment.Amount`. | `PaymentController.Success` + `PaymentService.FinalizePaymentAsync` | 🟡 |
| P2 | Finalizing is idempotent per `MerTxnId`: no second ticket or credit. | early return when already `Paid`; unique index on `MerTxnId`; but two concurrent callbacks can both pass the check | 🔴 G10 |
| P3 | A `Paid` payment never changes status. | `UpdatePaymentStatusAsync` skips `Paid` | 🟡 |
| P4 | AamarPay credentials never appear in logs, responses or exception messages. | review only | 🟡 |

## Tickets, RapidPass, scanning
| # | Invariant | Enforcement | Status |
| --- | --- | --- | --- |
| T1 | A single-journey ticket is accepted at most once for entry and once for exit; `Used` and `Expired` are terminal. | `ScannerService` status checks | 🟡 |
| T2 | Entry and exit are accepted only at stations between the ticket's from and to station. | `ScannerService` station-order checks | 🟡 |
| T3 | A RapidPass exit is charged only with a recorded entry, and charges the fare between entry and exit stations. | Redis entry check + `GetFare` | 🟡 |
| T4 | A QR payload carries only id, expiry and type, encrypted; a forged or altered QR is rejected. | AES encryption; no integrity check (MAC), fixed IV | 🔴 G12 |
| T5 | Ticket expiry is compared in one time base. | creation uses local time, the list endpoint compares with UTC | 🔴 G8 |

## Identity and ownership
| # | Invariant | Enforcement | Status |
| --- | --- | --- | --- |
| I1 | The caller is taken from the token, never from request data. | controllers read `User`; `PaymentController` checks body e-mail = token e-mail | 🟡 |
| I2 | A user reaches only their own tickets, payments, wallet, RapidPass and journeys; another user's → 404. | wallet, ticket list, RapidPass: scoped by token ✅ in code; ticket PDF and payment status: not scoped | 🔴 G5 |
| I3 | Station and settings changes require the `Admin` role. | settings ✅ (`AdminController`); station create/update/delete anonymous | 🔴 G4 |
| I4 | Passwords are stored only as BCrypt hashes; refresh tokens only as SHA-256 hashes. | `AccountService`, `ProfileService` | 🟡 |
| I5 | E-mail and phone number identify at most one user. | checked in `RegisterNewUserAsync`; no unique index | 🔴 G10 |

## Tests
| # | Invariant | Enforcement | Status |
| --- | --- | --- | --- |
| X1 | Tests never run against a database they did not create. | `scripts/ci.sh` guard (`SMARTMETRO_TEST_DB`, name contains `test`) | 🟡 G3 |
