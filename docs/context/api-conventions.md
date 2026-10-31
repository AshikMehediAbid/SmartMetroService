# API conventions

How the HTTP surface works today, and the rules for new or changed routes. Shapes come from the controllers and DTOs
(and `/openapi/v1.json` in Development); this file lists what holds across routes.

## Rules for new and changed routes
- **Prefix**: `api/<area>` in lower case (`api/wallet`, `api/tickets`). Kebab-case for multi-word segments
  (`change-password`). Existing exceptions (`Verify-otp`, `verifyemail`) stay as they are: renaming a route breaks the
  Angular client.
- **Auth**: `[Authorize]` by default; anonymous only for the routes `AGENTS.md` §8 lists. Admin operations use
  `[Authorize(Roles = "Admin")]`.
- **Identity**: the caller comes from `User` (e-mail claim; `User.FindFirstValue(ClaimTypes.Email)`). Never add a
  route parameter or body field that names the caller.
- **Not yours = not found**: another user's resource returns 404 with the same body as a missing one.
- **Envelope**: return `ApiResponse<T>` (`{ "data": …, "message": "…" }`) from new actions, success and error alike.
  Some existing actions return bare objects or strings; do not copy them.
- **Errors**: throw an `ApiException` subtype in the manager; map it in the controller (or the global handler once it
  exists) to its `StatusCode` with `Message`. Unexpected errors → 500 with a generic message. Never put
  `ex.Message`, stack traces or SQL in a response (gap G7).
- **Validation**: data annotations on the DTO, checked with `ModelState.IsValid` → 400 `"One or more fields are
  invalid."` (the existing pattern).
- **JSON**: System.Text.Json web defaults (camelCase); enums serialize as **strings**
  (`JsonStringEnumConverter` in `DependencyConfig`).
- **Additive changes**: adding a response field is safe; removing or renaming a field or route breaks the Angular
  client and needs a contract update (`docs/contracts/`) in the same PR.

## Status codes in use
| Code | When |
| --- | --- |
| 200 | success (also used for creates today; `201` is fine for new create routes) |
| 302 | AamarPay callbacks redirect the browser to `BaseFeUrl` |
| 400 | invalid model, `ValidationException`, `OtpException`, many expected failures today |
| 401 | missing/invalid token, `UnauthorizedException`, missing refresh cookie |
| 403 | authenticated without the required role (framework) |
| 404 | `NotFoundException`, resource not found or not yours |
| 409 | `AlreadyExistsException`, station still referenced |
| 500 | unexpected error |

## Routes today
| Route | Auth | Notes |
| --- | --- | --- |
| `POST api/account/register` | anonymous | |
| `POST api/account/login` | anonymous | phone + password; returns access token, sets `refreshToken` cookie when verified |
| `GET api/account/verifyemail?email&otp` | anonymous | does not check the code (G13) |
| `POST api/account/change-password` | `[Authorize]` | |
| `GET api/account/recover-password?email` | anonymous | e-mails a temporary password |
| `POST api/account/token` | anonymous (cookie) | rotates the refresh token |
| `POST api/account/logout` | anonymous (cookie or bearer) | |
| `GET api/account/user-profile?email` | anonymous | reveals whether an e-mail exists (G4) |
| `POST api/account/Verify-otp`, `POST api/account/resend-otp` | anonymous | |
| `POST api/account/keycloak-user` | `[Authorize]` | roles from the body (G5) |
| `GET/PUT api/admin/settings` | `Admin` | binds the `Settings` entity (G11) |
| `POST api/payment` | `[Authorize]` | body e-mail must match the token |
| `POST api/payment/success|fail|cancel` | anonymous (AamarPay) | form field `mer_txnid`; redirect |
| `GET api/payment/status/{merTxnId}` | `[Authorize]` | not scoped to the caller (G5) |
| `POST api/scanner/qr-data` | anonymous | Q1, G4 |
| `POST api/station/create`, `PUT/DELETE api/station/{stationId}` | anonymous | should be `Admin` (G4) |
| `GET api/station/{orderByHints}` | anonymous | 1 = ascending, 0 = descending |
| `GET api/station/fare?fromStationId&toStationId` | anonymous | omit `toStationId` for fares to every station |
| `GET api/tickets?ticketStatus`, `GET/PUT api/tickets/rapidpass` | `[Authorize]` | scoped to the caller |
| `GET api/tickets/download-ticket/{id}` | `[Authorize]` | not scoped to the caller (G5) |
| `GET api/wallet/balance` | `[Authorize]` | creates a zero wallet when missing |

## Cookies and CORS
- `refreshToken`: HttpOnly, Secure, SameSite=None, Path `/`, 30 days. The Angular app must send credentials.
- CORS policy `AllowNgFrontend`: origin `http://localhost:4200` only, any header and method, credentials allowed. A new
  origin is a configuration change, never `AllowAnyOrigin` with credentials.
