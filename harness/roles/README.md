# Specialist roles

Each file here is a specialist persona used for day-to-day feature
work. In any tool, read the file and act as it, or hand its body to a
subagent or a fresh chat as the system prompt. `AGENTS.md` §5 says
which ones a change needs.

## 1. The role matrix

One-line "use this when…" for each role. Full descriptions live in the
role file itself.

| Role | Use when |
|---|---|
| [`security-reviewer`](./security-reviewer.md) | Any code change — this is the default reviewer. Focus: auth, ownership, payments, injection, secrets. |
| [`domain-invariant-reviewer`](./domain-invariant-reviewer.md) | Any change to an endpoint, an entity or migration, or fare, wallet, payment, ticket, RapidPass or scanner logic. Checks ownership, fares and money, atomicity, idempotency and status rules against the repo's invariants. |
| [`test-coverage-reviewer`](./test-coverage-reviewer.md) | Pre-PR. Verifies new features have unit/integration tests at the right level; for bug fixes, ensures a regression test was added. |
| [`architecture-guardian`](./architecture-guardian.md) | A new manager, repository or controller, DI or auth wiring, or a change that bends the layer rules. |
| [`data-access-reviewer`](./data-access-reviewer.md) | Queries, DbContext mapping, migrations, or raw SQL land. |
| [`final-reviewer`](./final-reviewer.md) | Last step of every implementation and every PR review, after the specialist reviewers. Verifies their findings and gives `APPROVED` / `CHANGES REQUIRED`. |
| [`spec-reviewer`](./spec-reviewer.md) | After every merge (spots doc drift in README, ADRs, contracts, AGENTS.md, golden principles, context docs). |
| [`quick-fix`](./quick-fix.md) | One- or two-line fixes: typos, broken links, stale log messages, unused usings. Avoid the heavy orchestrator pipeline. |
| [`feature-planner`](./feature-planner.md) | Before implementing a non-trivial feature — produces a structured plan with file list + test strategy. |
| [`implementer`](./implementer.md) | Autonomous feature delivery from a plan (runs in a worktree). |
| [`orchestrator`](./orchestrator.md) | End-to-end multi-step feature delivery (plan + implement + review + PR). |
| [`penetration-tester`](./penetration-tester.md) | On-demand whitehat source review plus read-only probes of a local or explicitly named non-production stack. |

## 2. Pipeline patterns

Every pattern follows the tier rule in
[`../README.md`](../README.md#model-tiers-and-the-delivery-pipeline):
`fast`-tier `implementer` agents write the code, in parallel by plan
slice; `standard`-tier specialist reviewers review it, in parallel;
one `high`-tier `final-reviewer` verifies their findings and approves.

**One-line typo or comment fix**: skip the orchestrator — use
`quick-fix` (`fast`) + `security-reviewer` (`standard`) +
`final-reviewer` (`high`). Heavy pipelines are wasted on trivial
changes.

**New feature (non-trivial)**: `feature-planner` → `implementer`
agents, one per plan slice, in parallel → `test-coverage-reviewer`
(pre-PR) + `security-reviewer` + `domain-invariant-reviewer` + the
other relevant reviewers, in parallel → `final-reviewer` → PR on
`APPROVED`.

**Bug fix**: `implementer` agents → `test-coverage-reviewer` (pre-PR)
to confirm a regression test was added that would fail against the
buggy code, plus the other relevant reviewers, in parallel →
`final-reviewer` → PR on `APPROVED`.

**Post-merge**: `spec-reviewer` sweeps `README.md`, the ADRs,
`AGENTS.md`, `golden-principles.md`, the repo's context docs and the
client contracts in `docs/contracts/`
for drift. Runs once per merge, not periodically.

**Review an open pull request**: [`review-pr`](../workflows/review-pr.md)
runs one `high`-tier reviewer that walks each applicable role's
checklist against the diff, checks it against the linked GitHub issue, and
gives the verdict. Specialist agents and `final-reviewer` run only when
the user asks for them. It reports in the conversation. It does not post
on the pull request.

**PR comments**: after every push to a PR branch, run the
[`check-pr-comments`](../workflows/check-pr-comments.md) workflow once.
It lists the unresolved review threads and the check status, and waits
only for running checks. Nothing posts review comments automatically,
so it doesn't poll. Run it again when the user asks. Replies to and
resolves addressed threads via
[`respond-to-pr-comments`](../workflows/respond-to-pr-comments.md).

## 3. If your tool has no subagents

Only some tools can spawn a separate agent mid-task. Where a pipeline
says "spawn `security-reviewer`", the fallback is:

- **Run the reviewer in a fresh chat/session**, pointed at the diff
  (`git diff origin/master...HEAD`), and give it
  `harness/roles/<name>.md` to read and act as.
- Don't re-use the session that wrote the code. `golden-principles.md`
  §12 (*evaluator independence*) is about the reviewer not carrying the
  author's assumptions — a fresh session satisfies that just as well as
  a subagent does.
- `orchestrator` is the one role that genuinely needs delegation. On a
  single-session tool, read it as a checklist you drive yourself rather
  than an agent you launch.

## 4. Overlap with tool-native review features

Several tools ship their own review helpers. Rule of thumb:

- The roles in **this** folder are the workflow-integrated defaults —
  `AGENTS.md` §4/§5 refers to them by name, and PR descriptions record
  which of them ran.
- Tool-native reviewers are complementary, ad-hoc extras — use them for
  one-off deep passes, not as a replacement for the pre-PR gate.

