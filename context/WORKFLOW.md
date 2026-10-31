---
type: workflow
title: AIDLC workflow
description: Requirement-to-implementation process for AI-assisted SmartMetroService development.
tags: [aidlc, workflow]
spec_version: 1
last_review: 2026-10-01
---
# AIDLC Workflow

This repository uses a lightweight, spec-led lifecycle inspired by Context-Boilerplate. No slash commands, hooks, phase automation, or external issue tracker integration are installed by this context bundle.

## Change lifecycle

1. **Frame**: read the loading map and relevant business context. Capture the user problem, scope, non-goals, and acceptance criteria in plain language. Resolve any blocking **Confirm** items.
2. **Design**: identify API contracts, domain rules, persistence changes, integrations, authorization, and failure cases. Record meaningful lasting decisions in `04-architecture/` after agreement.
3. **Plan**: create a phase under `06-phase/` for a coherent delivery increment. Break requirements into small tasks with explicit acceptance criteria and test gates. Do not create a phase for trivial work unless requested.
4. **Implement**: work one task at a time. Test behavior, preserve layer boundaries, add migrations for schema changes, and avoid unrelated refactors.
5. **Verify**: run focused tests and the relevant broader gate. Check API and security behavior as part of the change; report commands and outcomes honestly.
6. **Reconcile**: update feature technical/test contexts and phase task status. Update business requirements only when scope or accepted behavior changed. Close a phase only when its acceptance criteria and required gates pass or exceptions are explicitly recorded.

## Writing contract

- Business context states user value, rules, and acceptance criteria without prescribing implementation.
- Technical context records the public behavior, implementation map, dependencies, and unresolved design points.
- Test context maps acceptance criteria to existing and required tests.
- Avoid duplicating details across features; expose cross-feature dependencies through the technical `## Public surface` section.
- Context is a guide, not a substitute for checking code. Correct stale paths or behavior when discovered.
