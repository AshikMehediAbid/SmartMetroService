---
name: grilling
kind: playbook
description: >-
  Interview the user one question at a time until every design
  decision a change depends on is settled. Use at a human gate before
  any product code is written (the issue workflow's step 3), or when
  the user asks to be grilled or to stress-test a plan. Not for
  reviewing someone else's pull request.
---

# Grilling

Resolve every decision a plan depends on before acting on it. Treat
the plan as a **tree of decisions**: each decision opens the ones that
depend on it.

## When to use it, and when not to

- **Use it** when a ticket leaves real design choices open: more than
  one plausible approach, an ambiguous acceptance criterion, a
  client-visible edge case nobody has decided, or a product rule the
  code does not settle (`docs/context/open-questions.md`).
- **Skip it** for a tiny, unambiguous fix. Present the analysis and
  get a quick confirmation instead. Do not manufacture questions.
- **Not for reviewing a pull request someone else wrote.** The
  decisions there belong to the author, so the user can only guess.
  An open question about a reviewed change becomes a review comment
  for the author, not a question.

## Facts are yours, decisions are the user's

Before each question, look up every fact it needs: the code, its
tests, `docs/adr/`, `docs/context/`, `docs/contracts/`, and the GitHub
issue with its comments.
Never ask the user for something the code or a tool can answer. Where
your tool can run a lookup in parallel (a subagent, a background
search), start it and ask a question that does not depend on it while
it runs.

Label what you present as **known** (a documented decision, a design
source, or a human's stated intent), **assumed** (inferred from nearby
code) or **unknown**. Only unknowns that need a human's judgement
become questions. An assumption the user would want to overrule
becomes a question too. A question already listed in
`docs/context/open-questions.md` is quoted by its id (`Q3`, `G14`).

## Start with the goal

The root of the tree is what the change is for. Before any design
question, check that the ticket says who it is for, why it is wanted,
what success looks like and what is out of scope. A ticket filed by
the `create-issue` workflow states all four. A bug covers them with
Expected and Why it matters, plus out of scope when the fix could
grow. Ask only for what is missing or vague ("make it robust", "the
usual way"), and ask it first: every design decision below depends on
it.

## One question at a time

The **frontier** is every open decision whose prerequisites are
settled: the questions you can ask now without guessing at an answer
you have not heard. Ask one question from it per message, starting
with the one the most other decisions depend on. One answer often
changes the next question, and a list invites skimming.

Number the questions through the session and give your recommended
answer, worded so that "yes" accepts it:

```text
Q3 — <title>
<the question, with the options and what each one costs>
Recommended: <answer, and the one-line reason>
```

Then stop and wait for the answer. Do not continue on an assumption
because the user is slow to reply. Each answer settles part of the
tree and moves the frontier: recompute it and ask the next question.
"Whatever you think" accepts the recommendation for that question
only.

**Stop when it is stuck.** When three answers in a row have settled
nothing (each opens more than it closes, or you still cannot predict
the user's answer to the next question), say so. Name what seems to
be missing and propose stepping back to the goal or to a smaller
scope instead of asking another question.

## Words and claims

- When the user uses a term that conflicts with
  `docs/context/domain-glossary.md`, say so in the next question ("the
  glossary says the basket is an open order; you seem to mean a
  separate cart. Which one?").
- When a term is vague or overloaded (ticket or RapidPass? fare or
  amount paid? wallet balance or payment?), propose the precise one.
- When the user states how something works, check the code. A
  contradiction becomes a question, quoted with `file:line`.
- When the user states how AamarPay or Keycloak behaves, check the code
  that calls it. Behaviour the code does not show is an assumption to
  confirm, not a fact.
- Probe boundaries with concrete scenarios: two users and one ticket id,
  a wallet with exactly the fare in it, the same AamarPay success
  callback twice, an entry scan with no exit, an expired ticket inside
  the gates, two concurrent purchases. A scenario the plan
  cannot answer is a new branch of the tree.

## Done

The session is done when the frontier is empty: every branch visited,
nothing silently assumed. Then summarise the settled decisions in one
list that ends with an **Out of scope** line, since half of a
misunderstanding is what is not being built. Ask the user to confirm
it. Only an explicit yes counts: "sounds good", "whatever you think"
or moving on without answering are not confirmation, so ask what they
would change. Act only after that yes.

Write nothing during the session. List what the decisions call for,
and write it in the implementation step under the rules of the
workflow you are running:

- a decision that was contested (an ADR in `docs/adr/`);
- a client-facing route whose behaviour changes (a contract in
  `docs/contracts/`);
- an open question the session answered (close or update it in
  `docs/context/open-questions.md`);
- a goal or out-of-scope line the ticket lacked (an edit to the ticket,
  which is a GitHub write and needs the user's approval like any
  other). With no ticket yet, the summary is what the `create-issue`
  workflow drafts from.

A term the session settled that `docs/context/domain-glossary.md`
lacks or gets wrong goes in the final report as a suggestion. Do not
change the glossary as part of the ticket.

<!--
Adapted from the `grilling` skill in https://github.com/mattpocock/skills

MIT License

Copyright (c) 2026 Matt Pocock

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
-->
