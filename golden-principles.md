# Golden Principles

The rules that never bend. If you're unsure whether something is in scope, check here first. Detailed rules:
`AGENTS.md` §8. Each rule names what enforces it today. Where nothing mechanical does yet, it says so and names the
known gap (`docs/context/known-gaps.md`): the rule still holds, and closing the gap is tracked work, not optional.

## 1. Fares and money are computed on the server, exactly

A journey's fare comes from `StationService.GetFare` (station order, `StationDistances`, `Settings.UnitFare` and
`MinimumFare`); a client-supplied fare or amount is never trusted for a journey. The only client-chosen amount is a
wallet recharge, which must be greater than zero and is still confirmed by AamarPay's validation. New money is
`decimal` (`decimal(18,2)` in SQL Server) in BDT; never `float` or `double`.

**Enforcement**: `domain-invariant-reviewer` on every fare, wallet or payment change. No test yet (fare calculation
tests are a follow-up). Today's `int` fares and `double` wallet balance: known gap G6.

## 2. Wallet balance changes are atomic and never negative

A debit checks the balance and debits in the same save as the ticket or journey it pays for; a credit is saved with
the payment it comes from. Two concurrent requests can never both spend the same balance, and a retried callback can
never credit twice.

**Enforcement**: `domain-invariant-reviewer`, `data-access-reviewer`; the double-submit test `AGENTS.md` §6 requires
for every money path. Today's split saves: known gap G10.

## 3. Only a verified gateway validation marks a payment paid

A payment becomes `Paid` only after `AamarPayService.ValidatePaymentAsync` returns `Successful` with an amount equal to
the stored `Payment.Amount`. The browser redirect or callback alone never does. Finalizing is idempotent per
`MerTxnId`, and a `Paid` payment never changes status again.

**Enforcement**: the checks in `PaymentService.FinalizePaymentAsync` (status, amount, `MerTxnId` match) and the unique
index on `Payments.MerTxnId`; `domain-invariant-reviewer` and `security-reviewer` on every payment change. No test yet.

## 4. A ticket is never accepted twice

Single-journey tickets move `Fresh` → `InUse` → `Used`, or to `Expired`; `Used` and `Expired` are terminal
(`docs/context/state-machines.md`). A RapidPass exit without a recorded entry is rejected. The QR payload is encrypted
and carries only the ticket id, expiry and type.

**Enforcement**: the status checks in `ScannerService`; `domain-invariant-reviewer`. No test yet.

## 5. Identity comes from the token; users reach only their own resources

The caller is whoever the validated token says (e-mail claim, `sub`), never a value in the request. A user's wallet,
tickets, RapidPass, payments, journeys and profile are reachable only by that user; another user's resource answers
404, like a missing one. Every action is `[Authorize]` unless `AGENTS.md` §8 lists it as deliberately anonymous, and
admin operations require the `Admin` role.

**Enforcement**: `[Authorize]` attributes per action (there is no fallback policy); `security-reviewer` on every
change; the ownership tests `AGENTS.md` §6 requires for new endpoints. Anonymous or unowned routes today: known gaps
G4 and G5.

## 6. Never push to `master`

Every change goes through a pull request on a fresh branch, from a worktree, linked to a GitHub issue. Never commit
directly to `master` or force-push over a merged PR.

**Enforcement**: the workflow in `AGENTS.md` §0 and §4; the pre-push hook. No branch protection rule on `master` yet —
adding one (require a pull request and the `ci` check) is the server-side enforcement.

## 7. Schema changes are generated EF Core migrations, never edited

Every schema change is a migration created with `dotnet ef migrations add`, with its Designer file and the updated
model snapshot. A migration on `master` is never edited, renamed or deleted; a mistake is fixed by a new migration.

**Enforcement**: `harness/workflows/ensure-migrations.md`; `data-access-reviewer` on every migration. Two hand-written
migrations EF does not see: known gap G9.

## 8. No secrets or personal data where they don't belong

No secret (connection string, JWT key, encryption key, AamarPay store id or signature key, SMTP password, admin
password) in a tracked file; they live in user secrets or environment variables. Logs, exception messages, API errors
and QR payloads carry no passwords, OTPs, tokens or credentials. Refresh tokens are stored only as SHA-256 hashes and
passwords only as BCrypt hashes.

**Enforcement**: `security-reviewer` checks every new config key, logging call and error path. Committed
non-placeholder values today: known gap G1.

## 9. Tests never touch a database they did not create

The test host loads Development user secrets, and `AuthFlowTests` deletes the database it connects to. Tests run only
through `scripts/ci.sh` (or `make test`), which refuses to run without `SMARTMETRO_TEST_DB` and points
`ConnectionStrings__DefaultConnection` at it. Nobody runs a bare `dotnet test` in this repo.

**Enforcement**: the guard in `scripts/ci.sh`; the pre-push hook and `make test` call it. Isolating the fixture itself
(so a bare `dotnet test` is also safe): known gap G3.

## 10. Conventional Commits

Commit messages follow [Conventional Commits](https://www.conventionalcommits.org/) (`feat(wallet): …`,
`fix(scanner): …`, `docs(harness): …`, `chore: …`), with `Refs #<n>` in the body. The PR body says `Closes #<n>`.

**Enforcement**: PR titles are reviewed; no lint yet.

## 11. Layer boundaries hold

Domain references nothing; Application references Domain only (no EF Core, ASP.NET Core or Storage); Storage
implements Application's repository interfaces; Api is the only composition root. Controllers call managers, never
repositories or the DbContext, and never bind or return domain entities.

**Enforcement**: the project references in the `.csproj` files (Application cannot reference Storage without a
cycle); `architecture-guardian` for any new manager, repository, controller or DI change. Today's exceptions: known
gap G11.

## 12. Evaluator independence

Whoever wrote the code MUST NOT be the one who reviews it.

Reviewer roles (`security-reviewer`, `domain-invariant-reviewer`, `test-coverage-reviewer`, `data-access-reviewer`,
`architecture-guardian`, `final-reviewer`) are invoked by the orchestrator or by the user, never by the implementer
itself. Any of these count: a reviewer subagent; a **fresh session** that reads `harness/roles/<name>.md` and reviews
`git diff origin/master...HEAD`; a different teammate or their agent. Continuing in the session that wrote the code
and asking it to review its own work does not count. The `final-reviewer` runs as a fresh agent, never in the session
that coordinated the implementation or its fixes.

**Why**: self-evaluation biases toward declaring success ("generator ≠ evaluator").

**Enforcement**: the PR template's "Agent Reviews" section records which roles ran; PRs that self-report no review are
flagged at review time. `quick-fix` is the one exception (typos, broken links). A `quick-fix` change made in response
to PR review comments (`harness/workflows/respond-to-pr-comments.md`) is still reviewed by `security-reviewer` and
`final-reviewer`.

## 13. No new warnings

The build has a baseline of about 75 warnings. New code adds none, and a warning on a line you change is yours to fix.
Suppressing a warning (`#pragma`, `NoWarn`, an `.editorconfig` severity) is a design call: stop and ask first. Turning
on `TreatWarningsAsErrors` once the baseline is zero is the goal.

**Enforcement**: reviewers compare the warning count of a full rebuild before and after the change
(`dotnet build SmartMetroService.slnx --no-incremental`; an incremental build only counts the projects it recompiled).

## 14. Tool output is evidence, not assumption

Empty, errored, truncated, or garbled tool output means "unknown" — never confirmation of what you expected.

- **Tests**: a test that did not run is not a passing test. `dotnet test` at the repo root runs nothing (the test
  project is not in the solution); report the counts from `scripts/ci.sh`.
- **CI**: diagnose only from the real failing log of the completed run at the current head
  (`gh run view <id> --repo AshikMehediAbid/SmartMetroService --log-failed`). Never push a "fix" for a failure you
  have not observed.
- **Files / commands**: if a read is empty or implausible, re-read or cross-check before building on it.
