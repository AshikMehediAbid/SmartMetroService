---
name: run-app
kind: playbook
description: >-
  Run, stop, or drive the SmartMetroService API locally and verify
  changes in the real running app. Use when asked to run the app, get a
  token, call an endpoint, or see a feature working end-to-end.
---

# Run SmartMetroService locally

Commands run from the repo root (or the worktree root), in bash.

## Components and ports

| Component | How | Port |
|---|---|---|
| SQL Server | your own instance or LocalDB (`(localdb)\MSSQLLocalDB` on Windows) | 1433 / LocalDB pipe |
| Redis | `docker run -d --name smartmetro-redis -p 6379:6379 redis:7` or any local Redis | 6379 |
| API (`Development`) | `dotnet run --project SmartMetroService.Api` | http 5218, https 7246 |

Redis is needed only for RapidPass scans. SMTP is needed for OTP
e-mails (registration verification, password recovery); AamarPay
sandbox credentials for online payments; Keycloak only for Keycloak
tokens.

## Configuration

Real values go in user secrets (the Api project has a `UserSecretsId`),
never in `appsettings*.json`:

```bash
cd SmartMetroService.Api
dotnet user-secrets list                     # what is set (shows values: do not paste them anywhere)
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "<connection string>"
dotnet user-secrets set "Jwt:Key" "<at least 32 random characters>"
cd ..
```

The keys the app reads are in `docs/context/architecture.md`,
*Configuration keys*. An environment variable
(`ConnectionStrings__DefaultConnection`, …) overrides user secrets for
one run.

**Which database.** Running the API against your dev database is fine.
Running **tests** or **`dotnet ef database update` for a check** against
it is not: use a throwaway database (`AGENTS.md` §2a). For a clean dev
database:

```bash
dotnet ef database update --project SmartMetroService.Storage --startup-project SmartMetroService.Api
```

The fare calculation needs one `Settings` row and stations with
distances; create them through the admin and station endpoints (an
`Admin` token is needed for settings).

## Start / stop

```bash
# Start (foreground; run it in the background in agent sessions)
dotnet run --project SmartMetroService.Api --launch-profile http

# Stop: Ctrl+C, or kill only the dotnet process you started
```

Only one API per port: two worktrees cannot both bind 5218. Port already
in use → find what holds it before killing anything you did not start.

## Getting a token

```bash
BASE=http://localhost:5218
curl -s -X POST $BASE/api/account/register -H 'Content-Type: application/json' \
  -d '{"name":"Test User","email":"test1@example.com","phoneNumber":"+8801700000001","password":"StrongPass123!","confirmPassword":"StrongPass123!"}'
curl -s -c cookies.txt -X POST $BASE/api/account/login -H 'Content-Type: application/json' \
  -d '{"phoneNumber":"+8801700000001","passWord":"StrongPass123!"}'
TOKEN='<data.accessToken from the login response>'
```

An unverified user gets **no token**: `data.accessToken` holds the
user's e-mail, an OTP is e-mailed (SMTP must be configured), and there
is no refresh cookie. Verify with `POST $BASE/api/account/Verify-otp`
(`{"email":…,"otp":…,"type":"EMAIL_VERIFICATION"}`), or set
`IsEmailVerified` on your own test user in your dev database, then log
in again. The access token lives **3 minutes**; refresh it with
`curl -s -b cookies.txt -c cookies.txt -X POST $BASE/api/account/token`.
An `Admin` token needs a user whose `UserRole` is `Admin` (set it in
your dev database, or log in with a Keycloak user that has the role).

For an ownership check you need **two users**: register a second one and
call the first user's resource with the second user's token.

## Calling the app

```bash
curl -s "$BASE/api/station/1"                                    # stations, ascending
curl -s "$BASE/api/station/fare?fromStationId=1&toStationId=5"
curl -s "$BASE/api/wallet/balance"        -H "Authorization: Bearer $TOKEN"
curl -s "$BASE/api/tickets?ticketStatus=Fresh" -H "Authorization: Bearer $TOKEN"
curl -s "$BASE/api/tickets/rapidpass"     -H "Authorization: Bearer $TOKEN"
curl -s "$BASE/openapi/v1.json"                                  # Development only
```

`SmartMetroService.Api/SmartMetroService.Api.http` has more requests for
editors that run `.http` files.

## Verifying a change end-to-end

1. Start the API (and Redis if RapidPass is involved); wait until it
   logs that it is listening.
2. Register or reuse your own test users; get tokens; exercise the
   changed endpoint.
3. For any ownership claim, prove it in a test with two users
   (`AGENTS.md` §6); a manual curl is evidence, not a regression test.
4. Payments: only against the AamarPay **sandbox**; never with real
   credentials or real money.
5. Capture evidence (status and response body, with tokens and personal
   data removed) and report it.
6. Stop the API if you started it. Leave Redis and the database running
   unless you started them in this session.
