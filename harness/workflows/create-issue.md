---
name: create-issue
kind: workflow
description: >-
  File a bug or feature issue on GitHub from a short description: check
  for duplicates, gather evidence, draft it for human approval, then
  create it with the right labels. Use it for the "issue first" rule
  before a change starts.
arguments: what is broken or wanted, in a sentence; optionally the type
---

Given a description in `$ARGUMENTS` (e.g. `/create-issue wallet balance
goes negative when two tickets are bought at once`), draft an issue, get
it approved, then create it. This workflow only files the issue; to
implement it afterwards, run [`issue`](./issue.md) with the new number.

GitHub Issues in `AshikMehediAbid/SmartMetroService` is the tracker
(`AGENTS.md` §0 rule 3). `REPO` is `AshikMehediAbid/SmartMetroService`;
every `gh` call passes `--repo "$REPO"`.

## Step 1 — Look for an existing issue

Search titles and bodies for two or three key words before drafting
anything, open and closed:

```bash
gh issue list --repo "$REPO" --state all --search "<key words>" --limit 20 \
  --json number,title,state,labels,url
```

Open each plausible hit (`gh issue view <n> --repo "$REPO" --comments`).
If one already covers the case, report it and stop: add to it (with
approval) rather than filing a duplicate. Also check
`docs/context/known-gaps.md` and `docs/context/open-questions.md`: the
case may already be a known gap (`G<n>`) or an open question (`Q<n>`),
and the issue should say which.

## Step 2 — Gather evidence

Read-only. Verify the claim before you write it down:

- **Defect:** how it shows (request and response, log line, failing
  test), and the code path that causes it, checked in a checkout of
  fresh `origin/master`. Say when the cause is assumed rather than
  found. Never run the test suite outside `scripts/ci.sh`.
- **Change:** what the user wants, who it is for (a passenger, an admin,
  gate staff, the team), why, what success looks like and what is out
  of scope. Ask for whichever is missing, one question at a time with a
  recommended answer, as in [`grilling`](../playbooks/grilling.md)
  *Start with the goal*. Don't make one up. Design questions wait for
  the `issue` workflow.

Never copy personal data (names, phone numbers, e-mail addresses),
passwords, OTPs, tokens, AamarPay credentials or connection strings into
an issue. Issues on this repo are public if the repo is.

## Step 3 — Pick the type

| Situation | Label | Body template | Branch type later |
|---|---|---|---|
| Something that worked, or should work, is broken | `bug` | Bug template below | `fix/` |
| New behaviour | `enhancement` | Feature template below | `feat/` |
| A change to existing behaviour, a cleanup | `enhancement` | Feature template below | `refactor/`, `chore/` |
| Docs or harness only | `documentation` | Feature template below | `docs/` |

Use existing labels only (`gh label list --repo "$REPO"`); never create a
label without the user asking.

## Step 4 — Draft

Plain language, written for a teammate who has not seen this
conversation. Short paragraphs, no restating of the code.

**Title:** what is wrong or wanted, from the caller's point of view.
Prefix the area when it isn't obvious (`Wallet:`, `Payment:`,
`Scanner:`, `Station:`, `Account:`, `Docs:`).

**Bug body (markdown):**

```markdown
**Where:** endpoint or component, how often.

**Steps**
1. …

**What happens:** … (and the code path, when known)

**Expected:** …

**Why it matters:** who is affected and how badly (a passenger charged twice, a free ride, another user's data).

**Related:** known gap G<n> / open question Q<n>, or `none`.

**Out of scope:** … (only when the fix could grow into more)
```

**Feature body (markdown):**

```markdown
<the change>

**For:** who benefits (a passenger, an admin, gate staff, the team). **Why:** what it fixes or enables.

**Acceptance criteria**
- [ ] an outcome someone can check, not an implementation step
- [ ] …

**Related:** known gap G<n> / open question Q<n>, or `none`.

**Out of scope:** what this issue will not do, or `none`.
```

The `issue` workflow's grilling reads the who, why, acceptance criteria
and out-of-scope lines and asks only for what is missing.

## Step 5 — Human gate

**STOP.** Show in the conversation: title, the body as it will read, and
labels. Wait for approval of the wording. Creating the issue is a
GitHub write, so it needs this confirmation even when the "issue first"
rule triggered the workflow.

## Step 6 — Create

Write the body to a file outside the repo, then:

```bash
gh issue create --repo "$REPO" --title "<title>" --body-file <body.md> --label "<label>"
```

The command prints the issue URL; the number is its last path segment.

## Step 7 — Verify and report

Read it back (`gh issue view <n> --repo "$REPO"`) and check title, body
and labels; fix anything wrong with `gh issue edit` after showing the
change. Then give the user the number and URL, and remind them that the
branch is `<type>/<n>-<slug>`, commits carry `Refs #<n>`, and the PR
body says `Closes #<n>`. If the issue closes a known gap, say that the
PR must also remove its row from `docs/context/known-gaps.md`.
