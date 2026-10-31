---
name: understanding-check
kind: playbook
description: >-
  Before creating a pull request, ask the person who wants it a few
  questions about this change, one at a time, and create it only when
  the answers show they understand it, or when they say "push anyway",
  which is then recorded at the top of the PR description. Use before
  every new pull request, from a workflow or an ad-hoc request. Not
  for pushing more commits to a PR that already exists.
---

# Understanding check

A pull request is a claim by its creator: "I know what this does and
I stand behind it." When an agent wrote most of the code, that claim
can be empty. This check makes the creator show it before the PR
exists, and records it on the PR either way.

It is a forcing function and a record, not proof. Anyone can open a
PR outside this check, and free-text answers can be gamed. What it
guarantees is that a PR created through the harness says whether its
creator passed.

## When

- Before creating any new pull request: the `issue` workflow, an
  autonomous `orchestrator` run, or a user asking for a PR directly.
- Once per change.
- Not for later pushes to an existing PR, and not for a PR someone
  else will create.

The creator is the person who asked for the PR and whose GitHub
account creates it. In an unattended run with nobody to answer, stop
before creating the PR and report that it waits for this check.

If the tool suggests the user's next message from the conversation,
that suggestion can contain the answer. Ask the creator to turn it off
for the check and on again after it.

## Questions

Read the diff against `origin/master` and the ticket first. Ask **two or
three questions, one per message**, each specific to this change. One
question is enough for a docs-only or one-line change. Choose from:

- **What and why:** what the change does, in the creator's words, and
  which problem in the ticket it solves.
- **What could break:** a concrete case the change touches ("another
  user's ticket id", "the same AamarPay success callback twice", "a
  migration on a table that already has rows") and what now happens there; the blast radius
  if it is wrong.
- **How it is verified:** which test fails without the change, or how
  it was seen working.

Rules for asking:

- **Do not hand over the answer.** No recommended answer, no summary
  of the change before the questions, no yes/no question. Grilling
  recommends answers because there the user decides; here the user
  explains.
- A question the PR title or ticket answers word for word tests
  nothing. Ask about the code.
- **Show the change you ask about.** When a question or follow-up
  refers to a specific change, put its diff hunk in the same message,
  with enough context to answer from, as the PR would show it. For an
  unchanged line, show the lines around `file:line`. The creator
  should never have to look the change up. Show the code, not your
  reading of it.
- Wait for each answer before the next question.

## Judging

An answer passes when it is **correct against the code and specific
to this change**. Check it against the diff, not against what you
remember writing. Where your tool can, have a fresh session or a
subagent that did not write the code judge each answer from the diff,
the question and the answer: whoever wrote the code is a lenient
grader.

An incomplete first answer is normal, not a failure. When part of an
answer is missing or wrong:

- Say which part holds and which part is missing or wrong, without
  giving the missing part away.
- Ask a follow-up aimed at exactly that gap ("you said the other
  user gets a 404; what does the query at this line return
  for them?"). For a wrong claim, point at the code to look at, by
  `file:line`, show those lines, and ask again.
- Repeat as often as it takes. There is no limit on tries, and no
  number of tries counts against the creator or is recorded.
- If the creator asks for the answer, or says a suggestion or another
  tool gave it to them, give or accept it, then ask a new question
  about a different point of the change: the given one no longer
  shows anything.

The check passes when every question has been answered correctly,
however many follow-ups it took.

## Outcome

**Passed:** create the PR. Put this line at the very top of the body,
then a blank line, then the template (it starts with **In short:**).
Add nothing else to the line: the PR already shows who created it and
when:

```markdown
> 🧠✅
```

**Not passed yet:** do not create the PR; keep asking follow-ups. Only
"push anyway" ends the check without a pass. Mention it once, when the
creator asks how to skip or stops answering, never as part of a
question. Never create the PR on any other wording: "fine" or "just
do it" is not an override, so ask.

**Override:** on "push anyway", create the PR with this block at the
very top of the body:

```markdown
> ⚠️ Understanding check overridden ("push anyway"): <creator>, <YYYY-MM-DD>.
> Not shown: <one line per point still missing or wrong>.
```

This is the only outcome that marks a question as not answered.

The creator is the GitHub account the PR is created under. The block
stays in the body: do not remove it in a later edit. Keep it short;
the rest of the body still follows `.github/pull_request_template.md`.
Leave a blank line after the block (or after the passed line), or the
**In short:** line below it renders inside the quote.
