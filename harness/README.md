# Agent harness

The team's agentic engineering workflow for SmartMetroService, written
once in tool-neutral markdown that whatever coding agent each of us
uses can read — **Claude Code, Cursor, GitHub Copilot, Codex, or the
next thing**. It is adapted from the RestaurantOS harness and kept
inside this repo: a harness change ships in the same PR as the code
that needs it.

The entry point is [`AGENTS.md`](../AGENTS.md) at the repo root. It
holds the rules and points here for procedures.

## What's in the box

```text
harness/
  roles/        specialist personas: reviewers, planner, implementer, orchestrator
  workflows/    procedures you invoke: create-issue, issue, review-pr, open-pr,
                check-pr-comments, respond-to-pr-comments, ensure-migrations
  playbooks/    on-demand reference and techniques: run-app, grilling,
                understanding-check
  setup/        claude-code-agents.py (regenerates the committed Claude Code pointers)

docs/context/   architecture, glossary, invariants, state machines, API conventions,
                data model, known gaps, open questions
docs/adr/, docs/contracts/   decisions; client-facing traps per endpoint
.github/pull_request_template.md, .githooks/pre-push, scripts/ci.sh, Makefile   the PR and push gates
.claude/        committed Claude Code skills (workflows, playbooks) and agents (roles)
```

| Directory | What it is | How it is used |
|---|---|---|
| `roles/` | A specialist persona with a narrow job (reviewer, planner, implementer). Invoked *about* a change. | The agent reads the file and acts as it, or hands it to a subagent or a fresh chat |
| `workflows/` | A repeatable procedure with numbered steps, usually taking an argument. Invoked *by the user* by name. | The agent reads the file and follows the steps with the user's argument |
| `playbooks/` | Reference knowledge or a technique an agent pulls in on demand ("how to run this app", "how to run a human gate"). | The agent reads the file when the task, or a workflow that points at it, needs it |

If you're unsure: does the user type it? → workflow. Does it describe
*who* is working? → role. Does it describe *how something here works*,
or a technique a workflow points at? → playbook.

## Core ideas

1. **`AGENTS.md` is the contract.** Cursor, Copilot and Codex read it
   directly; Claude Code reaches it through the one-line `CLAUDE.md`.
2. **Never push to `master`** — every change is a PR from a fresh branch.
3. **Always work in a git worktree** — the primary checkout stays on
   `master`; agents edit in a worktree under `worktrees/` next to it,
   branched off a freshly fetched `origin/master`.
4. **Issue-first** — every change has a GitHub issue before work
   starts; the PR says `Closes #<n>`.
5. **Decisions before code** — the `issue` workflow settles open design
   choices with [`grilling`](./playbooks/grilling.md), one question at
   a time, before any code is written.
6. **Evaluator independence** — whoever wrote the code never reviews
   it. Spawn a reviewer subagent, or open a fresh session.
7. **The creator understands the PR** — before a new PR is created,
   [`understanding-check`](./playbooks/understanding-check.md) asks the
   creator about the diff and records the result at the top of the PR.
8. **Local CI is the gate** — `scripts/ci.sh` must be green before a PR
   is opened; the pre-push hook test-merges against fresh `master` and
   runs it on every push.
9. **Tests never touch your data** — the test suite deletes the
   database it connects to, so tests run only through `scripts/ci.sh`
   against `SMARTMETRO_TEST_DB` (`AGENTS.md` §2a).
10. **Tool output is evidence, not assumption** — empty or errored
    output means "unknown", never confirmation. A test that did not run
    is not a passing test.
11. **Rules describe the target; known gaps describe today** —
    `docs/context/known-gaps.md` lists where the existing code breaks
    the rules. New code never copies those patterns; each gap is fixed
    by its own issue.

## Model tiers and the delivery pipeline

A role's `reasoning` frontmatter is the model tier it runs on: `high`
is the strongest model your tool offers, `standard` the mid tier,
`fast` the smallest and quickest. The tiers are not a preference; they
define the delivery pipeline. Every workflow and role that writes or
reviews code follows this split, and says which tier runs each step.

| Tier | Who runs on it | Job |
|---|---|---|
| `fast` | `implementer`, `quick-fix` | Write the code and its tests. |
| `standard` | the specialist reviewers (`security-reviewer`, `domain-invariant-reviewer`, `test-coverage-reviewer`, `data-access-reviewer`, `architecture-guardian`, `spec-reviewer`) | Review the change, one role each. |
| `high` | `final-reviewer`, `orchestrator`, `feature-planner`, `penetration-tester` | Plan, coordinate, and give the final review and approval. |

**Implementation.** In autonomous delivery, code is written by
`fast`-tier `implementer` agents, not by the coordinating session. (In
interactive mode the user drives the code and the pipeline starts at
review.) Split the plan into slices that touch disjoint files, and run
one `implementer` per slice in parallel, each in its own worktree
branched from the feature branch. The coordinator merges the slices
back in plan order and runs the gate. A plan that cannot be split runs
one `implementer`.

**Review.** Each applicable specialist reviewer runs as its own
`standard`-tier agent, all in parallel, against the same diff. Then one
`high`-tier `final-reviewer` reads the diff and every reviewer report,
verifies each blocking finding against the code, and returns
`APPROVED` or `CHANGES REQUIRED`. No PR is opened, and no review is
reported as done, without that verdict.

**Fix loop.** On `CHANGES REQUIRED`, the coordinator hands the
blocking findings to `fast`-tier `implementer` agents (same slicing
rule), then re-runs the affected `standard` reviewers and the
`final-reviewer`. After two rounds without `APPROVED`, stop and report
to the user.

**Independence still holds** (`golden-principles.md` §12): no agent
reviews code it wrote, and the `final-reviewer` is never the session
that coordinated the implementation's fixes in the same context. Run
it as a fresh agent.

**Without per-agent model settings or subagents**, keep the same
sequence. Run each step in a fresh session on the closest model you
can choose: implementation first, then one session per reviewer, then
the final review. Say in the report which tier each step actually ran
on.

## Prerequisites (each team member, once)

1. **GitHub CLI**, signed in with access to the repo (issues and PRs
   both go through it):

   ```bash
   winget install GitHub.cli        # or: brew install gh / apt install gh
   gh auth login
   ```

2. **The pre-push hook**, once per clone (worktrees share it):

   ```bash
   make install-hooks               # or: git config core.hooksPath .githooks
   git config core.hooksPath        # must print .githooks
   ```

   The hook runs `scripts/ci.sh`, which fails while `AuthFlowTests` is
   red on `master` (known gap G2). Until G2 is fixed, expect the hook to
   stop code pushes; decide per push with the user, never bypass it
   silently (`AGENTS.md` §4).

3. **A throwaway test database**, exported in your shell profile:

   ```bash
   export SMARTMETRO_TEST_DB='Server=(localdb)\MSSQLLocalDB;Database=SmartMetro_Test;Trusted_Connection=True;TrustServerCertificate=True'
   ```

   Any SQL Server works; the database name must contain `test`, and
   nothing in it may matter: the tests delete it.

4. **dotnet-ef** for migrations: `dotnet tool install --global dotnet-ef`.

Every `gh` command in the harness passes
`--repo AshikMehediAbid/SmartMetroService`, so it works from any folder.
Keep that flag when you add a command.

## Using the harness

You never have to memorise the procedures: you ask the agent to run
one, and it reads the file and follows its numbered steps. There are
two ways to ask, and both run exactly the same file.

**1. Without any setup (works in every tool).** Open the repo in your
coding agent and say what you want, naming the file:

```text
Run harness/workflows/create-issue.md for: wallet balance goes negative on two quick purchases
Run harness/workflows/issue.md for 42
Run harness/workflows/review-pr.md for 18
Act as harness/roles/security-reviewer.md and review my branch against origin/master
Follow harness/playbooks/grilling.md and grill me on this plan: …
```

**2. With slash commands.** The same requests become:

```text
/create-issue wallet balance goes negative on two quick purchases
/issue 42
/review-pr 18
/harness check-pr-comments 18      # any workflow by name, including new ones
grill me on this plan: …           # playbooks load from what you say
```

Everything after the command name is the workflow's argument; inside
the workflow file it appears as `$ARGUMENTS`.

### The commands

| Command | Argument | What it does |
|---|---|---|
| `/create-issue` | what is broken or wanted, in a sentence | Checks GitHub for duplicates, drafts an issue, waits for your OK, then creates it |
| `/issue` | issue number, e.g. `42` | Reads the issue and the code, settles open design questions with you (grilling), implements in a worktree, runs the reviewers, runs the understanding check, then pushes and opens the PR after your OK |
| `/review-pr` | PR number | One `high`-tier reviewer walks every applicable role checklist against the PR's diff, checks it against the linked issue, and reports in the chat. Specialist agents only on request. Never posts |
| `/open-pr` | PR number | Opens each changed file of a PR as a diff tab in VS Code or Cursor. Read-only |
| `/check-pr-comments` | PR number (optional) | After a push: test-merges against `master`, lists unresolved review threads and check status, waits for running checks |
| `/respond-to-pr-comments` | PR number (optional) | Replies to and resolves review threads that are fixed or declined, then posts one summary comment, after your OK |
| `/ensure-migrations` | — | Checks a schema change has a generated EF Core migration, snapshot and mapping that agree, applied to a throwaway database |
| `/harness` | workflow name + argument | Runs any workflow in `harness/workflows/` by name |

Playbooks (`grilling`, `understanding-check`, `run-app`) and roles
(`security-reviewer`, `feature-planner`, …) are used *by* the
workflows; you can also ask for them directly ("grill me on …", "run
the app and call the fare endpoint", "spawn `security-reviewer` on my
branch").

### A typical change, end to end

1. `/create-issue <what you want>` → approve the draft → issue `#42`.
2. `/issue 42` → answer the agent's questions one at a time → approve
   the plan. The agent works in `../worktrees/<slug>`, never in your
   primary checkout.
3. The agent runs the reviewers in separate contexts and
   `scripts/ci.sh`, then asks you two or three questions about the diff
   (the understanding check). Answer them, approve the push, and it
   opens the PR with `Closes #42`.
4. When reviewers comment: `/check-pr-comments`, then
   `/respond-to-pr-comments` to fix, reply, resolve and summarise.
   The merge closes the issue; tell the agent, and it cleans up the
   worktree and runs `spec-reviewer` (`issue` workflow step 6).
5. Reviewing someone else's PR: `/review-pr <number>` in a **fresh**
   session.

The agent always stops and asks before any GitHub write (creating an
issue, pushing, opening a PR, posting a comment). Saying no is always
safe.

## Tool setup

### Claude Code: nothing to do

Everything is committed under `.claude/`, so a clone has it:

- `.claude/skills/<workflow>/SKILL.md` — `/issue`, `/review-pr`, … and
  `/harness`. Each says "read `harness/workflows/<name>.md` and follow
  it", so a changed workflow needs nothing.
- `.claude/skills/<playbook>/SKILL.md` — `grilling`, `run-app`,
  `understanding-check`. These load from what you say ("grill me", "run
  the app"), or by typing `/<name>`.
- `.claude/agents/<role>.md` — one subagent per role except
  `orchestrator` (a subagent cannot start subagents; run it in the main
  session). `reasoning: high` → Opus, `standard` → Sonnet, `fast` →
  Haiku; `tools` maps `read` → Read, `search` → Grep, Glob and Bash,
  `edit` → Edit and Write, `run` → Bash, `web` → WebFetch and
  WebSearch. Each agent is told never to write to GitHub and never to
  run tests outside `scripts/ci.sh`; a subagent cannot ask the user, so
  it reports what it would post and the calling session asks.

Run the workflows from an Opus main session; it coordinates and starts
the agents. Start a **new** session after pulling a change to
`.claude/`.

The playbook skills and role agents are generated. After adding,
renaming or removing a role or playbook, or changing its
`description`, `tools` or `reasoning`, run this and commit the result
in the same PR:

```bash
python harness/setup/claude-code-agents.py
```

They live in the repo, not in `~/.claude`, on purpose: personal agents
and skills apply to every project, and RestaurantOS uses the same role
names.

- **Bash is not read-only.** What stops a reviewer from writing through
  the shell is its instructions plus Claude Code's permission prompt
  for each command; a broad allow rule such as `Bash(gh:*)` or
  `Bash(git push:*)` removes the second.
- **Turn off prompt suggestions for the understanding check.** Claude
  Code suggests your next message from the conversation, so it can
  suggest the answer. Toggle *Prompt suggestions* in `/config`.

### Cursor

Cursor loads `.cursor/skills/<name>/SKILL.md` from the open workspace
and offers each one as `/<name>`. Run this once in Git Bash from the
primary checkout; it writes one pointer per workflow and keeps them out
of git:

```bash
for f in harness/workflows/*.md; do
  n=$(basename "$f" .md)
  mkdir -p .cursor/skills/"$n"
  cat > .cursor/skills/"$n"/SKILL.md <<EOF
---
name: $n
description: Run the SmartMetroService harness $n workflow. Only when the user types /$n.
disable-model-invocation: true
---

Read \`harness/workflows/$n.md\` in full before doing anything else, then
follow its numbered steps in order without skipping, merging, or
changing its output format. The text after /$n is \$ARGUMENTS.
If a step says stop, stop.
EOF
done
grep -qx '.cursor/skills/' .git/info/exclude 2>/dev/null || echo '.cursor/skills/' >> .git/info/exclude
```

`.git/info/exclude` is local, so `git add -A` will not commit the
pointers. Worktrees share the exclude file but not the files: copy
`.cursor/skills/` into a worktree if you open that worktree in Cursor.

### GitHub Copilot (VS Code)

Copilot Chat offers `.github/prompts/<name>.prompt.md` files as
`/<name>` commands (prompt files must be enabled in your VS Code
settings). Run this once from the primary checkout:

```bash
mkdir -p .github/prompts
for f in harness/workflows/*.md; do
  n=$(basename "$f" .md)
  cat > .github/prompts/"$n".prompt.md <<EOF
---
description: Run the SmartMetroService harness $n workflow
---

Read \`harness/workflows/$n.md\` in full before doing anything else, then
follow its numbered steps in order. The text I typed after /$n is the
workflow's argument (\$ARGUMENTS in that file). If a step says stop, stop.
EOF
done
grep -qx '.github/prompts/' .git/info/exclude 2>/dev/null || echo '.github/prompts/' >> .git/info/exclude
```

Use Copilot's agent mode, so it can read files and run commands.

### Check that the commands are enabled

1. Start a **new** session or chat (tools load commands at start).
2. Type `/`. The workflow names (`issue`, `review-pr`, …) should be in
   the list; in Claude Code also `harness`, `grilling`, `run-app` and
   `understanding-check`.
3. Run a read-only one to be sure: `/harness` with no argument lists
   the workflows; `/review-pr <an open PR number>` only reads.
4. Claude Code subagents: ask "which agents can you spawn?" — the role
   names should be listed.

## Adding a new command

A command is a workflow file. Adding one takes a PR to this repo and
nothing else.

1. **Decide it is a workflow.** The user types it and it has steps →
   workflow (`harness/workflows/`). A persona that reviews or builds →
   role (`harness/roles/`). Reference or technique another workflow
   uses → playbook (`harness/playbooks/`).
2. **Create `harness/workflows/<name>.md`** (kebab-case; the name is
   the command), with the frontmatter below, numbered steps with exact
   commands and what to do with each result, `$ARGUMENTS` validated in
   step 1, every GitHub write behind a human gate, and pointers to
   `AGENTS.md`, `golden-principles.md` and `docs/` instead of copied
   rules.
3. **Add its Claude Code skill**: copy `.claude/skills/issue/SKILL.md`
   to `.claude/skills/<name>/SKILL.md` and change `name`,
   `description`, `argument-hint` and the workflow path. (A new role or
   playbook instead: run `python harness/setup/claude-code-agents.py`.)
4. **List it** in *What's in the box* and the command table above, and
   in `AGENTS.md` §2; a new role also goes in `AGENTS.md` §5 and
   `roles/README.md`.
5. **Try it** without setup first: "run `harness/workflows/<name>.md`
   for <argument>" in a fresh session.
6. **Open a PR** (issue first, worktree, `/issue` like any change).

## Frontmatter schema

Neutral fields only; no tool-specific keys belong in these files.

```yaml
---
name: security-reviewer          # kebab-case, must match the filename
kind: role                       # role | workflow | playbook
description: >-                  # one or two sentences
  What this does and when to reach for it. Lead with the trigger.
tools: read, search, web         # roles only, optional: read, search, edit, run, web, delegate
reasoning: standard              # roles only, optional — high | standard | fast
arguments: issue number, e.g. 42 # workflows only, optional
---
```

## Writing rules

1. **Never name a specific coding tool in a role, workflow or playbook
   body** unless the point *is* that tool. Write "your tool's worktree
   helper, if it has one", not a tool's command name. GitHub and `gh`
   are the code host, not a coding tool, and are named freely. Tool
   names belong in this README.
2. **Don't assume subagents exist.** Where a file says "spawn X", it
   also says what a single-session tool does instead (a fresh chat per
   reviewer), so evaluator independence (`golden-principles.md` §12)
   still holds.
3. **Point at facts, don't copy them.** Rules live in `AGENTS.md` and
   `golden-principles.md`, decisions in `docs/adr/`, domain notes in
   `docs/context/`. A harness file names where to look; it does not
   restate a rule that can drift. Routes, DTOs, file trees and versions
   go nowhere: the code answers them.
4. **Cross-references stay relative** (`./respond-to-pr-comments.md`,
   `../playbooks/grilling.md`).
5. **"Ticket" means a metro ticket here.** The tracker item is an
   *issue*; keep the words apart in every harness file.
6. **A new or renamed file** is listed in this README and, for a role,
   in `AGENTS.md` §5; `spec-reviewer` checks both after merge.

A harness change is a change like any other: issue, worktree, PR.

## Deliberately left out (vs. the RestaurantOS harness)

- **Linear**: SmartMetroService has no Linear team; GitHub Issues is
  the tracker, so no MCP server is needed.
- **Multi-tenant rules** (tenant ids, RLS, store grants): this is a
  single-operator system. The equivalent here is per-user ownership.
- **Hand-written SQL migrations**: EF Core generates them here.
- **`api-docs` playbook and an OpenAPI drift check**: the app only maps
  `/openapi/v1.json` in Development and no copy is committed.
- **Tests in CI and `TreatWarningsAsErrors`**: added once known gaps
  G2/G3 are closed and the warning baseline is zero.
- **Personal `~/.claude` setup**: the Claude Code pointers are
  committed instead, so nothing collides with RestaurantOS.
- **CODEOWNERS and Dependabot**: one maintainer today; add them when
  the team grows.
