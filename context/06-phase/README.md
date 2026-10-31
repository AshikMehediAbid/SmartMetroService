---
type: phase-index
title: Delivery phases
description: Phase planning convention; no historical or active phase is inferred from the current codebase.
tags: [phases, planning]
spec_version: 1
last_review: 2026-10-01
---
# Delivery Phases

No active or completed AIDLC phase was documented when this context bundle was created. Do not invent project history or mark existing code as a phase delivery.

For a new scoped increment, copy `_template/requirement.md` and `_template/tasks.md` into `phase-NNN-short-name/`. Add `outcome.md` when completed with delivered scope, verification results, exceptions, and follow-up items.

Keep phase requirements immutable after implementation starts; record agreed changes as a dated amendment. Mark a phase complete only when acceptance criteria and required gates pass or the owner accepts documented exceptions.
