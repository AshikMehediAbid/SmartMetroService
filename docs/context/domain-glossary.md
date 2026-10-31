# Domain glossary

Terms as this codebase uses them. When a request uses a word differently, say so and settle which one is meant
before building (`harness/playbooks/grilling.md`).

| Term | Meaning here | Code |
| --- | --- | --- |
| **User** | A passenger or admin account. Either registered with phone + password (our JWT) or mirrored from Keycloak (`KeycloakUserId` set). Identified by e-mail in most manager calls. | `User`, `UserRole` (`User`, `Admin`, `SuperAdmin`) |
| **Admin** | A user whose token carries the `Admin` role. Manages stations and fare settings. `SuperAdmin` exists in the enum but nothing checks it. | `[Authorize(Roles = "Admin")]` |
| **Station** | A stop on the line, with name, location, coordinates, `StationOrder` and `IsActive`. | `Station` (key `StationId`, int) |
| **Station order** | The position of a station on the line, 1..n. Fares, route checks and the station list all walk this order; the line is treated as a single straight line. | `Station.StationOrder` |
| **Station distance** | The distance between two stations, stored per consecutive pair. A journey's distance is the sum of the pairs between its two stations. | `StationDistance` |
| **Settings** | The one row of fare settings: `UnitFare` (taka per distance unit) and `MinimumFare`. | `Settings`, `api/admin/settings` |
| **Fare** | What a journey between two stations costs, in whole taka: `max(UnitFare × distance, MinimumFare)` rounded to the nearest 10. Always computed on the server. | `StationService.GetFare`, `StationFareDto` |
| **Wallet** | A user's prepaid balance in BDT ("account balance"). Paid into by online recharge; paid out of by wallet ticket purchases and RapidPass journeys. | `UserWallet` |
| **Ticket** (single-journey ticket) | A one-time QR ticket for a fixed from/to pair and fare, valid for 2 days. | `Ticket`, `TicketType.SingleJourney` |
| **RapidPass** | A user's reusable QR (one per user, 1-day QR expiry, refreshable). Each trip is charged from the wallet at exit. Not a ticket row: a separate entity. | `RapidPass`, `TicketType.RapidPass` |
| **Journey** | A completed (or in-progress) trip recorded for a RapidPass: from, to, fare, start and end time. | `Journey`, `JourneyStatus` |
| **QR payload** | The encrypted JSON inside a QR: ticket or RapidPass id, expiry and type. | `QrPayload`, `EncryptionService`, `QrCodeService` |
| **Gate / scan** | A scanner at a station's entry or exit gate posting the QR text, station id and `Gate` (`Entry` / `Exit`). | `QrCodeRequest`, `ScannerService` |
| **Payment** | One AamarPay transaction we started, identified by our `MerTxnId` (the `tran_id` we send). `PaymentFor` says what it buys. | `Payment`, `PaymentFor` (`SingleJourney`, `RapidPass`, `WalletRecharge`) |
| **Payment method** | `Online` (AamarPay) or `AccountBalance` (wallet). | `PaymentMethod` |
| **MerTxnId / PgTxnId / BankTrxId** | Our transaction id / AamarPay's / the bank's. Only `MerTxnId` is ours and unique. | `Payment` |
| **Callback** | AamarPay posting the browser back to `success`, `fail` or `cancel` with `mer_txnid`. Not proof of payment by itself. | `PaymentController` |
| **Validation** | Our server-to-server call to AamarPay's validation API that confirms a transaction's status and amount. The only proof of payment. | `AamarPayService.ValidatePaymentAsync` |
| **OTP** | A 6-digit code e-mailed for e-mail verification (10 min) or password recovery (30 min). | `UserOTP`, `OtpType`, `OTPService` |
| **Refresh token** | Long-lived (30 days) random token in an HttpOnly cookie, stored hashed, rotated on use. | `Token` (`RefreshTokens` table) |
