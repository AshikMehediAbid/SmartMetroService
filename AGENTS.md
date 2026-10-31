# AGENTS.md — SmartMetroService

Rules for AI coding agents and developers. Read this first; it is short on purpose. Details live in `docs/`.
**The entry point for every coding agent in this repo** — Claude Code (via the one-line `CLAUDE.md`), Cursor, GitHub
Copilot, Codex. Keep rules here, not in a tool-specific file. The hard constraints are in
[`golden-principles.md`](./golden-principles.md).

## 0. Rules that never bend
1. **Never push to `master`.** Every change goes through a PR on a fresh branch.
2. **Always work in a git worktree.** Before editing any file, create a worktree outside this checkout, under the
   `worktrees/` folder next to it, branched off a freshly fetched `origin/master`:
   ```bash
   git fetch origin
   git worktree add --no-track -b <type>/<n>-<slug> ../worktrees/<short-slug> origin/master
   ```
   The primary checkout stays on `master` for browsing and reviewing. Your tool's worktree helper is fine if it
   branches off `origin/master`.
3. **Every change starts from a GitHub issue** in `AshikMehediAbid/SmartMetroService`. Issues are referred to as
   `#<n>`. If none exists, run the `create-issue` workflow: draft, get the user's confirmation, create it. Branches
   are `<type>/<n>-<slug>` (`feat`, `fix`, `docs`, `chore`, `refactor`, `test`), commit bodies say `Refs #<n>`, the PR
   body says `Closes #<n>` so the merge closes the issue. Creating an issue needs the user's approval like any other
   GitHub write.
4. **GitHub writes need approval; production and shared data are off-limits.** Read freely. Show each issue create
   or edit, push, PR create/edit, review, comment, thread resolve or label change and get the user's approval before
   it runs. The only stack an agent may run is the local one (§3), and only against a database it created for the
   purpose (§2a, first bullet).
5. **Procedures live in [`harness/`](./harness/README.md).** Roles, workflows and playbooks are read from there by
   path; follow them rather than improvising a process. A harness change ships in a PR like any other change.

## 1. What this repo is
SmartMetroService is the backend of a metro (MRT) ticketing service in Bangladesh: passenger accounts, stations and
distance-based fares, a prepaid wallet, single-journey QR tickets, a reusable RapidPass QR charged per journey, gate
scanning, and online payment through AamarPay in BDT. The client is an Angular app (CORS origin
`http://localhost:4200`); it lives outside this repo.

```text
Angular app  ──Bearer JWT───────────┐
Gate scanner ──QR payload───────────┼──> Api (controllers) ──> Application (managers) ──> Storage (EF Core) ──> SQL Server
AamarPay     ──success/fail/cancel──┘                                  ├──> Redis (RapidPass entry state)
                                                                       └──> AamarPay API, SMTP (OTP e-mail)
```

There are no product source documents in the repo. For **existing** behaviour the code is the truth. For **new**
behaviour the issue and the user's stated intent are the truth; decisions go in `docs/adr/`, unknowns in
`docs/context/open-questions.md`. Never invent metro or AamarPay behaviour: if neither the code nor the user says
it, raise it as an open question.

### Stack
.NET 10 (SDK 10.0.x; there is no `global.json`), ASP.NET Core controllers, EF Core 10 + SQL Server, Redis through
`IDistributedCache`, AutoMapper, BCrypt, QRCoder, QuestPDF (Community licence), JWT bearer auth with two schemes (our
own HS256 tokens and Keycloak). Tests: xUnit 2 with `WebApplicationFactory<Program>`.

### Layout
```text
SmartMetroService.Domain/        entities + enums; no project or package dependencies
SmartMetroService.Application/   managers (Managers/), their interfaces (Interfaces/IManagers), repository interfaces
                                 (Interfaces/IRepositories), DTOs (Models/), AutoMapper profile (Mapping/), ApiException types
SmartMetroService.Storage/       MyApplicationDbContext (Sql/), repositories + UnitOfWork (Repositories/), EF migrations
SmartMetroService.Api/           controllers, DI and auth setup (Configurations/), ApiResponse<T>, Program.cs, .http file
SmartMetroService.Tests/         xUnit tests through WebApplicationFactory — NOT listed in SmartMetroService.slnx
```
Dependencies point Api → Application + Storage, Storage → Application → Domain. Storage implements the repository
interfaces Application declares.

## 2. Where to look first
| Question | File |
| --- | --- |
| What must I never violate? | [`golden-principles.md`](./golden-principles.md), then §8 below |
| How do the layers, auth, payment and scan flows fit together? | [`docs/context/architecture.md`](./docs/context/architecture.md) |
| What do station, fare, RapidPass, journey… mean here? | [`docs/context/domain-glossary.md`](./docs/context/domain-glossary.md) |
| Which rules must the domain keep, and what enforces them? | [`docs/context/invariants.md`](./docs/context/invariants.md) |
| Ticket, journey, payment and OTP states | [`docs/context/state-machines.md`](./docs/context/state-machines.md) |
| Routes, auth per route, response envelope, errors | [`docs/context/api-conventions.md`](./docs/context/api-conventions.md) |
| Tables and keys | [`docs/context/data-model.md`](./docs/context/data-model.md) |
| Where does today's code break the rules below? | [`docs/context/known-gaps.md`](./docs/context/known-gaps.md) |
| Undecided product questions | [`docs/context/open-questions.md`](./docs/context/open-questions.md) |
| Why did we decide X? | [`docs/adr/`](./docs/adr/README.md) (new ones from `0000-template.md`) |
| What must the Angular client know about an endpoint? | [`docs/contracts/`](./docs/contracts/README.md) |
| Which specialist role should I use? | §5 below; prompts in `harness/roles/` |
| Which procedures exist? | `harness/workflows/`: `create-issue`, `issue`, `review-pr`, `open-pr`, `check-pr-comments`, `respond-to-pr-comments`, `ensure-migrations` |
| How do I settle a design, or check a PR's creator understands it? | `harness/playbooks/grilling.md`, `harness/playbooks/understanding-check.md` |
| How do I run and call the app locally? | [`harness/playbooks/run-app.md`](./harness/playbooks/run-app.md) |
| How do I add a migration? | [`harness/workflows/ensure-migrations.md`](./harness/workflows/ensure-migrations.md) |
| How do I set up my tool for the harness? | [`harness/README.md`](./harness/README.md) |

**Using the harness.** To use a role, read its file and act as it, or hand it to a subagent or a fresh chat. To run a
workflow, read its file and follow the steps; `$ARGUMENTS` in the text is the argument you were given.

When sources disagree about *existing* behaviour: live code, then its tests, then `docs/adr/`, then
`docs/contracts/`, then other markdown. A change to a routed HTTP surface the Angular client calls updates its
contract in the same change set; a contested decision gets an ADR in the same PR.

**Rules describe the target; `known-gaps.md` describes today.** Several rules in §8 are not yet true of the existing
code. Never copy a pattern listed in `known-gaps.md` into new code, and do not fix a gap as a side effect of an
unrelated issue: each gap gets its own issue. A reviewer reports a gap only when the diff adds to it or touches it.

### 2a. What the code cannot tell you
- **The test suite deletes a database.** `AuthFlowTests` calls `EnsureDeleted()` then `EnsureCreated()` on whatever
  `ConnectionStrings:DefaultConnection` resolves to, and the test host runs as Development, so it loads your user
  secrets: a bare `dotnet test` wipes your local dev database. Run tests only through `scripts/ci.sh` (or
  `make test`), which requires `SMARTMETRO_TEST_DB` and points the connection string at it. Never set
  `SMARTMETRO_TEST_DB` to a database that holds data you need.
- **The test project is not in the solution.** `SmartMetroService.slnx` lists four projects; `dotnet test` at the
  root finds no tests and reports success. Build and test `SmartMetroService.Tests` by path (`scripts/ci.sh` does).
- **The suite is red on `master`.** `AuthFlowTests` fails: login sets no refresh-token cookie for an unverified
  user (known gap G2). A red result for that test alone is the baseline, not your regression; say so in the report.
- **Secrets.** Real values live in user secrets (`UserSecretsId` in the Api csproj) or environment variables
  (`ConnectionStrings__DefaultConnection`, `Jwt__Key`, `AamarPay__StoreId`, …). `appsettings.json` holds
  placeholders such as `[See User Secrets]`; some committed values are not placeholders (known gap G1). Never add a
  real secret to a tracked file, and never print one in a report.
- **Two token issuers.** The `SmartMetro` policy scheme reads the bearer token's `iss` claim: a Keycloak issuer goes
  to the `Keycloak` scheme (roles from `realm_access.roles`), anything else to `SmartMetroJwt` (our HS256 token,
  3-minute lifetime, refresh token in an HttpOnly cookie). Both put the e-mail in `ClaimTypes.Email` and roles in
  `ClaimTypes.Role`.
- **Redis is required for RapidPass scans.** The entry scan is stored in Redis for one day under the RapidPass id;
  the exit scan reads it. Without Redis the RapidPass flow fails; single-journey tickets do not need it.
- **Hand-written migrations are invisible to EF.** `20260902000000_Add_Payments.cs` and
  `20260916120000_Add_RapidPass_ForeignKey_To_Journey.cs` have no Designer file and no `[Migration]` attribute, so
  `dotnet ef` ignores them (known gap G9). Generate migrations with `dotnet ef migrations add`; never hand-write one.
- **Warnings baseline.** A full rebuild (`dotnet build SmartMetroService.slnx --no-incremental`) currently reports
  about 75 warnings; an incremental build shows fewer because it skips unchanged projects. Do not add new ones; fix a
  warning in a line you change.
- **Time.** Existing code mixes `DateTime.Now` and `DateTime.UtcNow` (known gap G8). New code uses `UtcNow`.

## 3. Daily commands
GitHub repo (issues and PRs): `AshikMehediAbid/SmartMetroService`; default branch `master`. Every `gh` command passes
`--repo AshikMehediAbid/SmartMetroService`.
```bash
dotnet build SmartMetroService.slnx                     # the four app projects
dotnet build SmartMetroService.Tests                    # the test project (not in the .slnx)
dotnet run --project SmartMetroService.Api              # http://localhost:5218 (https://localhost:7246)

# Tests: only against a throwaway database (§2a). LocalDB works on Windows:
export SMARTMETRO_TEST_DB='Server=(localdb)\MSSQLLocalDB;Database=SmartMetro_Test;Trusted_Connection=True;TrustServerCertificate=True'
bash scripts/ci.sh                                      # build both + run tests; same as `make ci`
bash scripts/ci.sh --filter "FullyQualifiedName~AuthFlowTests"   # one class

# EF Core migrations (needs dotnet-ef: dotnet tool install --global dotnet-ef)
dotnet ef migrations add <Name> --project SmartMetroService.Storage --startup-project SmartMetroService.Api
dotnet ef database update --project SmartMetroService.Storage --startup-project SmartMetroService.Api
```
`make ci`, `make test` and `make install-hooks` do the same where `make` is installed.

## 4. Git workflow — always a PR
Issue first and worktree first (§0 rules 3 and 2) apply before you touch a file.
1. Branch `<type>/<n>-<slug>` off a freshly fetched `origin/master`.
2. Commit with [Conventional Commits](https://www.conventionalcommits.org/) (`feat(wallet): …`), `Refs #<n>` in the
   body.
3. **Pre-push gate**, every push including follow-ups:
   - **Test-merge against fresh `master`**: `git fetch -q origin master && git merge-tree --write-tree --name-only
     --no-messages origin/master HEAD` (exit 1 = conflicts, paths listed). On a conflict, merge `origin/master` in,
     rerun the gate, show the user the push, and push the merge commit once they approve. Two branches that both add
     a migration conflict in `MyApplicationDbContextModelSnapshot.cs`: regenerate yours on top of the other.
     GitHub's `mergeable` is not evidence.
   - `scripts/ci.sh` green: both builds succeed with no new warnings, and the tests pass apart from the documented
     baseline failure (§2a).
   - ADRs and contracts go in the same PR as the code they describe.
   - The §5 reviewer roles for this change, in a separate context; `test-coverage-reviewer` always.
   - `.githooks/pre-push` runs the merge check and `scripts/ci.sh`. **Never bypass it** (`git push --no-verify`,
     `PREPUSH_SKIP`, `PREPUSH_SKIP_MERGE_CHECK`) without the user's explicit approval for that push.
4. Before the first push of a new PR, run [`understanding-check`](./harness/playbooks/understanding-check.md): the
   creator answers questions about the diff, and the PR is created only on a pass or on "push anyway", with the result
   line at the top of the body. Then push and open the PR (both need approval, §0 rule 4):
   ```bash
   git push -u origin <branch>
   gh pr create --repo AshikMehediAbid/SmartMetroService --base master --head <branch> \
     --title "<conventional-commit title>" --body-file <body.md>
   ```
   Fill `<body.md>` from `.github/pull_request_template.md` (GitHub applies it only in the web UI). Start it, under the
   understanding-check line, with **In short:** at most two plain, jargon-free sentences on what the PR does and why.
5. After each push run the `check-pr-comments` workflow once (test-merge, threads, checks); it does not poll.
6. After fixing review comments, reply to and resolve each thread, then post one PR summary comment
   (`respond-to-pr-comments`). Never fix a check failure you have not read the log of (`golden-principles.md` §14).
7. After merge, run `spec-reviewer` (README, `docs/context/`, ADRs, this file, contracts).

## 5. Specialist role matrix
Roles: `harness/roles/<name>.md`. Whoever implemented a change MUST NOT review it — use a
subagent, or a fresh session pointed at `git diff origin/master...HEAD` (`golden-principles.md` §12).

Model tiers (`harness/README.md`, *Model tiers and the delivery pipeline*): `fast`-tier `implementer` agents write
the code, in parallel by plan slice; `standard`-tier specialist reviewers review it, in parallel; one `high`-tier
`final-reviewer` verifies their findings and approves. No PR without its `APPROVED`.

| Role | When to use |
| --- | --- |
| `security-reviewer` | Always — every code change |
| `final-reviewer` | Always — last, after the specialist reviewers; its `APPROVED` gates the PR |
| `domain-invariant-reviewer` | Any endpoint, entity or migration, or code that touches fares, wallet, payments, tickets, RapidPass, journeys or scanning. Sources: §8, `docs/context/invariants.md`, `docs/context/state-machines.md` |
| `test-coverage-reviewer` | **Pre-PR, every change** |
| `data-access-reviewer` | Any migration, `MyApplicationDbContext` mapping, repository or query |
| `architecture-guardian` | A new manager, repository or controller, a change across more than one layer's conventions, DI or auth wiring |
| `spec-reviewer` | After every merge |
| `quick-fix` | One- or two-line fixes (never migrations, money, auth, payments or ownership checks) |
| `feature-planner` | Before any non-trivial feature |
| `implementer` | Autonomous delivery from a plan, in a worktree |
| `orchestrator` | Plan → implement → review → PR |
| `penetration-tester` | On-demand; local stack only |

## 6. Testing expectations
- Every behaviour change ships with tests. Pure logic (fare calculation, ticket status transitions, QR payload
  round-trip) → plain xUnit tests with no host. Anything touching EF queries, auth or HTTP shape → a
  `WebApplicationFactory` test against a real SQL Server test database (`SMARTMETRO_TEST_DB`). No EF InMemory: the
  package is referenced but cannot catch query, constraint or transaction bugs.
- Every new endpoint on a user's resource gets ownership tests: another user's id → 404, no token → 401, and for
  admin routes a non-admin token → 403.
- Money paths (wallet debit/credit, payment finalization) get a test for the double-submit case (the same callback
  twice, or two purchases racing the balance).
- Each test creates its own users and data; never depend on seed data, on another test, or on test order.
- External services (AamarPay, SMTP, Keycloak) are replaced at the DI seam in tests; tests never call them for real.

## 7. CI/CD
Local gate: `scripts/ci.sh` (also `make ci`) builds `SmartMetroService.slnx` and `SmartMetroService.Tests`, then runs
the tests against `SMARTMETRO_TEST_DB`. It refuses to run tests without that variable, so a missing database fails the
gate instead of silently passing or wiping your dev data. The pre-push hook (`make install-hooks`, or
`git config core.hooksPath .githooks`) runs the merge check and the gate on every non-docs push.
Server: `.github/workflows/ci.yml` runs `scripts/ci.sh --build-only` on every PR to `master` and every push to
`master`; it does not run tests yet, because CI has no SQL Server and the suite is red (known gaps G2, G3). Keep
`scripts/ci.sh`, the Makefile, the hook and the workflow in step. No deploy pipeline exists.

## 8. Code rules

### Layers (see `docs/context/architecture.md`)
- Domain references nothing. Application references Domain and abstraction packages only: never EF Core, ASP.NET Core
  or Storage. Storage implements Application's repository interfaces. Api is the only composition root
  (`Configurations/DependencyConfig.cs`).
- Controllers are thin: bind a DTO, read identity from `User`, call one manager (`I…Service`), map the result or
  `ApiException` to a response. Controllers never use repositories, `IUnitOfWork` or the DbContext.
- A new manager: interface in `Application/Interfaces/IManagers`, class in `Application/Managers`, registered in
  `DependencyConfig`. A new repository: interface in `Application/Interfaces/IRepositories`, class in
  `Storage/Repositories`, exposed on `IUnitOfWork`, registered in `DependencyConfig`.
- Request and response bodies are DTOs in `Application/Models`; a controller never binds or returns a domain entity.
- Persist through `IUnitOfWork.CompleteAsync()`. Changes that must succeed or fail together are saved by **one**
  `CompleteAsync`, or inside one explicit transaction.

### Identity and ownership (non-negotiable)
- The caller's identity comes **only** from the validated token (`ClaimTypes.Email`, `sub`). Never from a body, query
  or route value. When a body carries the caller's e-mail, it must match the token (`PaymentController.PurchaseTicket`
  is the pattern).
- A user's wallet, tickets, RapidPass, payments, journeys and profile are reachable only by that user. Another user's
  resource returns **404**, the same as a missing one; never 403, never a message that confirms it exists.
- Every action is `[Authorize]` unless it is deliberately anonymous: register, login, token refresh, logout, e-mail
  and OTP verification, password recovery, station list, fare lookup, and the AamarPay callbacks. Admin operations
  (stations, settings) are `[Authorize(Roles = "Admin")]`. Roles come from the token, never from a request body.
- Gate scanning is a privileged operation; how a gate device authenticates is open question Q1. Until it is settled,
  do not widen what an anonymous scan can do.

### Fares, wallet and money
- A journey's fare is computed on the server from station order, `StationDistances` and `Settings`
  (`UnitFare`, `MinimumFare`) through `StationService.GetFare`. A client-supplied amount is never trusted for a
  journey; it is accepted only for a wallet recharge and must be greater than zero.
- Currency is BDT. New money columns are `decimal` with `HasPrecision(18, 2)` set explicitly in `OnModelCreating`
  (`Payment.Amount` gets `decimal(18,2)` only by default, and EF warns about it); never `float` or `double` for money.
  Existing `int` fares (whole taka) and the `double` wallet balance are known gap G6.
- The wallet never goes negative. A debit checks the balance and debits in the same save as the ticket or journey it
  pays for, so two concurrent requests cannot both spend the same balance.

### Payments (AamarPay)
- Only a server-side validation call (`AamarPayService.ValidatePaymentAsync`) that returns `Successful` **and** an
  amount equal to the stored `Payment.Amount` marks a payment `Paid`. The success redirect alone never does.
- Finalizing is idempotent per `MerTxnId`: a second success callback for a `Paid` payment issues no second ticket and
  no second credit. The credit or ticket and the `Paid` status are saved together.
- A `Paid` payment never changes status again. Status strings: `Pending`, `Paid`, `Failed`, `Cancelled`.
- `StoreId` and `SignatureKey` never appear in logs, responses or exception messages.

### Tickets, RapidPass and scanning
- A QR payload is the AES-encrypted JSON of `QrPayload` (ticket id, expiry, type) only. No personal data in a QR.
- Single-journey tickets follow `docs/context/state-machines.md`: `Fresh` → `InUse` (entry scan inside the route) →
  `Used` (exit scan); `Fresh`/`InUse` past `ExpiryTime` → `Expired`. `Used` and `Expired` are terminal; a ticket is
  never accepted twice.
- RapidPass: the entry scan stores the entry station in Redis; the exit scan computes the fare, records a `Journey`
  and debits the wallet in one save.

### Data and migrations
- The schema is EF Core code-first. Migrations live in `SmartMetroService.Storage/Migrations` and are generated with
  `dotnet ef migrations add`, each with its Designer file and the updated model snapshot. **Never edit or delete a
  migration that is on `master`**; add a new one. Mapping goes in `MyApplicationDbContext.OnModelCreating`.
- New code stores instants as UTC (`DateTime.UtcNow`).
- Destructive schema changes (drop, rename, narrowing a type, `NOT NULL` on a populated column) are called out to the
  user before the migration is written.

### Errors
- Expected failures throw an `ApiException` subtype (`NotFoundException` 404, `AlreadyExistsException` 409,
  `ValidationException` 400, `UnauthorizedException` 401, `OtpException` 400); the controller maps it to
  `ApiResponse<T>` with `Message`. Unexpected exceptions return a generic 500 message: never `ex.Message`, stack traces
  or SQL to the client (known gap G7 lists where this is broken today).

### Do not
- Do not run tests or `dotnet ef database update` against a database that was not created for that run.
- Do not trust a client-supplied fare, balance, role, user id or e-mail.
- Do not use `float`/`double` for new money, or `DateTime.Now` in new code.
- Do not hand-write or edit an existing migration.
- Do not let a controller touch a repository or the DbContext.
- Do not commit secrets, connection strings or keys; do not log tokens, OTPs, passwords or AamarPay credentials.
- Do not add infrastructure (a queue, a new store, a new external API) without an ADR.
- Do not copy a pattern listed in `docs/context/known-gaps.md`.

## 9. Key references elsewhere
- `docs/context/` architecture, glossary, invariants, state machines, API conventions, data model, known gaps,
  open questions · `docs/adr/` decisions · `docs/contracts/` client-facing traps per endpoint ·
  `harness/` agent roles, workflows and playbooks, and how to set up each tool for them (`harness/README.md`).
