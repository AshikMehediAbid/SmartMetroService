# Open questions

Product decisions the code does not answer. Ask the product owner; record the answer here (and in an ADR when it was
contested), then close the row. Quote a question by its id (`Q3`) in issues, grilling sessions and reviews.
Priority: P1 blocks safe operation or the next feature, P2 blocks a later feature, P3 can wait.

| # | Pri | Question | Why it matters | Today |
| --- | --- | --- | --- | --- |
| Q1 | P1 | **How does a gate scanner authenticate?** A device credential (API key, client-credentials token from Keycloak), a station-staff login, or network isolation? | `POST api/scanner/qr-data` moves tickets and charges wallets; it is anonymous today (gap G4). | Anonymous. |
| Q2 | P2 | **Which identity provider is the long-term one?** Keep both our own JWT and Keycloak, or move fully to Keycloak? | Two token issuers, two user-creation paths, and roles from two places; every auth change has to handle both. | Both, chosen per token by issuer. |
| Q3 | P1 | **Fare rounding and unit.** Should fares be whole taka rounded to the nearest 10, and how should a midpoint round? `Math.Round(fare / 10) * 10` uses banker's rounding (25 → 20, 35 → 40) although the variable is named `roundupTo10`. Up, nearest, or half-up? | Passengers are charged by this number; changing it changes prices. | Banker's rounding to the nearest 10, whole taka. |
| Q4 | P2 | **What happens to an expired ticket inside the gates?** An `InUse` ticket past its 2-day expiry is rejected at the exit and stays `InUse`. | Passengers can be stuck behind the exit gate; status never reaches a terminal state. | Rejected, status unchanged. |
| Q5 | P2 | **What if a RapidPass entry is never closed?** The entry lives one day in Redis; with no exit there is no journey and no charge. Should it charge a maximum fare, record a `TimeOut` journey, or block the next entry? | Free rides, and `JourneyStatus.InProgress`/`TimeOut` are unused. | No charge, no record. |
| Q6 | P1 | **Should login issue a refresh token before the e-mail is verified?** `AuthFlowTests` expects one; the code issues none for an unverified user. | Decides gap G2: whether the test or the code is wrong. | No refresh token until verified. |
| Q7 | P2 | **What does buying a RapidPass online buy?** `PaymentFor.RapidPass` finalizes by creating a `Ticket` row of type `RapidPass` with the paid amount as its fare, but the RapidPass itself is a separate entity charged per journey from the wallet. | The payment may create a ticket nothing uses. | Creates a `RapidPass`-type ticket row. |
| Q8 | P3 | **Refunds and cancellations.** Can a passenger cancel an unused ticket or recover a failed wallet debit? | No refund path exists for wallet or AamarPay payments. | None. |
| Q9 | P3 | **One line or several?** Fares and route checks assume one straight line ordered by `StationOrder`. | A second line or a branch breaks fare calculation and route checks. | One line. |
| Q10 | P2 | **Should the scanner check a RapidPass QR's expiry?** The QR expires after a day and can be refreshed, but the scanner checks only the balance and the Redis entry. | An old screenshot of a RapidPass QR keeps working. | Not checked. |
